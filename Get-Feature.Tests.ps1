$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
. "$here\$sut"

InModuleScope CMIMCInstaller {
    Describe "Get-Feature" {
        It "Does not filter output when no parameters are provided" {
            $result = Get-Feature
            $result | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAddNew" -and $_.App -eq [App]::Sitzungsvorbereitung
            } | Should -HaveCount 1
            $result | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteDelete" -and $_.App -eq [App]::Zusammenarbeitdritte
            } | Should -HaveCount 1
            $result | Where-Object {
                $_.AspectPath -eq "service.supportsPersoenlicheDokumente" -and $_.AxSupport -eq [AxSupport]::R16_1
            } | Should -HaveCount 1
            $result | Where-Object {
                $_.AspectPath -eq "service.supportsLatestHistory" -and $_.AxSupport -eq [AxSupport]::R17
            } | Should -HaveCount 1
            $result | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAnnotations" -and $_.AxSupport -eq [AxSupport]::R18
            } | Should -HaveCount 1

        }
        It "Filters Axioma versions when parameter provided" {
            $R16 = Get-Feature -AxSupport R16_1
            $R16 | Where-Object {
                $_.AspectPath -eq "service.supportsPersoenlicheDokumente" -and $_.AxSupport -eq [AxSupport]::R16_1
            } | Should -HaveCount 1
            $R16 | Where-Object {
                $_.AspectPath -eq "service.supportsLatestHistory" -and $_.AxSupport -eq [AxSupport]::R17
            } | Should -HaveCount 0
            $R16 | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAnnotations" -and $_.AxSupport -eq [AxSupport]::R18
            } | Should -HaveCount 0

            $R17 = Get-Feature -AxSupport R17
            $R17 | Where-Object {
                $_.AspectPath -eq "service.supportsPersoenlicheDokumente" -and $_.AxSupport -eq [AxSupport]::R16_1
            } | Should -HaveCount 1
            $R17 | Where-Object {
                $_.AspectPath -eq "service.supportsLatestHistory" -and $_.AxSupport -eq [AxSupport]::R17
            } | Should -HaveCount 1
            $R17 | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAnnotations" -and $_.AxSupport -eq [AxSupport]::R18
            } | Should -HaveCount 0

            $R18 = Get-Feature -AxSupport R18
            $R18 | Where-Object {
                $_.AspectPath -eq "service.supportsPersoenlicheDokumente" -and $_.AxSupport -eq [AxSupport]::R16_1
            } | Should -HaveCount 1
            $R18 | Where-Object {
                $_.AspectPath -eq "service.supportsLatestHistory" -and $_.AxSupport -eq [AxSupport]::R17
            } | Should -HaveCount 1
            $R18 | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAnnotations" -and $_.AxSupport -eq [AxSupport]::R18
            } | Should -HaveCount 1
        }
        It "Filters App when parameter provided" {
            $result = Get-Feature -App Zusammenarbeitdritte
            $result | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteAddNew" -and $_.App -eq [App]::Sitzungsvorbereitung
            } | Should -HaveCount 0
            $result | Where-Object {
                $_.AspectPath -eq "service.allowDokumenteDelete" -and $_.App -eq [App]::Zusammenarbeitdritte
            } | Should -HaveCount 1

        }
        It "Adds note properties to result object" {
            $result = Get-Feature
            $result | ForEach-Object {
                $_.AspectPath | Should Be $_.GetAspectPath() 
            } | Should -HaveCount 0             
            
            
            $result = Get-Feature -App Zusammenarbeitdritte
            $result | ForEach-Object {
                $_.App | Should -Be ([App]::Zusammenarbeitdritte)
            } | Should -HaveCount 0

            
        }

    }
}
