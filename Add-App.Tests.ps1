$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Add-App" {
        Context "When adding app" {
            $c = New-Configuration | New-Tenant -TenantName 't1','t2','t3','t4' -Passthru

            foreach($app in [cmi.mobileclients.config.ModelContract.McSymbols]::Apps){
                It "Adds app $app by tenant object" {
                    Add-App -Tenant $c['t1'],$c['t2'] -App $app -EnsureDependencies -Confirm:$false
                    $c['t1'].Has($app)
                    $c['t2'].Has($app)
                }
            }

            foreach($app in [cmi.mobileclients.config.ModelContract.McSymbols]::Apps){
                It "Adds app $app by tenant name" {
                    Add-App -Configuration $c -TenantName t3,t4 -App $app -EnsureDependencies -Confirm:$false
                    $c['t1'].Has($app)
                    $c['t2'].Has($app)
                }
            }

            It "Outputs error when tenant can not be found" {
                { Add-App -Configuration $c -TenantName notpresent -App Zusammenarbeitdritte -EnsureDependencies -Confirm:$false -ErrorAction Stop } | Should -Throw
            }

            It "Does not fail when app is already present" {
                Add-App -Configuration $c -TenantName t4 -App Zusammenarbeitdritte -EnsureDependencies -Confirm:$false
                Add-App -Configuration $c -TenantName t4 -App Zusammenarbeitdritte -Confirm:$false
            }
        }

        Context "When returning result" {
            $c = New-Configuration | New-Tenant -TenantName 't1','t2','t3','t4' -Passthru
            It "Returns AppConfiguration" {
                $result = Add-App -Configuration $c -TenantName 't1','t2','t3','t4' -App Zusammenarbeitdritte -EnsureDependencies -Confirm:$false
                $result | Should -HaveCount 4
                $result | ForEach-Object { $_ | Should -BeOfType [AppConfiguration] }
                $result | ForEach-Object { $_.App | Should -Be Zusammenarbeitdritte }
            }
            It "Returns configuration when passthru is set" {
                $result = Add-App -Configuration $c -TenantName 't1','t2','t3','t4' -App Zusammenarbeitdritte -EnsureDependencies -Confirm:$false -Passthru
                $result | Should -BeOfType [JsonConfiguration]
            }
        }
    }
}
