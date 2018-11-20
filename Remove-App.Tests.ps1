$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Remove-App" {
        Context "When removing apps" {
            It "Removes app by tenant name" {
                $config = New-Configuration | Add-Tenant -TenantName t1,t2 -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru

                $config['t1'].Has([App]::Dossierbrowser) | Should -BeTrue
                $config['t2'].Has([App]::Dossierbrowser) | Should -BeTrue
                $config['t1'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
                $config['t2'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue

                $config | Remove-App -TenantName t1,t2 -App Dossierbrowser -Confirm:$false

                $config['t1'].Has([App]::Dossierbrowser) | Should -BeFalse
                $config['t2'].Has([App]::Dossierbrowser) | Should -BeFalse

                # Shall not remove the wrong apps
                $config['t1'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
                $config['t2'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
            }

            It "Removes several apps to tenant when app list is given" {
                $c = New-Configuration | Add-Tenant -TenantName 't1' -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru

                $c['t1'].Has([App]::Dossierbrowser) | Should be $true
                $c['t1'].Has([App]::Sitzungsvorbereitung) | Should be $true

                $c | Remove-App -TenantName t1 -App Dossierbrowser,Sitzungsvorbereitung -Confirm:$false
                
                $c['t1'].Has([App]::Dossierbrowser) | Should be $false
                $c['t1'].Has([App]::Sitzungsvorbereitung) | Should be $false
            }

            It "Removes from all tenants when no tenantlist list is given" {
                $c = New-Configuration | Add-Tenant -TenantName t1,t2,t3,t4 -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru

                foreach($t in $c.Tenants) {
                    $t.Has([App]::Sitzungsvorbereitung) | Should be $true
                }

                $c | Remove-App -App Sitzungsvorbereitung -Confirm:$false

                foreach($t in $c.Tenants) {
                    $t.Has([App]::Sitzungsvorbereitung) | Should be $false
                    $t.Has([App]::Dossierbrowser) | Should be $true
                }
            }

            It "Removes all apps except common when no app list is given" {
                $c = New-Configuration | Add-Tenant -TenantName t1,t2,t3,t4 -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru

                foreach($t in $c.Tenants) {
                    $t.Has([App]::Common) | Should be $true
                    $t.Has([App]::Sitzungsvorbereitung) | Should be $true
                    $t.Has([App]::Dossierbrowser) | Should be $true
                }

                $c | Remove-App -Confirm:$false

                foreach($t in $c.Tenants) {
                    $t.Has([App]::Common) | Should be $true
                    $t.Has([App]::Sitzungsvorbereitung) | Should be $false
                    $t.Has([App]::Dossierbrowser) | Should be $false
                }
            }

            It "Removes app by tenant object" {
                $config = New-Configuration | Add-Tenant -TenantName t1,t2 -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru

                $config['t1'].Has([App]::Dossierbrowser) | Should -BeTrue
                $config['t2'].Has([App]::Dossierbrowser) | Should -BeTrue
                $config['t1'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
                $config['t2'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue

                [Tenant]$t1 = $config['t1']
                [Tenant]$t2 = $config['t2']

                Remove-App -Tenant $t1,$t2 -App Dossierbrowser -Confirm:$false

                $config['t1'].Has([App]::Dossierbrowser) | Should -BeFalse
                $config['t2'].Has([App]::Dossierbrowser) | Should -BeFalse

                # Shall not remove the wrong apps
                $config['t1'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
                $config['t2'].Has([App]::Sitzungsvorbereitung) | Should -BeTrue
            }

            It "Does not fail, when tenant does not have the specified app" {
                $config = New-Configuration | Add-Tenant -TenantName t1,t2 -Passthru
                Remove-App -Configuration $config -TenantName t1,t2 -App Dossierbrowser -Confirm:$false -ErrorAction Stop
            }
        }

        Context "When returning result" {
            $c = New-Configuration | Add-Tenant -TenantName t1,t2,t3,t4 -Passthru | Add-App -App Dossierbrowser,Sitzungsvorbereitung -Passthru
            It "Returns tenant" {
                $result = Remove-App -Configuration $c -TenantName 't1','t2','t3','t4' -App Dossierbrowser -Confirm:$false
                $result | Should -HaveCount 4
                $result | ForEach-Object { $_ | Should -BeOfType [Tenant] }
                $result | ForEach-Object { $_.Name | Should -Match 't[1,2,3,4]' }
            }
            It "Returns configuration when passthru is set" {
                $result = Remove-App -Configuration $c -TenantName 't1','t2','t3','t4' -App Sitzungsvorbereitung -Confirm:$false -Passthru
                $result | Should -BeOfType [JsonConfiguration]
            }
        }
    }
}
