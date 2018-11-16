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
        ToDo
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    .PARAMETER TenantName
        List of tenant names to remove.
        When not present, the operation will be applied on all tenants in the configuration.
    .PARAMETER Value
        The value to set.
    .PARAMETER AspectPath
        JPaht like path of the aspect to configure.
    .PARAMETER EnsureDependencies
        When setting the aspect to a tenant, change depending aspects of the configuration as required for this aspect.
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName = "__AllParameterSets")]
    [OutputType([Tenant])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,
    
        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,
    
        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $True, Position = 3, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$AspectPath,

        [parameter(Mandatory = $True, Position = 4, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [object]$Value,

        [parameter(Mandatory = $False, Position = 5, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies
    )
    Process {
        foreach($name in $TenantName){
            try{
                #ToDo: SupportsShouldProcess
                $tenant = $Configuration[$name]
                Write-Verbose "$($tenant.Name).$App.$AspectPath = $Value"
                if(!$tenant.Has($App)){
                    Write-Error "App '$App' is not enabled for tenant $($tenant.Name)" -TargetObject $Configuration
                }
                $tenant[$App].Set($AspectPath, $Value, $EnsureDependencies)
                Write-Output $tenant[$App]
            }
            catch{
                Write-Error $_.Exception
            }
        }
    }
}