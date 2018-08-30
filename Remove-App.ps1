function Remove-App {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High', DefaultParameterSetName="__AllParameterSets")]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias('FullName', 'Path')]
        [String]$ConfigurationFile,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [cmi.ps.mcschema.App]$App
    )
    Process {
        # Datei lesen
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop

        foreach($tenant in $TenantName){
            if(-not ($data | HasProperty -Name $tenant)){
                Write-Error "Tenant $tenant could not be found in $ConfigurationFile" -TargetObject $tenant
                continue
            }
            if(-not ($data | HasProperty -Path "$tenant.$App")) {
                Write-Verbose "$tenant does not have the app $App. No action required."
                continue
            }

            if($data | HasProperty -Path "$tenant.common.appDirectory.$App"){
                $data.$tenant.common.appDirectory.PSObject.Properties.Remove($App)
            }
            $data.$tenant.PSObject.Properties.Remove($App)

            if($PSCmdlet.ShouldProcess($ConfigurationFile, "Remove $tenant.$App")){
                $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
            }
        }
    }
}
