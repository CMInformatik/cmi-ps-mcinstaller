#Requires -PSEdition Desktop

# Konfigurationsmodell Definition
Add-Type -TypeDefinition (Get-Content .\Model\Schema.cs -ErrorAction Stop -Raw) -ErrorAction Stop

# Konfigurationsmodell laden
[System.Collections.Generic.Dictionary[[string], [CMI.PS.AppSection]]]$sections = (New-Object 'System.Collections.Generic.Dictionary`2[System.String,CMI.PS.AppSection]')
foreach ($app in [System.Enum]::GetNames( [CMI.PS.App])) {
    $sections.Add($app, (New-Object CMI.PS.AppSection ([CMI.PS.App]$app)))
}
Set-Variable -Name ConfigurationModel -Value $sections -Option ReadOnly -Force
Get-ChildItem -Path $PSScriptRoot\Model\*.ps1 | ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
    }
}


# Dot-Sourcen der Modulfunktionen
$filesToInclude = @()
$filesToInclude += Get-ChildItem -Path $PSScriptRoot\*.ps1
$filesToInclude += Get-ChildItem -Path $PSScriptRoot\Internal\*.ps1 -Recurse
$filesToInclude | ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
    }
}

# Alles exportieren. Einschraenkungen werden in der Manifestdatei festgelegt.
Export-ModuleMember -Function '*'
Export-ModuleMember -Variable '*'
Export-ModuleMember -Alias '*'