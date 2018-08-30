#Requires -PSEdition Desktop

# Konfigurationsmodell Definition
Add-Type -Path $PSScriptRoot\Schema\cmi.ps.mcschema.dll -ErrorAction Stop

# define accelerators to shortcut: New-Object cmi.ps.mcschema.ComplexAspect --> New-Object ComplexAspect 
$accelerators = [PSObject].Assembly.GetType('System.Management.Automation.TypeAccelerators')
$accelerators::Add('ComplexAspect','cmi.ps.mcschema.ComplexAspect')
$accelerators::Add('SimpleAspect','cmi.ps.mcschema.SimpleAspect')
$accelerators::Add('AppSection','cmi.ps.mcschema.AppSection')
$accelerators::Add('App','cmi.ps.mcschema.App')
$accelerators::Add('ConfigControlAttribute','cmi.ps.mcschema.ConfigControlAttribute')
$accelerators::Add('AxSupport','cmi.ps.mcschema.AxSupport')

Get-ChildItem -Path $PSScriptRoot\Schema\*.ps1 | ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
    }
}

# Konfigurationsmodell laden
[System.Collections.Generic.Dictionary[[string], [cmi.ps.mcschema.AppSection]]]$sections = (New-Object 'System.Collections.Generic.Dictionary`2[System.String,cmi.ps.mcschema.AppSection]')
foreach ($app in [System.Enum]::GetNames( [cmi.ps.mcschema.App])) {
    $sections.Add($app, (New-Object cmi.ps.mcschema.AppSection ([cmi.ps.mcschema.App]$app)))
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