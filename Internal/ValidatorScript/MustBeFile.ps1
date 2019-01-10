function MustBeFile {
    <#
.EXAMPLE
[parameter( Position = 1, ValueFromPipeline = $True)]
[ValidateScript( { MustBeFile $_ })]
[String]$Path
    #>
    PARAM(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string]$Path,
        [AllowNull()]
        [Parameter(Mandatory = $false)]
        [string]$ErrMsg = $null
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        Throw [System.Management.Automation.ValidationMetadataException] "Ein Dateipfad muss angegeben werden."
    }
    if (Test-Path $Path -PathType Leaf) {
        return $true;
    }
    if ([string]::IsNullOrWhiteSpace($ErrMsg)) {
        $ErrMsg = "'$Path' ist keine gueltige Datei."
    }
    Throw [System.Management.Automation.ValidationMetadataException] $ErrMsg
}