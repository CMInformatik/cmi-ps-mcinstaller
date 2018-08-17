$account = New-Object CMI.PS.ComplexAspect ('account', [CMI.PS.ConfigControlAttribute]::Private)
$account.AddAspect((New-Object CMI.PS.SimpleAspect ('changePassword', [bool], $false)))
$account.AddAspect((New-Object CMI.PS.SimpleAspect ('resetPassword', [bool], $true)))

$ConfigurationModel[[CMI.PS.App]::Common].AddAspect($account)