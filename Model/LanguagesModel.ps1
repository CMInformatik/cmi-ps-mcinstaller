$languages = New-Object CMI.PS.ComplexAspect 'languages'
$languages.AddAspect((New-Object CMI.PS.SimpleAspect ('supports', [string[]], [string[]]@('De'))))
$languages.AddAspect((New-Object CMI.PS.SimpleAspect ('default', [string], 'De')))

$ConfigurationModel[[CMI.PS.App]::Common].AddAspect($languages)