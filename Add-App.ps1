function Add-App {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName="__AllParameterSets")]
    [OutputType([AppConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies
    )
    Process {
        foreach($name in $TenantName){
            try{
                $tenant = $Configuration[$name]
                if(!$tenant.Has($App)){
                    if($PSCmdlet.ShouldProcess($Configuration, "Add app $App to tenant $name")){
                        $tenant.Add($App, $EnsureDependencies)
                    }
                }
                Write-Output $tenant[$App]
            }
            catch{
                Write-Error $_.Exception
            }
        }
    }
}
