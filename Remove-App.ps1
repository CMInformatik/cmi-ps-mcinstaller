function Remove-App {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High', DefaultParameterSetName="__AllParameterSets")]
    [OutputType([Tenant])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App
    )
    Process {
        foreach($name in $TenantName){
            try{
                $tenant = $Configuration[$name]
                if($tenant.Has($App)){
                    if($PSCmdlet.ShouldProcess($Configuration, "Remove app $App from tenant $name")){
                        $tenant.Remove($App)
                    }
                }
                Write-Output $tenant
            }
            catch{
                Write-Error $_.Exception
            }
        }
    }
}
