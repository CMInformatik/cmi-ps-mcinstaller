function Add-App {
    <#
    .SYNOPSIS
        Adds apps to tenants.
    .DESCRIPTION
        Adds apps to tenants.
        When a tenant or app can not be processed, an output on the error stream will be generated.
    .EXAMPLE
        PS> Add-App -Configuration $config -App Dossierbrowser,Sitzungsvorbereitung -EnsureDependencies
        Adds the given app to all tenants in the configuration.
    .EXAMPLE
        PS> Add-App -Configuration $config -App Dossierbrowser -TenantName cmi -EnsureDependencies
        Adds the app 'Dossierbrowser' to the tenant 'cmi'.
    .OUTPUTS
        Passthru off: The app object or array of app objects.
        Passthru on: The configuration object.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    .PARAMETER App
        List of apps to add to tenants.
        Does nothing, when an app is already present on the tenant.
    .PARAMETER TenantName
        List of tenants names to operate on.
        When not present, the operation will be applied on all tenants in the configuration.
    .PARAMETER Tenant
        List of tenants objects to operate on.
    .PARAMETER EnsureDependencies
        When adding an app to a tenant, change depending aspects of the configuration as required for the app.
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
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App[]]$App,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies,

        [parameter(Mandatory = $False, Position = 4, ParameterSetName="ByConfiguration")]
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

        # enable app
        foreach($t in $Tenant){
            try{
                foreach($a in $App){
                    if(!$t.Has($a)){
                        if($PSCmdlet.ShouldProcess($Configuration, "Add app $a to tenant $($t.Name)")){
                            $t.Add($a, $EnsureDependencies)
                        }
                    }
                    if(-not $Passthru){
                        Write-Output $t[$a]
                    }
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
