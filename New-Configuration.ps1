function New-Configuration {
    [CmdletBinding(SupportsShouldProcess = $false, ConfirmImpact='None')]
    [OutputType([JsonConfiguration])]
    Param()
    Process {
        try {
            $c = [JsonConfiguration]::CreateInstance($Schema)
            return $c
        }
        catch {
            Write-Error $_.Exception
        }
    }
}
