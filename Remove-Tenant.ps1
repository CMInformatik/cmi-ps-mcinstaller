function Remove-Tenant {
    <#
    .SYNOPSIS
        Removes tenants from the configuration.
    .DESCRIPTION
        Removes tenants from the configuration.
        When a tenant can not be processed, an output on the error stream will be generated.
    .EXAMPLE
        PS C:\> Remove-Tenant -Configuration $config -TenantName cmi,talus
        Removes the tenants 'cmi' and 'talus' from the configuration.
    .OUTPUTS
        Passthru off: None.
        Passthru on: The configuration object.
    .PARAMETER TenantName
        List of tenant names to remove.
        When not present, the operation will be applied on all tenants in the configuration.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High')]
    [OutputType([void], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $false, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $False, Position = 2)]
        [switch]$Passthru
    )
    Process {
        if(-not $TenantName){
            # select all tenants
            $TenantName = $Configuration.Tenants | Select-Object -ExpandProperty Name
        }

        # remove tenant
        foreach($name in $TenantName){
            try{
                $tenant = $Configuration.GetTenant($name)
                if($tenant){
                    if($PSCmdlet.ShouldProcess($Configuration, "Remove tenant $name")){
                        $Configuration.RemoveTenant($name)
                    }
                }
            }
            catch{
                Write-Error $_.Exception
            }
        }
        if($Passthru){
            Write-Output $Configuration
        }
    }
}
