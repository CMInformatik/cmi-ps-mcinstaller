$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Remove-Tenant" {
        Context "When removing tenants" {
            $config = New-Configuration
            $config.AddTenant('test1', 'test2', 'test3', 'test4', 'test5')

            It "Removes tenant by name" {
                $config.GetTenant('test2') | Should -not -be $null
                $config.GetTenant('test3') | Should -not -be $null
                $config.GetTenant('test4') | Should -not -be $null

                Remove-Tenant -Configuration $config -TenantName test2,test3,test4 -Confirm:$false

                $config.GetTenant('test2') | Should -be $null
                $config.GetTenant('test3') | Should -be $null
                $config.GetTenant('test4') | Should -be $null

                # Soll nicht die falschen Mandanten entfernen
                $config.GetTenant('test1') | Should -not -be $null
                $config.GetTenant('test5') | Should -not -be $null
            }

            It "Does not fail, when tenant can not be found" {
                Remove-Tenant -Configuration $config -TenantName notpresent -Confirm:$false -ErrorAction Stop
            }
        }
        Context "When passthru is set" {
            $config = New-Configuration
            $config.AddTenant('test1', 'test2', 'test3', 'test4', 'test5')
            It "Returns configuration"{
                $r = Remove-Tenant -Configuration $config -TenantName test2 -Confirm:$false -Passthru
                $r | Should -be $config
            }
        }
    }
}
