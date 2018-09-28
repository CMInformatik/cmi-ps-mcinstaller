$account = New-Object ComplexAspect ('account', [cmi.mc.config.ConfigControlAttribute]::Private)
$account.AddAspect((New-Object SimpleAspect ('changePassword', [bool], $false)))
$account.AddAspect((New-Object SimpleAspect ('resetPassword', [bool], $true)))

$ConfigurationModel[[cmi.mc.config.App]::Common].AddAspect($account)