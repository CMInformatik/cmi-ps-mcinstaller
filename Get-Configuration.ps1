function Get-Configuration {
    <#
    .SYNOPSIS
        Reads configurations from JSON files.
    .DESCRIPTION
        Reads configurations from JSON files.
        When a JSON file can not be processed, an output on the error stream will be generated.
    .EXAMPLE
        PS> Get-Configuration -ConfigurationPath "C:\data\config.mandanten.json"
        Returns a JSON configuration object.
    .EXAMPLE
        PS> Get-Configuration -ConfigurationPath "C:\data\config1.json","C:\data\config2.json","C:\data\config3.json"
        Returns an array of JSON configuration objects.
    .OUTPUTS
        A list of JSON configuration objects.
    .PARAMETER ConfigurationPath
        Array of file paths to configurations files. Tries to resolve relative paths.
    .PARAMETER Schema
        The configuration model.
    #>
    [CmdletBinding(SupportsShouldProcess = $false)]
    [OutputType([JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias("Path", "FullName")]
        [string[]]$ConfigurationPath,

        [parameter(Mandatory = $False, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Schema]$Schema=$DefaultSchema
    )
    Process {
        foreach ($path in $ConfigurationPath) {
            try {
                $fullName = (Get-Item -Path $path -ErrorAction Stop).FullName
                Write-Verbose "Reading configuration from $fullName"
                [cmi.mobileclients.config.JsonConfiguration]$c = [JsonConfiguration]::ReadFromFile($fullName, $Schema)
                Write-Output $c
            }
            catch {
                Write-Error $_.Exception -TargetObject $fullName
            }
        }
    }
}
