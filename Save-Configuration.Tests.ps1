$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Save-Configuration" {
        Context "When writing configuration" {
            $config = New-Configuration
            $config.AddTenant('test')

            It "Writes configuration to file" {
                $path = Join-Path $TestDrive 't.json'
                Test-Path $path | Should be $false
                Save-Configuration -Configuration $config -Path $path -Confirm:$false
                Test-Path $path | Should be $true
            }

            It "Returns null" {
                $path = Join-Path $TestDrive 't1.json'
                Save-Configuration -Configuration $config -Path $path -Confirm:$false | Should be $null
            }

            It "Does not overwrite existing configuration file" {
                $path = Join-Path $TestDrive 't2.json'
                Set-Content -Path $path -Value "Unchanged"
                { Save-Configuration -Configuration $config -Path $path -Confirm:$false -ErrorAction Stop } | Should -Throw
                Get-Content -Path $path | Should be "Unchanged"
            }

            It "Does overwrite existing configuration file with force" {
                $path = Join-Path $TestDrive 't3.json'
                Set-Content -Path $path -Value "Unchanged"
                Save-Configuration -Configuration $config -Path $path -Confirm:$false -Force
                Get-Content -Path $path | Should -not -be "Unchanged"
            }

            It "Saves configuration to relative paths" {
                $path = Join-Path $TestDrive 't4.json'
                Push-Location (Split-Path $path)
                Save-Configuration -Configuration $config -Path ".\$(Split-Path $path -Leaf)" -ErrorAction Stop -Confirm:$false
                Test-Path $path | Should be $true
                Pop-Location
            }
        }

        Context "When passthru is set" {
            $config = New-Configuration
            $config.AddTenant('test')

            It "Returns configuration" {
                $path = Join-Path $TestDrive 'test.json'
                $result = Save-Configuration -Configuration $config -Path $path -Confirm:$false -Passthru
                $result | Should -BeOfType [JsonConfiguration]
                $result['test'] | Should -BeOfType [Tenant]
            }
        }
    }
}
