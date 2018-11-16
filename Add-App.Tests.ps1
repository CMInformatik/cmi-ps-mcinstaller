$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Add-App" {
        Context "When adding app" {
            $c = New-Configuration | Add-Tenant -TenantName 't1','t2','t3','t4' -Passthru

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

            It "Adds several apps to tenant when app list is given" {
                $c = New-Configuration | Add-Tenant -TenantName 't1','t2' -Passthru
                $result = $c | Add-App -TenantName t1,t2 -App Zusammenarbeitdritte,Dossierbrowser -EnsureDependencies -Confirm:$false -ErrorAction Stop

                $c['t1'].Has([App]::Zusammenarbeitdritte) | Should be $true
                $c['t1'].Has([App]::Dossierbrowser) | Should be $true
                $c['t1'].Has([App]::Sitzungsvorbereitung) | Should be $false
                
                $c['t2'].Has([App]::Zusammenarbeitdritte) | Should be $true
                $c['t2'].Has([App]::Dossierbrowser) | Should be $true
                $c['t2'].Has([App]::Sitzungsvorbereitung) | Should be $false
            }

            It "Adds to all tenants when no tenantlist list is given" {
                $c = New-Configuration | Add-Tenant -TenantName 't1','t2','t3' -Passthru
                $result = $c | Add-App -App Zusammenarbeitdritte,Dossierbrowser -EnsureDependencies -Confirm:$false -ErrorAction Stop

                $c['t1'].Has([App]::Zusammenarbeitdritte) | Should be $true
                $c['t1'].Has([App]::Dossierbrowser) | Should be $true

                $c['t2'].Has([App]::Zusammenarbeitdritte) | Should be $true
                $c['t2'].Has([App]::Dossierbrowser) | Should be $true

                $c['t3'].Has([App]::Zusammenarbeitdritte) | Should be $true
                $c['t3'].Has([App]::Dossierbrowser) | Should be $true

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
            $c = New-Configuration | Add-Tenant -TenantName 't1','t2','t3','t4' -Passthru
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
