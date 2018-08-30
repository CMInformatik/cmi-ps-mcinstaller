function Set-ConfigProperty {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High')]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [PSCustomObject]$Data,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [cmi.ps.mcschema.App]$App,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [cmi.ps.mcschema.Aspect]$Aspect,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [object]$Value,

        [parameter(Mandatory = $False, Position = 4, ValueFromPipelineByPropertyName = $True)]
        [switch]$EnsureDependencies
    )
    Process {
        # Test value
        if ($Aspect -is [cmi.ps.mcschema.SimpleAspect]) {
            Test-AspectValue $Aspect $Value
        }
        else {
            if ($Value -and $Value -isnot [PSCustomObject]) {
                throw "$($Aspect.Name): $($Value.GetType().FullName) is not convertable to type PSCustomObject"
            }
        }

        # Is app enabled
        if (-not (HasProperty $Data $App)) {
            throw "App $App is not enabled"
        }

        # Test dependencies
        if ($Aspect.Dependencies -and $Aspect.Dependencies.Count -gt 0) {
            foreach ($dep in $Aspect.Dependencies) {
                try {
                    $dep.Verify($Data)
                }
                catch [cmi.ps.mcschema.ElementDependencyNotFulfilled] {
                    if ($EnsureDependencies -and $PSCmdlet.ShouldProcess($Aspect.GetAspectPath(), "Ensure dependency: $($_.Exception.Message)")) {
                        $dep.Ensure($Data)
                    }
                    else {
                        throw
                    }
                } # ende catch
            } # ende foreach Dependency
        }

        if (-not $PSCmdlet.ShouldProcess($Aspect.GetAspectPath(), "Set to $value")) {
            return
        }

        # Create parent aspects
        $configPart = $Data.$App
        foreach ($parentAspect in $Aspect.GetParents()) {
            if (!(HasProperty $configPart $parentAspect.Name)) {
                Set-ConfigProperty $Data $App $parentAspect ([pscustomobject]@{}) -ErrorAction Stop -Confirm:$Confirm
            }
            $configPart = $configPart.$($parentAspect.Name)
        }

        # Create aspect
        if (HasProperty $configPart $Aspect.Name) {
            Write-Verbose "Property $($Aspect.Name) is already present. Overwriting"
            $configPart.$($Aspect.Name) = $Value
        }
        else {
            Write-Verbose "Adding property $($Aspect.Name)"
            $configPart | Add-Member -MemberType NoteProperty -Name $Aspect.Name -Value $Value -ErrorAction Stop
        }

        # Create default cca if not present
        if ( $Aspect.DefaultCCA -ne 'NotSet') {
            switch ($Aspect.DefaultCCA) {
                'Extend' { $attrName = '_extend' }
                'Replace' { $attrName = '_replace' }
                'Remove' { $attrName = '_remove' }
                'Internal' { $attrName = '_internal' }
                'Private' { $attrName = '_private' }
                Default { throw "Unkown attribute $($Aspect.DefaultCCA.ToString())" }
            }
            if (!(HasProperty $configPart.$($Aspect.Name) $attrName)) {
                Write-Verbose "Adding default control attribute $attrName to $($Aspect.Name)"
                $configPart.$($Aspect.Name) | Add-Member -MemberType NoteProperty -Name $attrName -Value $true -ErrorAction Stop
            }
        }
    } # ende process
} # ende function