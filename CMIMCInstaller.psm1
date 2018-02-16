#Requires -PSEdition Desktop

Add-Type -TypeDefinition '
    namespace CMI.PS
    {
        public enum App {
            Common, Mobileclients, Dossierbrowser, Sitzungsvorbereitung, Zusammenarbeitdritte, Steuerbrowser, Starclient
        }

        public enum ConfigurationAttribute {
            Extend, Replace, Remove, Internal, Private
        }
    }
'

# Dot-Sourcen der Modulfunktionen
$filesToInclude = @()
$filesToInclude += Get-ChildItem -Path $PSScriptRoot\*.ps1
$filesToInclude += Get-ChildItem -Path $PSScriptRoot\Internal\*.ps1 -Recurse
$filesToInclude |
    ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
    }
}

# Alles exportieren. Einschraenkungen werden in der Manifestdatei festgelegt.
Export-ModuleMember -Function '*'
Export-ModuleMember -Variable '*'
Export-ModuleMember -Alias '*'