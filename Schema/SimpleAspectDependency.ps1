class SimpleAspectDependency : cmi.ps.mcschema.IElementDependency {

    hidden [cmi.ps.mcschema.App]$app
    hidden [cmi.ps.mcschema.SimpleAspect]$aspect
    hidden [string]$aspectPath
    hidden [object]$value

    SimpleAspectDependency ([cmi.ps.mcschema.App]$app, [string]$aspectPath, [object]$value)
    {
        if([string]::IsNullOrWhiteSpace($aspectPath)){
            Throw New-Object System.ArgumentException "aspectPath is empty","aspectPath"
        }
        if($aspectPath -notmatch '^[A-Za-z]+(\.[A-Za-z]+)*$'){
            Throw New-Object System.ArgumentException "aspectPath is invalid","aspectPath"
        }

        $this.app = $app
        $this.value = $value
        $this.aspectPath = $aspectPath
    }

    hidden EnsureAspect(){
        if(!$this.aspect){
            $this.aspect = Get-AspectFromModel $this.aspectPath $this.app -ErrorAction Stop
        }
        if($this.aspect -isnot [cmi.ps.mcschema.SimpleAspect]){
            throw New-Object System.ArgumentException "$($this.aspectPath) needs to be a simple aspect", "aspectPath"
        }
    }

    Verify([System.Management.Automation.PSObject]$data) {
        $this.EnsureAspect()

        # verify app is enabled
        if(!(HasProperty $data $this.app.ToString())){
            throw [cmi.ps.mcschema.ElementDependencyNotFulfilled] "Dependency: The app $($this.app) is not enabled."
        }

        $datasection = $data.$($this.app)

        # verify aspect path
        foreach ($parentAspect in $this.aspect.GetParents()) {
            if (!(HasProperty $datasection $parentAspect.Name)) {
                throw [cmi.ps.mcschema.ElementDependencyNotFulfilled] "Dependency: The part '$($parentAspect.Name)' of aspect $($this.Aspect) is not present."
            }
            $datasection = $datasection.$($parentAspect.Name)
        }

        # verify aspect value
        if($null -ne $this.value -and $this.value -ne $datasection){
            throw [cmi.ps.mcschema.ElementDependencyNotFulfilled] "Dependency: Aspect $($this.Aspect) requires to be set to '$($this.value)'."
        }
    }

    Ensure([System.Management.Automation.PSObject]$data) {
        $this.EnsureAspect()

        #verify app is enabled
        if(!(HasProperty $data $this.app.ToString())){
            throw [cmi.ps.mcschema.ElementDependencyNotFulfilled] "Dependency: The app $($this.app) is not enabled."
        }

        #ensure aspect is set
        Set-ConfigProperty -Data $data -App $this.App -Aspect $($this.aspect) -Value ($this.value) -EnsureDependencies -ErrorAction Stop -Confirm:$false
    }
}