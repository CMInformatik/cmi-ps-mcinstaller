function Set-Aspect {
    <#
    .SYNOPSIS
        Sets an aspect of the configuration to the specified value.
    .DESCRIPTION
        Sets an aspect of the configuration to the specified value.
        When the operation fails, an output on the error stream will be generated.
    .EXAMPLE
        ToDo
    .OUTPUTS
        Passthru off: The tenant object or array of tenant objects.
        Passthru on: The configuration object.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    .PARAMETER TenantName
        List of tenants names to operate on.
        When not present, the operation will be applied on all tenants in the configuration.
    .PARAMETER Tenant
        List of tenants objects to operate on.
    .PARAMETER Value
        The value to set.
        When the value is null, the default value defined be the configuration schema will be used.
    .PARAMETER App
        The hosting app of the aspect.
    .PARAMETER AspectPath
        JPaht like path of the aspect to configure.
    .PARAMETER EnsureDependencies
        When setting the aspect to a tenant, change depending aspects of the configuration as required for this aspect.
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium')]
    [OutputType([AppConfiguration], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName="ByConfiguration")]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName="ByTenant")]
        [ValidateNotNull()]
        [Tenant[]]$Tenant,

        [parameter(Mandatory = $False, Position = 1, ValueFromPipelineByPropertyName = $True, ParameterSetName="ByConfiguration")]
        [AllowNull()]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,
    
        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $True, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [string[]]$AspectPath,

        [parameter(Mandatory = $False, Position = 4, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [object]$Value,

        [parameter(Mandatory = $False, Position = 5, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies,

        [parameter(Mandatory = $False, Position = 6, ParameterSetName="ByConfiguration")]
        [switch]$Passthru
    )
    Process {

        if($PSCmdlet.ParameterSetName -eq 'ByConfiguration'){
            # get tenant object by name
            $Tenant = @()

            # if tenant name is null, select all tenants
            if(-not $TenantName){
                $Tenant = $Configuration.Tenants
            }
            else {
                foreach($name in $TenantName){
                    $t = $Configuration.GetTenant($name)
                    if($null -eq $t){
                        Write-Error "A tenant with name '$name' was not found in the configuration" -TargetObject $Configuration
                    } else {
                        $Tenant += $t
                    }
                }
            }
        }

        # set aspect
        foreach($t in $Tenant){
            try{
                if(!$tenant.Has($App)){
                    Write-Error "App '$App' is not enabled for tenant $($t.Name)" -TargetObject $t
                }
                foreach($path in $AspectPath){
                    if($PSCmdlet.ShouldProcess("$($t.Name).$App.$path", $Value)){
                        if($null -ne $Value){
                            $t[$App].Set($path, $Value, $EnsureDependencies)
                        } else {
                            $t[$App].Set($path, [bool]$EnsureDependencies)
                        }
                    }
                }
                if(-not $Passthru){
                    Write-Output $t
                }
            }
            catch{
                Write-Error $_.Exception -TargetObject $t
            }
        }

        if($Passthru){
            Write-Output $Configuration
        }
    }
}