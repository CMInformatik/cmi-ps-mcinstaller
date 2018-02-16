$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Remove-Tenant" {
        Context "When removing tenants" {

            foreach ($name in ("Test1", "Test2", "Test3", "Test4", "Test5")) {
                New-Tenant -ConfigurationFile "Testdrive:\t2.json" -Name $name -ErrorAction Stop
            }

            It "Removes tenant from configuration" {
                $before = Get-Content -Raw -Path "Testdrive:\t2.json" | ConvertFrom-Json
                $before.Test2 | Should not be $null
                $before.Test5 | Should not be $null

                Remove-Tenant -ConfigurationFile "Testdrive:\t2.json" -Name "Test2", "Test5" -ErrorAction Stop -Confirm:$false
                $after = Get-Content -Raw -Path "Testdrive:\t2.json" | ConvertFrom-Json
      
                $after.Test1 | Should not be $null
                $after.Test2 | Should be $null
                $after.Test3 | Should not be $null
                $after.Test4 | Should not be $null
                $after.Test5 | Should be $null

            }
            It "Outputs error when tenant can not be found" {
                { Remove-Tenant -ConfigurationFile "Testdrive:\t2.json" -Name "NotInConfig" -ErrorAction Stop -Confirm:$false } | Should throw
            }

            It "Returns null when operation is successful" {
                Remove-Tenant -ConfigurationFile "Testdrive:\t2.json" -Name "Test1" -ErrorAction Stop -Confirm:$false | Should be $null
            }

        }
    }
}
