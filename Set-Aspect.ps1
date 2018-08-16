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
    }
    Process {
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
        # Verfiy AspectPath in Model
        $aspectModel = Get-AspectFromModel $Aspect $ConfigurationModel[$App]

        if($aspectModel -isnot [CMI.PS.SimpleAspect]){
            throw "$Aspect is not a simple aspect and can not be set with this method."
        }
        if (-not(HasProperty $data $TenantName)) {
            throw "Tenant $TenantName could not be found in $ConfigurationFile"
        }
        if (-not (HasProperty $data.$TenantName $App)) {
            throw "App $App is not enabled for tenant $TenantName"
        }

        # Create parent aspects
        $path = $data.$TenantName.$App
        foreach($parent in ($aspectModel.Parent.GetAspectPath() -split '\.')){
            if(!(HasProperty $path $parent)){
                $path | Add-Member -MemberType NoteProperty -Name $parent -Value ([pscustomobject]@{}) -ErrorAction Stop
            }
            $path = $path.$parent
        }

        # Create aspect
        if($PSCmdlet.ShouldProcess($ConfigurationFile, "Set $($aspectModel.GetAspectPath()) to $value")){
            if(HasProperty $path $aspectModel.Name){
                $path.$($aspectModel.Name) = $Value
            } else {
                $path | Add-Member -MemberType NoteProperty -Name $aspectModel.Name -Value $Value -ErrorAction Stop
                if($aspectModel.DefaultCCA)
            }
            $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
        }
    }
}

#Set-CMIMCAspect -ConfigurationFile .\test.json -TenantName test -App Common -Aspect 'languages.default' -Value 'De'
