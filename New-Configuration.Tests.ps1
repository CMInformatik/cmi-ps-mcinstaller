$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

Describe "New-Configuration" {

    Context "When create new configuration" {
        [JsonConfiguration]$result = New-Configuration

        It "Returns json configuration" {
            $result | Should -BeOfType [JsonConfiguration]
        }

        It "Does not contain tenants" {
            $result.Tenants | Should -HaveCount 0
        }
    }
}
