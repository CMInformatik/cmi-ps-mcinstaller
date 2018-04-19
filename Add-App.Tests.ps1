$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Add-App" {

        $config1 = "Testdrive:\config1.json"
        New-Tenant -ConfigurationFile "Testdrive:\config1.json" -Name "T1"
        New-Tenant -ConfigurationFile "Testdrive:\config1.json" -Name "T2"

        Context "When configuration precondition is not fulfilled" {
            It "Outputs error when tenant does not exists" {
                { Add-App -ConfigurationFile $config1 -Tenant 'T0' -App Dossierbrowser -ErrorAction Stop } | Should throw
            }
            It "Outputs error when app is already enabled for tenant" {
                Add-App -ConfigurationFile $config1 -Tenant 'T1' -App Dossierbrowser -ErrorAction Stop
                { Add-App -ConfigurationFile $config1 -Tenant 'T1' -App Dossierbrowser -ErrorAction Stop } | Should throw
            }

            Mock Get-Content { "{`"T1`":  {}}" }
            It "Outputs error when common section is not available for tenant" {
                { Add-App -ConfigurationFile "C:\mock" -Tenant 'T1' -App Dossierbrowser -ErrorAction Stop } | Should throw
                $error[0].Exception.Message | Should BeLike "*does not have required common configuration*"
            }
        }
    }

}
