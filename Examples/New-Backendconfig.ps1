Import-Module CMIMCInstaller
$config=New-CMIMCConfiguration -Schema $DefaultSchema
$config|Add-CMIMCTenant -TenantName schwerzenwil, schwerzenwiltest
$config|Add-CMIMCApp -App Sitzungsvorbereitung, Dossierbrowser,Zusammenarbeitdritte -EnsureDependencies
$config|Set-CMIMCAspect -App Common -AspectPath "account.changePassword" -Value $true -EnsureDependencies
$config|Set-CMIMCAspect -App Common -AspectPath "account.resetPassword" -Value $true -EnsureDependencies
$config|Enable-CMIMCCommonFeature -Feature allowDokumenteOpenExternal
$config|Enable-CMIMCSitzungsvorbereitungFeature -Feature @([enum]::getvalues([SitzungsvorbereitungFeatures]))
$config|Save-CMIMCConfiguration -ConfigurationPath c:\temp\config.mandanten.json -Force