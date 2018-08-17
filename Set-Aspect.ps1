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
        [CMI.PS.App]$App,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [ValidatePattern('^[A-Za-z]+(\.[A-Za-z]+)*$')]
        [string]$Aspect,

        [parameter(Mandatory = $False, Position = 4, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [object]$Value
    )
    Begin {
        function Get-AspectFromModel([string]$Aspect, [CMI.PS.AppSection]$AppSection) {
            $aspectPath += $Aspect -split '\.'
            $currentAspect = $AppSection
            foreach ($part in $aspectPath) {
                if ($currentAspect -isnot [CMI.PS.ComplexAspect] -or !$currentAspect.Aspects[$part]) {
                    throw "$($AppSection.App) does not have a aspect path of $($aspectPath -join '.')"
                }
                $currentAspect = $currentAspect.Aspects[$part]
            }
            return $currentAspect
        }
        function Add-ConfigProperty([PSCustomObject]$ConfigPart, [CMI.PS.Aspect]$Aspect, [object]$Value) {
            if ($Aspect -is [CMI.PS.SimpleAspect]) {
                Test-Value $Aspect $Value
            }
            else {
                if ($Value -and $Value -isnot [PSCustomObject]) {
                    throw "$($Aspect.Name): $($Value.GetType().FullName) is not convertable to type PSCustomObject"
                }
            }
            Write-Verbose "Adding property $($Aspect.Name)"
            $ConfigPart | Add-Member -MemberType NoteProperty -Name $Aspect.Name -Value $Value -ErrorAction Stop
            if ($Aspect.DefaultCCA -ne 'NotSet') {
                switch ($Aspect.DefaultCCA) {
                    'Extend' { $attrName = '_extend' }
                    'Replace' { $attrName = '_replace' }
                    'Remove' { $attrName = '_remove' }
                    'Internal' { $attrName = '_internal' }
                    'Private' { $attrName = '_private' }
                    Default { throw "Unkown attribute $($Aspect.DefaultCCA.ToString())" }
                }
                $ConfigPart.$($Aspect.Name) | Add-Member -MemberType NoteProperty -Name $attrName -Value $true -ErrorAction Stop
            }
        }

        function Test-Value([CMI.PS.SimpleAspect]$Aspect, [object]$Value){
            if ($Value -and !$Aspect.Type.IsInstanceOfType($Value)) {
                throw "$($Aspect.Name): $($Value.GetType().FullName) is not convertable to type $($Aspect.Type.FullName)"
            }
            foreach($attr in $Aspect.ValidationAttributes){
                Write-Verbose "Testing value against $($attr.GetType().FullName)"
                $val = $attr.GetType().GetMethods(([Reflection.BindingFlags] "NonPublic,Instance")) | Where-Object Name -eq Validate
                try{
                    $val.Invoke($attr, @($Value, $null))
                }
                catch {
                    # non terminating error to terminating error
                    throw
                }
                
            }
        }
    }
    Process {
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
        # Verfiy AspectPath in Model
        $aspectModel = Get-AspectFromModel $Aspect $ConfigurationModel[$App]

        if ($aspectModel -isnot [CMI.PS.SimpleAspect]) {
            throw "$Aspect is not a simple aspect and can not be set with this method."
        }
        if (-not(HasProperty $data $TenantName)) {
            throw "Tenant $TenantName could not be found in $ConfigurationFile"
        }
        if (-not (HasProperty $data.$TenantName $App)) {
            throw "App $App is not enabled for tenant $TenantName"
        }

        # Create parent aspects
        $configPart = $data.$TenantName.$App
        foreach ($parentAspect in $aspectModel.GetParents()) {
            if ($parentAspect -is [CMI.PS.AppSection]) {
                continue;
            }
            if (!(HasProperty $configPart $parentAspect.Name)) {
                Add-ConfigProperty $configPart $parentAspect ([pscustomobject]@{})
            }
            $configPart = $configPart.$($parentAspect.Name)
        }

        # Create aspect
        if ($PSCmdlet.ShouldProcess($ConfigurationFile, "Set $($aspectModel.GetAspectPath()) to $value")) {
            if (HasProperty $configPart $aspectModel.Name) {
                Test-Value $aspectModel $Value
                $configPart.$($aspectModel.Name) = $Value
            }
            else {
                Add-ConfigProperty $configPart $aspectModel $Value
            }
            $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
        }
    }
}

#Set-CMIMCAspect -ConfigurationFile .\test.json -TenantName test -App Common -Aspect 'languages.default' -Value 'De'
