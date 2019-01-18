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
        
        It "Uses default schema if no schema is provided" {
            $result = New-Configuration 
            $DefaultSchema.Equals($result.Schema)  | Should -BeTrue   
        }

        It "Uses provided schema" {
            $result = New-Configuration -Schema $FrontendSchema
            $FrontendSchema.Equals($result.Schema)  | Should -BeTrue
        }   
    }
}
