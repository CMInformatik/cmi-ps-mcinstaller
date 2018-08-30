$account = New-Object ComplexAspect ('account', [cmi.ps.mcschema.ConfigControlAttribute]::Private)
$account.AddAspect((New-Object SimpleAspect ('changePassword', [bool], $false)))
$account.AddAspect((New-Object SimpleAspect ('resetPassword', [bool], $true)))

$ConfigurationModel[[cmi.ps.mcschema.App]::Common].AddAspect($account)