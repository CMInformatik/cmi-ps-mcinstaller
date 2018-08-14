function Remove-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High', DefaultParameterSetName = "__AllParameterSets")]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias('FullName', 'Path')]
        [String]$ConfigurationFile,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String[]]$Name
    )
    Process {
        # Konfiguration lesen
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop

        foreach ($n in $Name) {
            # Mandant entfernen
            if (-not (HasProperty $data.$n)) {
                Write-Error "Tenant $n not found"
            }
            if ($PSCmdlet.ShouldProcess($ConfigurationFile, "Remove $n")) {
                $data.PSObject.Properties.Remove($n)
            }
        }
        # Konfiguration schreiben
        $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
    }
}
