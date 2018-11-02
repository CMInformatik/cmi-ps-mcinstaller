function Remove-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High', DefaultParameterSetName = "__AllParameterSets")]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String[]]$TenantName
    )
    Process {
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
    }
}
