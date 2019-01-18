$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"


InModuleScope CMIMCInstaller {
    Describe "New-ReverseProxyConfiguration" {

        $config = New-Configuration |
            Add-Tenant -TenantName 't1', 't2' -Passthru |
            Add-App -App Dossierbrowser -Passthru

        Context "When using relay server url" {
            It "Requires relay base url with host" {
                { New-ReverseProxyConfiguration -Configuration $config -RelayServer "" } | Should throw
                { New-ReverseProxyConfiguration -Configuration $config -RelayServer $null } | Should throw
                { New-ReverseProxyConfiguration -Configuration $config -RelayServer "/relay" } | Should throw
                New-ReverseProxyConfiguration -Configuration $config -RelayServer "http://relay.ch" -ErrorAction Stop | Should -Not -BeNullOrEmpty
            }

            It "Uses given relay server" {
                $result = New-ReverseProxyConfiguration -Configuration $config -RelayServer "http://relay.ch" -ErrorAction Stop
                $result | Should -BeLike "*http://relay.ch*"
            }

            It "Uses default relay server if no url is provided" {
                $result = New-ReverseProxyConfiguration -Configuration $config -ErrorAction Stop
                $result | Should -BeLike "*https://relay.cmiaxioma.ch*"
            }
        }

        Context "When generating json" {
            It "Adds private and public reverse entries" {
                $result = New-ReverseProxyConfiguration -Configuration $config -RelayServer "http://r.r" -ErrorAction Stop | ConvertFrom-Json
                $result.t1pri.url | Should -BeExactly "http://r.r/relay/t1/webapiprivate"
                $result.t1pub.url | Should -BeExactly "http://r.r/relay/t1/webapipublic"
                $result.t2pri.url | Should -BeExactly "http://r.r/relay/t2/webapiprivate"
                $result.t2pub.url | Should -BeExactly "http://r.r/relay/t2/webapipublic"
            }

            It "Adds app reverse entries" {
                $config = New-Configuration | Add-Tenant -TenantName 't1', 't2', 't3' -Passthru
                $config | Add-App -TenantName 't1' -App Sitzungsvorbereitung -EnsureDependencies
                $config | Add-App -TenantName 't2' -App Dossierbrowser -EnsureDependencies
                $config | Add-App -TenantName 't3' -App Zusammenarbeitdritte -EnsureDependencies

                $result = New-ReverseProxyConfiguration -Configuration $config -RelayServer "http://r.r" -ErrorAction Stop | ConvertFrom-Json
                $result.t1sv.url | Should -BeExactly "http://r.r/relay/t1/webapipublic/api/sitzungsvorbereitung/Public/GetSettings?app=cmi.sitzungsvorbereitung&tenant=t1"
                $result.t2db.url | Should -BeExactly "http://r.r/relay/t2/webapipublic/api/dossierbrowser/Public/GetSettings?app=cmi.dossierbrowser&tenant=t2"
                $result.t3zd.url | Should -BeExactly "http://r.r/relay/t3/webapipublic/api/zusammenarbeitdritte/Public/GetSettings?app=cmi.zusammenarbeitdritte&tenant=t3"
            }
        }
    }
}
