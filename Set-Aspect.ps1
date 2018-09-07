function Set-Aspect {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High')]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias('FullName', 'Path')]
        [String]$ConfigurationFile,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [cmi.ps.mcschema.App]$App,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [ValidatePattern('^[A-Za-z]+(\.[A-Za-z]+)*$')]
        [string]$Aspect,

        [parameter(Mandatory = $False, Position = 4, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [object]$Value,

        [parameter(Mandatory = $False, Position = 5, ValueFromPipelineByPropertyName = $True)]
        [switch]$EnsureDependencies
    )
    Process {
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
        # Verfiy AspectPath in Model
        $aspectModel = $ConfigurationModel.GetAspect($App $Aspect)

        if ($aspectModel -isnot [cmi.ps.mcschema.SimpleAspect]) {
            throw "$Aspect is not a simple aspect and can not be set with this method."
        }
        if (-not(HasProperty $data $TenantName)) {
            throw "Tenant $TenantName could not be found in $ConfigurationFile"
        }
        if (-not (HasProperty $data.$TenantName $App)) {
            throw "App $App is not enabled for tenant $TenantName"
        }

        # Set value
        $configPart = $data.$TenantName
        if ($PSCmdlet.ShouldProcess($ConfigurationFile, "Set $($aspectModel.GetAspectPath()) to $value")) {
            Set-ConfigProperty -Data $configPart -App $App -Aspect $aspectModel -Value $Value -ErrorAction Stop -Confirm:$Confirm -EnsureDependencies:$EnsureDependencies
            $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
        }
    }
}

#Set-CMIMCAspect -ConfigurationFile .\test.json -TenantName test -App Common -Aspect 'languages.default' -Value 'De'
#Set-CMIMCAspect -ConfigurationFile .\test.json -TenantName test -App Sitzungsvorbereitung -Aspect 'service.allowDokumenteCopyAsPersoenlich' -Value $true
