function New-Configuration {
    <#
    .SYNOPSIS
        Creates a new empty configuration.
    .DESCRIPTION
        Creates a new empty configuration.
        When the operation fails, an output on the error stream will be generated.
    .EXAMPLE
        PS C:\> New-Configuration
        Returns a new empty configuration.
    .INPUTS
        None.
    .OUTPUTS
        A JSON configuration object.
    #>
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
