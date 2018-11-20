$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Add-Tenant" {
        Context "When adding tenant" {
            $c = New-Configuration

            It "Adds tenant by name" {
                $c | Add-Tenant -TenantName t1,t2 -Confirm:$false
                $c['t1'].Name | Should be 't1'
                $c['t2'].Name | Should be 't2'
            }

            $invalidNames = @(
                "Näme",
                "N me",
                " ame",
                "Name ",
                " ",
                "",
                $null,
                "+am?"
            )

            foreach($name in $invalidNames){
                It "Outputs error when tenant name is invalid: '$name'" {
                    { $c | Add-Tenant -TenantName $name -App -Confirm:$false -ErrorAction Stop } | Should -Throw
                }
            }
        }

        Context "When returning result" {

            It "Returns existing tenant, when tenant is already present" {
                $c = New-Configuration | Add-Tenant -TenantName 't1' -Passthru
                $c.Tenants.Count | Should be 1
                $result = Add-Tenant -Configuration $c -TenantName t1 -Confirm:$false
                $c.Tenants.Count | Should be 1
                $result.Name | Should be t1
            }

            It "Returns AppConfiguration" {
                $result = New-Configuration | Add-Tenant -TenantName 't1', 't2', 't3', 't4' -Confirm:$false
                $result | Should -HaveCount 4
                $result | ForEach-Object { $_ | Should -BeOfType [Tenant] }
            }
            It "Returns configuration when passthru is set" {
                $result = New-Configuration | Add-Tenant -TenantName 't1', 't2', 't3', 't4' -Confirm:$false -Passthru
                $result | Should -BeOfType [JsonConfiguration]
            }
        }
    }
}


