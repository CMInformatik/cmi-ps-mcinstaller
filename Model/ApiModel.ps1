$api = New-Object ComplexAspect ('api', [ConfigControlAttribute]::Private)

$server = New-Object SimpleAspect ('server', [uri], $null)
$server.ValidationAttributes.Add((New-Object System.Management.Automation.ValidateNotNullOrEmptyAttribute))
$server.IsRequired = $true

$public = New-Object SimpleAspect ('public', [string], $null)
$public.ValidationAttributes.Add((New-Object System.Management.Automation.ValidateNotNullOrEmptyAttribute))
$public.IsRequired = $true

$private = New-Object SimpleAspect ('private', [string], $null)
$private.ValidationAttributes.Add((New-Object System.Management.Automation.ValidateNotNullOrEmptyAttribute))
$private.IsRequired = $true

$api.AddAspect($server)
$api.AddAspect($public)
$api.AddAspect($private)

$ConfigurationModel[[App]::Common].AddAspect($api)