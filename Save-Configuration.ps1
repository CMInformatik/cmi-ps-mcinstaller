function Save-Configuration {
    <#
    .SYNOPSIS
        Saves the configuration to a JSON file.
    .DESCRIPTION
        Saves the configuration to a JSON file.
        Per default, an existing file will not be overwritten.
        When the operation fails, an output on the error stream will be generated.
    .EXAMPLE
        PS C:\> <example usage>
        Explanation of what the example does
    .OUTPUTS
        Passthru off: None.
        Passthru on: The configuration object.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER ConfigurationPath
        File paths to configurations files. Tries to resolve relative paths.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    .PARAMETER Force
        An existing file will be overwritten.
    #>
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact='High')]
    [OutputType([void], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [Alias("Path", "FullName")]
        [string]$ConfigurationPath,

        [parameter(Mandatory = $False, Position = 2)]
        [switch]$Force,

        [parameter(Mandatory = $False, Position = 3)]
        [switch]$Passthru
    )
    Process {
            try {
                # resolve relative paths
                if([System.IO.Path]::IsPathRooted($ConfigurationPath)) {
                    $fullName = $ConfigurationPath
                } else {
                    $fullName = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ConfigurationPath))
                }
                Write-Verbose "Saving configuration to $fullName"

                # write configuration
                $tenants = [string]::Join(',', ($Configuration.Tenants | Select-Object -ExpandProperty Name))
                if ($PSCmdlet.ShouldProcess($fullName, "Save $tenants")) {
                    $Configuration.WriteToFile($fullName, $Force)
                }
                if($Passthru){
                    Write-Output $Configuration
                }
            }
            catch {
                Write-Error $_.Exception -TargetObject $fullName
            }
    }
}
