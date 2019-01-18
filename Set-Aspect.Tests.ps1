$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Set-Aspect" {
        Context "When set aspect" {
            
            $a = $DefaultSchema['Common']['service']['supportsSaveSettings']
            $a2 = $DefaultSchema['Common']['service']['allowDokumenteOpenExternal']
            $c = New-Configuration | Add-Tenant -TenantName 't1','t2','t3' -Passthru
            $tenant = $c['t1']

            It "Sets aspect with given value by tenant object" {
                $value = $a.GetDefaultValue($tenant)
                $tenant | Set-Aspect -App Common -AspectPath $a.GetAspectPath() -Value $value -EnsureDependencies
                $tenant['Common'].Get($a.GetAspectPath()) | Should be $value
            }

            It "Sets aspect with given value by tenant name" {
                $value = $a.GetDefaultValue($tenant)
                $c | Set-Aspect -TenantName t2,t3 -App Common -AspectPath $a.GetAspectPath() -Value $value -EnsureDependencies
                $tenant['Common'].Get($a.GetAspectPath()) | Should be $value
            }

            It "It accepts null values" {
                $c = New-Configuration | Add-Tenant -TenantName 't1', 't2' -Passthru
                $c | Set-Aspect -App Common -TenantName t1, t2 -AspectPath $a.GetAspectPath() -Value $null -EnsureDependencies
            }

            It "Outputs error when tenant can not be found" {
                $c = New-Configuration | Add-Tenant -TenantName 't1' -Passthru
                { $c | Set-Aspect -App Common -TenantName t1, t2 -AspectPath $a.GetAspectPath() -Value $null -EnsureDependencies -Confirm:$false -ErrorAction Stop } | Should -Throw
            }

            It "It sets aspect for all tenants when no tenant list is given" {
                $c = New-Configuration | Add-Tenant -TenantName 't1', 't2', 't3' -Passthru
                $c['t1']['Common'].Get($a.GetAspectPath()) | Should be $null
                $c['t2']['Common'].Get($a.GetAspectPath()) | Should be $null
                $c['t3']['Common'].Get($a.GetAspectPath()) | Should be $null
                $c | Set-Aspect -App Common -AspectPath $a.GetAspectPath() -Value $true -EnsureDependencies
                $c['t1']['Common'].Get($a.GetAspectPath()) | Should be $true
                $c['t2']['Common'].Get($a.GetAspectPath()) | Should be $true
                $c['t3']['Common'].Get($a.GetAspectPath()) | Should be $true
            }

            It "Allows multiple aspect paths" {
                $c = New-Configuration | Add-Tenant -TenantName 't1' -Passthru
                $c | Set-Aspect -App Common -AspectPath $a.GetAspectPath(),$a2.GetAspectPath() -Value $true -EnsureDependencies
                $c['t1']['Common'].Get($a.GetAspectPath()) | Should be $true
                $c['t1']['Common'].Get($a2.GetAspectPath()) | Should be $true
            }

            It "Does not fail when aspect is already present" {
                $c = New-Configuration | Add-Tenant -TenantName 't1' -Passthru
                $c | Set-Aspect -App Common -AspectPath $a.GetAspectPath() -Value $true -EnsureDependencies
                $c | Set-Aspect -App Common -AspectPath $a.GetAspectPath() -Value $true -EnsureDependencies
            }
        }

        Context "When returning result" {
            $c = New-Configuration | Add-Tenant -TenantName 't1', 't2', 't3', 't4' -Passthru
            $aspect = $DefaultSchema['common'].Traverse() | Where-Object { $_ -is [SimpleAspect] } | Select-Object -First 1

            It "Returns Tenant" {
                $result = Set-Aspect -Configuration $c -TenantName 't1', 't2', 't3', 't4' -App Common -AspectPath $aspect.GetAspectPath() -EnsureDependencies -Confirm:$false
                $result | Should -HaveCount 4
                $result | ForEach-Object { $_ | Should -BeOfType [Tenant] }
            }
            It "Returns configuration when passthru is set" {
                $result = Set-Aspect -Configuration $c -TenantName 't1', 't2', 't3', 't4' -App Common -AspectPath $aspect.GetAspectPath() -EnsureDependencies -Confirm:$false -Passthru
                $result | Should -BeOfType [JsonConfiguration]
            }
        }
    }
}
