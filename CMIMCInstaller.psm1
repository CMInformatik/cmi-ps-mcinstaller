#Requires -PSEdition Desktop

# Konfigurationsmodell Definition
Get-ChildItem -Path $PSScriptRoot\Internal\cmi.mobileclients.config\*.dll | ForEach-Object { 
    [Reflection.Assembly]::LoadFile($_.FullName)
}

$accelerators = [PSObject].Assembly.GetType('System.Management.Automation.TypeAccelerators')
$accelerators::Add("App", "cmi.mobileclients.config.ModelContract.App")
$accelerators::Add("JsonConfiguration", "cmi.mobileclients.config.JsonConfiguration")
$accelerators::Add("AppConfiguration", "cmi.mobileclients.config.ModelContract.Components.IAppConfiguration")
$accelerators::Add("SimpleAspect", "cmi.mobileclients.config.ModelContract.Components.ISimpleAspect")
$accelerators::Add("Tenant", "cmi.mobileclients.config.ModelContract.Components.ITenant")

# Konfigurationsmodell laden
Set-Variable -Name Schema -Value (New-Object 'cmi.mobileclients.config.DefaultSchema.DefaultSchema') -Option ReadOnly -Force

#$Schema[[App]::Common]['service'].Aspects.Values |
#    Where-Object { $_ -is [SimpleAspect] -or ([SimpleAspect]$_).Type -eq [bool] } |
#    ForEach-Object { $_.GetAspectPath() }

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