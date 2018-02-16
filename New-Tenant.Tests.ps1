$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "New-Tenant" {

        $minimalDummy = "{ `"t1`": { `"common`": { `"api`": { `"server`": `"https://m.ch/mc`", `"public`": `"/proxy/t1pub`", `"private`": `"/proxy/t1pri`" }}}}"

        Context "When writing configuration" {
            It "Creates new file when configuration does not exists" {
                Remove-Item "Testdrive:\new.json" -ErrorAction SilentlyContinue
                New-Tenant -ConfigurationFile "Testdrive:\new.json" -Name "Test" -ErrorAction Stop
                $result = Get-Content -Raw -Path "Testdrive:\new.json" | ConvertFrom-Json
                $result.Test | Should not be $null
            }
            It "Uses existing file when configuration already exists" {
                Set-Content -Path "Testdrive:\existing.json" -Value $minimalDummy -ErrorAction Stop
                New-Tenant -ConfigurationFile "Testdrive:\existing.json" -Name "Test" -ErrorAction Stop
                $result = Get-Content -Raw -Path "Testdrive:\existing.json" | ConvertFrom-Json
                $result.Test | Should not be $null
                $result.t1 | Should not be $null
            }

            $r1 = New-Tenant -ConfigurationFile "Testdrive:\t2.json" -Name "Test" -WebServerBaseUri "https://t2.ch/m/" -ErrorAction Stop
            $r1 = New-Tenant -ConfigurationFile "Testdrive:\t3.json" -Name "Test" -ConfigureLandingPage -ErrorAction Stop
            $r1 = New-Tenant -ConfigurationFile "Testdrive:\t4.json" -Name "Test" -ConfigureLandingPage -Title "MyTitle" -ErrorAction Stop

            It "Writes minimal common configuration" {
                $result = Get-Content -Raw -Path "Testdrive:\t2.json" | ConvertFrom-Json
                $result.Test.Common.Api.Server | Should be "https://t2.ch/m/mobileclients"
                $result.Test.Common.Api.Public | Should be "/proxy/testpub"
                $result.Test.Common.Api.Private | Should be "/proxy/testpri"
            }
            It "Does not configure landing page when ConfigureLandingPage is not set" {
                $result = Get-Content -Raw -Path "Testdrive:\t2.json" | ConvertFrom-Json
                $result.Test.mobileclients | Should be $null
            }
            It "Configures landing page when ConfigureLandingPage is set" {
                $result = Get-Content -Raw -Path "Testdrive:\t3.json" | ConvertFrom-Json
                $result.Test.Common | Should not be $null
                $result.Test.mobileclients.info | Should be "Mobile Clients Test"
                $result.Test.mobileclients.boot.settings | Should be "https://mobile.cmiaxioma.ch/proxy/testsv"
                $result.Test.mobileclients.boot._internal | Should be $true
                $result.Test.mobileclients.Api._extend | Should be $true
                $result.Test.mobileclients.Api.public | Should be "/test"
                $result.Test.mobileclients.Api.private | Should be "/test"
            }
            It "Overrides default title when title-parameter is set" {
                $result = Get-Content -Raw -Path "Testdrive:\t4.json" | ConvertFrom-Json
                $result.Test.mobileclients.info | Should be "MyTitle"
            }
            It "Returns null when operation is successful" {
                $r1 | Should be $null
                $r2 | Should be $null
                $r3 | Should be $null
            }
        }
        Context "When reading configuration" {
            Set-Content -Path "Testdrive:\bad.json" -Value "$minimalDummy{bad=json}" -ErrorAction Stop
            Set-Content -Path "Testdrive:\t1.json" -Value $minimalDummy -ErrorAction Stop

            It "Fails when existing configuration is not valid json" {
                { New-Tenant -ConfigurationFile "Testdrive:\bad.json" -Name "Test" } | Should throw
            }
            It "Accepts empty json files" {
                Set-Content -Path "Testdrive:\empty.json" -Value "{}" -ErrorAction Stop
                New-Tenant -ConfigurationFile "Testdrive:\empty.json" -Name "Test"
                $result = Get-Content -Raw -Path "Testdrive:\empty.json" | ConvertFrom-Json
                $result.Test | Should not be $null
            }
            It "Fails when tenant already exists in configuration" {
                { New-Tenant -ConfigurationFile "Testdrive:\t1.json" -Name "t1" } | Should throw
            }
            It "Does not modify configuration when tenant already exists" {
                try {
                    New-Tenant -ConfigurationFile "Testdrive:\t1.json" -Name "t1"
                }
                catch {
                    $null
                }
                Get-Content -Path "Testdrive:\t1.json" | Should be $minimalDummy
            }
        }
    }
}


