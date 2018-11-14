function Save-Configuration {
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
                $tenants = [string]::Join(',', ($Configuration | Select-Object -ExcludeProperty Name))
                if ($PSCmdlet.ShouldProcess($ConfigurationPath, "Save $tenants")) {
                    $Configuration.WriteToFile($ConfigurationPath, $Force)
                }
                if($Passthru){
                    Write-Output $Configuration
                }
            }
            catch {
                Write-Error $_.Exception -TargetObject $ConfigurationPath
            }
    }
}
