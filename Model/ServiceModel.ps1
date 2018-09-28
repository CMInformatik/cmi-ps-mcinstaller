# Common
$common_service = New-Object ComplexAspect ('service')
$common_service.AddAspect((New-Object SimpleAspect ('allowDokumenteOpenExternal', [bool], $true)))
$common_service.AddAspect((New-Object SimpleAspect ('allowDokumenteAddNewVersion', [bool], $false)))
$common_service.AddAspect((New-Object SimpleAspect ('allowDokumenteAddNew', [bool], $false)))
$common_service.AddAspect((New-Object SimpleAspect ('supportsDokumenteDelete', [bool], $false)))
$common_service.AddAspect((New-Object SimpleAspect ('supportsPrivate', [bool], $false, [cmi.mc.config.AxSupport]::R17)))
$common_service.AddAspect((New-Object SimpleAspect ('supportsSaveSettings', [bool], $false, [cmi.mc.config.AxSupport]::R18)))

$ConfigurationModel[[App]::Common].AddAspect($common_service)

# DB
$db_service = New-Object ComplexAspect ('service')
$db_service.AddAspect((New-Object SimpleAspect ('allowDokumenteCheckIn', [bool], $false)))
$db_service.AddAspect((New-Object SimpleAspect ('allowDokumenteDetails', [bool], $false)))
$db_service.AddAspect((New-Object SimpleAspect ('allowSearchForKontakte', [bool], $false)))
$db_service.AddAspect((New-Object SimpleAspect ('supportsDokumenteVersions', [bool], $false)))
$db_service.AddAspect((New-Object SimpleAspect ('supportsDetailsSearch', [bool], $false)))

$ConfigurationModel[[App]::Dossierbrowser].AddAspect($db_service)

# ZD
$zd_dep1 = New-Object SimpleAspectDependency ([App]::Common, 'service.allowDokumenteAddNewVersion', $true)
$zd_dep2 = New-Object SimpleAspectDependency ([App]::Common, 'service.allowDokumenteAddNew', $true)
$zd_dep3 = New-Object SimpleAspectDependency ([App]::Common, 'service.supportsDokumenteDelete', $true)

$ConfigurationModel[[App]::Zusammenarbeitdritte].Dependencies.Add($zd_dep1)
$ConfigurationModel[[App]::Zusammenarbeitdritte].Dependencies.Add($zd_dep2)
$ConfigurationModel[[App]::Zusammenarbeitdritte].Dependencies.Add($zd_dep3)

# SV
$sv_slhm = New-Object SimpleAspect ('supportsLatestHistoryMail', [bool], $true,  [AxSupport]::R18)
$sv_slhm.Dependencies.Add((New-Object SimpleAspectDependency ([App]::Common, 'service.supportsSaveSettings', $true)))

$sv_sgp = New-Object SimpleAspect ('supportsGesamtPdf', [bool], $false,  [AxSupport]::R18)
$sv_sgp.Dependencies.Add((New-Object SimpleAspectDependency ([App]::Sitzungsvorbereitung, 'service.supportsPersoenlicheDokumente', $true)))

$sv_adcap = New-Object SimpleAspect ('allowDokumenteCopyAsPersoenlich', [bool], $true, [AxSupport]::R18)
$sv_adcap.Dependencies.Add((New-Object SimpleAspectDependency ([App]::Sitzungsvorbereitung, 'service.supportsPersoenlicheDokumente', $true)))

$sv_afts = New-Object SimpleAspect ('allowFreigabeToSachbearbeiter', [bool], $true, [AxSupport]::R18)
$sv_afts.Dependencies.Add((New-Object SimpleAspectDependency ([App]::Sitzungsvorbereitung, 'service.supportsFreigabe', $true)))

$sv_ada = New-Object SimpleAspect ('allowDokumenteAnnotations', [bool], $false, [AxSupport]::R18)
$sv_ada.Dependencies.Add((New-Object SimpleAspectDependency ([App]::Common, 'ui.pdf.editor', 'pdftools')))

$sv_service = New-Object ComplexAspect ('service')
$sv_service.AddAspect((New-Object SimpleAspect ('supportsPersoenlicheDokumente', [bool], $false)))
$sv_service.AddAspect((New-Object SimpleAspect ('supportsFreigabe', [bool], $false)))
$sv_service.AddAspect((New-Object SimpleAspect ('supportsWortbegehren', [bool], $true)))
$sv_service.AddAspect((New-Object SimpleAspect ('supportsLatestHistory', [bool], $false,  [AxSupport]::R17)))
$sv_service.AddAspect((New-Object SimpleAspect ('supportsPrintOnDemand', [bool], $false,  [AxSupport]::R18)))
$sv_service.AddAspect($sv_adcap)
$sv_service.AddAspect($sv_afts)
$sv_service.AddAspect($sv_slhm)
$sv_service.AddAspect($sv_sgp)
$sv_service.AddAspect($sv_ada)

$ConfigurationModel[[App]::Sitzungsvorbereitung].AddAspect($sv_service)
