$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Remove-Tenant" {
        Context "When removing tenants" {
            It "Implement" {
                $false | Should be $true
            }
    }
}
