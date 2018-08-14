$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Remove-App" {

        New-Tenant -ConfigurationFile "Testdrive:\config1.json" -Name "T1"
        New-Tenant -ConfigurationFile "Testdrive:\config1.json" -Name "T2"

        Add-App -ConfigurationFile "Testdrive:\config1.json" -TenantName "T1" -App Dossierbrowser
        Add-App -ConfigurationFile "Testdrive:\config1.json" -TenantName "T2" -App Dossierbrowser -NoAppDirectory

        $configBefore = Get-Content -Raw -Path "Testdrive:\config1.json" | ConvertFrom-Json
        $result1 = Remove-App -ConfigurationFile "Testdrive:\config1.json" -TenantName "T1" -App Dossierbrowser -Confirm:$false | Should be $null
        $result2 = Remove-App -ConfigurationFile "Testdrive:\config1.json" -TenantName "T2" -App Dossierbrowser -Confirm:$false | Should be $null
        $configResult = Get-Content -Raw -Path "Testdrive:\config1.json" | ConvertFrom-Json

        It "Removes app from tenant" {
            $configBefore.T1.PsObject.Properties.Name | Should -Contain 'Dossierbrowser'
            $configBefore.T2.PsObject.Properties.Name | Should -Contain 'Dossierbrowser'
            $configResult.T1.PsObject.Properties.Name | Should -Not -Contain 'Dossierbrowser'
            $configResult.T2.PsObject.Properties.Name | Should -Not -Contain 'Dossierbrowser'
        }

        It "Removes app directory entry" {
            $configBefore.T1.common.appDirectory.PsObject.Properties.Name | Should -Contain 'Dossierbrowser'
            $configBefore.T2.common.appDirectory.PsObject.Properties.Name | Should -Not -Contain 'Dossierbrowser'
            $configResult.T1.common.appDirectory.PsObject.Properties.Name | Should -Not -Contain 'Dossierbrowser'
            $configResult.T2.common.appDirectory.PsObject.Properties.Name | Should -Not -Contain 'Dossierbrowser'
        }

        It "Outputs null" {
            $result1 | Should be $null
            $result2 | Should be $null
        }

    }
}
