$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Get-Configuration" {
        Context "When reading configuration" {
            $path1 = Join-Path $TestDrive 't1.json'
            $path2 = Join-Path $TestDrive 't2.json'
            New-Configuration |
                Add-Tenant -TenantName 't1', 't2' -Passthru |
                Add-App -App Dossierbrowser -Passthru|
                Save-Configuration -Path $path1 -Passthru -Confirm:$false
            New-Configuration |
                Add-Tenant -TenantName 't3', 't4' -Passthru |
                Add-App -App Dossierbrowser -Passthru |
                Save-Configuration -Path $path2 -Passthru -Confirm:$false

            It "Returns configuration when reading from file" {
                $result = Get-Configuration -Path $path1,$path2
                $result[0] | Should -BeOfType [JsonConfiguration]
                $result[0]['t1'][[App]::Dossierbrowser] | Should -not -be $null
                $result[0]['t2'][[App]::Dossierbrowser] | Should -not -be $null

                $result[1] | Should -BeOfType [JsonConfiguration]
                $result[1]['t3'][[App]::Dossierbrowser] | Should -not -be $null
                $result[1]['t4'][[App]::Dossierbrowser] | Should -not -be $null
            }

            It "Reads configuration from relative paths" {
                Push-Location (Split-Path $path1)

                $result = Get-Configuration -Path ".\$(Split-Path $path1 -Leaf)" -Verbose
                $result | Should -BeOfType [JsonConfiguration]

                Pop-Location
            }

            It "Outputs error when configuration can not be read" {
                $path = Join-Path $TestDrive 'notpresent.json'
                { Get-Configuration -Path $path } | Should Throw
            }

            It "Uses default schema if no schema is provided" {
                $result = Get-Configuration -Path $path1
                $DefaultSchema.Equals($result.Schema)  | Should -BeTrue   
            }

            It "Uses provided schema" {
                $result = Get-Configuration -Path $path1 -Schema $FrontendSchema
                $FrontendSchema.Equals($result.Schema)  | Should -BeTrue
            }   
        }
    }
}
