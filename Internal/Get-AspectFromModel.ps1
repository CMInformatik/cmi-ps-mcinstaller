function Get-AspectFromModel([string]$Aspect, [cmi.ps.mcschema.App]$App) {
    $aspectPath = $Aspect -split '\.'
    $currentAspect = $ConfigurationModel[$App]
    foreach ($part in $aspectPath) {
        if (($currentAspect -isnot [cmi.ps.mcschema.ComplexAspect] -and $currentAspect -isnot [cmi.ps.mcschema.AppSection]) -or !$currentAspect.Aspects[$part]) {
            throw "$App does not have a aspect path of $($aspectPath -join '.')"
        }
        $currentAspect = $currentAspect.Aspects[$part]
    }
    return $currentAspect
}