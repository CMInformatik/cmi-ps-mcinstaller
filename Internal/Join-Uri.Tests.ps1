$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Join-Uri" {

        $testset = @(
            [System.Tuple]::Create("https://t.ch","e","https://t.ch/e"),
            [System.Tuple]::Create("https://t.ch/","e","https://t.ch/e"),
            [System.Tuple]::Create("https://t.ch","/e","https://t.ch/e"),
            [System.Tuple]::Create("https://t.ch/","/e","https://t.ch/e"),
            [System.Tuple]::Create("https://t.ch/sub","e","https://t.ch/e"),
            [System.Tuple]::Create("https://t.ch/sub/","e","https://t.ch/sub/e"),
            [System.Tuple]::Create("https://t.ch/sub","/e","https://t.ch/e")
        )

        foreach($test in $testset){
            It "Joins '$($test.Item1)' and '$($test.Item2)' to '$($test.Item3)'" {
                Join-Uri ([System.Uri]$($test.Item1)) $test.Item2 | Should be $test.Item3
            }
        }


    }
}
