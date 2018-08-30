function Test-AspectValue([cmi.ps.mcschema.SimpleAspect]$Aspect, [object]$Value){
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
