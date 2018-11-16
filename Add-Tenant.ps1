function Add-Tenant {
    <#
    .SYNOPSIS
        Adds tenants to a configuration.
    .DESCRIPTION
        Adds tenants to a configuration.
        When a tenant can not be processed, an output on the error stream will be generated.
    .EXAMPLE
        PS> Add-Tenant -Configuration $config -TenantName cmi,talus,abraxas
        Adds the tenants cmi, talus and abraxas to the configuration.
    .OUTPUTS
        Passthru off: The tenant object or array of tenant objects.
        Passthru on: The configuration object.
    .NOTES
        General notes
    .PARAMETER TenantName
        Array of tenant names to be added to the configuration.
        Does nothing, when a tenant with the same name is already present.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium')]
    [OutputType([Tenant], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,

        [parameter(Mandatory = $False, Position = 2)]
        [switch]$Passthru
    )
    Process {
        foreach($name in $TenantName){
            try{
                $tenant = $Configuration.GetTenant($name)
                if(!$tenant){
                    if($PSCmdlet.ShouldProcess($Configuration, "Add tenant $name")){
                        $Configuration.AddTenant($name) | Out-Null
                    }
                }
                if(-not $Passthru){
                    Write-Output $Configuration[$name]
                }
            }
            catch{
                Write-Error $_.Exception -TargetObject $name
            }
        }
        if($Passthru){
            Write-Output $Configuration
        }
    }
}