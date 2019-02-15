$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Get-Tenant" {

        $config = New-Configuration | Add-Tenant -TenantName t1, t2, t3, t4 -Passthru

        It "Returns requested tenants" {
            $r = Get-Tenant -TenantName t1,t3 -Configuration $config

            $r | Should -HaveCount 2
            $r[0] | Should -BeOfType [Tenant]
            $r[1] | Should -BeOfType [Tenant]
            $r[0].Name | Should -Be "t1"
            $r[1].Name | Should -Be "t3"
        }

        It "Returns all tenants if no tenant name is given" {
            $r = Get-Tenant -Configuration $config

            $r | Should -HaveCount 4
            $r[0] | Should -BeOfType [Tenant]
            $r[1] | Should -BeOfType [Tenant]
            $r[2] | Should -BeOfType [Tenant]
            $r[3] | Should -BeOfType [Tenant]
            $r[0].Name | Should -Be "t1"
            $r[1].Name | Should -Be "t2"
            $r[2].Name | Should -Be "t3"
            $r[3].Name | Should -Be "t4"
        }

        It "Outputs error when tenant name can not be found" {
            { Get-Tenant -TenantName invalid -Configuration $config -ErrorAction Stop } | Should throw
        }
    }
}
