function MustBeFolder {
<#
.EXAMPLE
[parameter( Position = 1, ValueFromPipeline = $True)]
[ValidateScript( { MustBeFolder $_ })]
[String]$Path
    #>
    PARAM(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string]$Path,
        [Parameter(Mandatory = $false)]
        [AllowNull()]           
        [string]$ErrMsg = $null
    )
    
    if ([string]::IsNullOrWhiteSpace($Path)) {
        Throw [System.Management.Automation.ValidationMetadataException] "Ein Verzeichnispfad muss angegeben werden."
    }
    if (Test-UNCPath $Path -IsFolder) {
        return $true;
    }
    if ([string]::IsNullOrWhiteSpace($ErrMsg)) {
        $ErrMsg = "'$Path' ist kein gueltiges Verzeichnis."
    }
    Throw [System.Management.Automation.ValidationMetadataException] $ErrMsg
}