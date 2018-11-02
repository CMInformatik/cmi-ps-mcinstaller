function Get-Configuration {
    [CmdletBinding(SupportsShouldProcess = $false, DefaultParameterSetName = "__AllParameterSets")]
    [OutputType([JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias("Path", "FullName")]
        [string[]]$ConfigurationPath
    )
    Process {
        foreach ($path in $ConfigurationPath) {
            try {
                [cmi.mobileclients.config.JsonConfiguration]$c = [JsonConfiguration]::ReadFromFile($path, $Schema)
                Write-Output $c
            }
            catch {
                Write-Error $_.Exception
            }
        }
    }
}
