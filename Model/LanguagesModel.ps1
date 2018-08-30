$languages = New-Object ComplexAspect 'languages'
$languages.AddAspect((New-Object SimpleAspect ('supports', [string[]], [string[]]@('De'))))
$languages.AddAspect((New-Object SimpleAspect ('default', [string], 'De')))

$ConfigurationModel[[App]::Common].AddAspect($languages)