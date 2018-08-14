$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "HasProperty" {

        Context "ByName" {
            $obj = [PSCustomObject]@{
                Prop1 = $null
            }
            It "Retruns true when object contains requested property" {
                HasProperty $obj Prop1 | Should be $true
            }
            It "Retruns false when object not contains requested property" {
                HasProperty $obj Prop2 | Should be $false
            }
        }
        Context "ByPath" {
            $obj = [PSCustomObject]@{
                Prop1 = [PSCustomObject]@{
                    Prop2 = [PSCustomObject]@{
                        Prop3 = $null
                    }
                }
            }
            It "Retruns true when object contains requested property path" {
                HasProperty $obj -Path 'Prop1.Prop2.Prop3' | Should be $true
            }
            It "Retruns false when object not contains requested property path" {
                HasProperty $obj -Path 'Prop1.Prop3.Prop2' | Should be $false
            }
        }
    }
}
