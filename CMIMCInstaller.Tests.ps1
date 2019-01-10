Describe "CMIMICInstaller" {

    $Modulename = 'CMIMCInstaller'

    Context "Function synopsis" {
        $functions = Get-Command -Module $Modulename
        if ($functions -eq $null -or $functions.Count -eq 0) {
            throw "Could not found any function in module, misspelling?"
        }

        foreach ($function in $functions) {
            It "$($function.Name): has synopsis text" {
                $synopsis = ($function | Get-Help | Select-Object synopsis).synopsis
                ((-not [string]::IsNullOrWhiteSpace($synopsis)) -and (-not $synopsis.Trim().StartsWith($function.Name))) | Should Be $true
                # Ist keine Synopsis vorhanden, kann es vorkommen, dass in der Synopsis die Syntax der Funktion steht.
                # Die Syntax beginnt immer mit dem Namen der Funktion
                # Synopsis-Texte die mit dem Namen der Funktion beginnen, auch als nicht bestanden erfassen.
            }
        }
    }

    Context "Parameter description" {
        $commonParameters = [System.Management.Automation.PSCmdlet]::CommonParameters + [System.Management.Automation.PSCmdlet]::OptionalCommonParameters
        $functions = Get-Command -Module $Modulename
        if ($functions -eq $null -or $functions.Count -eq 0) {
            throw "Could not found any function in module, misspelling?"
        }
        
        foreach ($function in $functions) {
            $help = Get-Help $function.Name -Full
            foreach ($parameter in $help.Parameters.Parameter) {
                if($commonParameters -contains $parameter.Name){
                    continue
                }
                It "Parameter $($function.Name):$($parameter.Name) has description" {
                    -not [string]::IsNullOrWhiteSpace($parameter.Description.Text) | Should be $true
                }
            }
        }
    }

    Context "Uniform function naming" {
        $pattern = "^[A-Z][a-z]+-CMIMC[A-Z].*"
        $functions = Get-Command -Module $Modulename
        if ($functions -eq $null -or $functions.Count -eq 0) {
            throw "Could not found any function in module, misspelling?"
        }

        foreach ($function in $functions) {
            It "$($function.Name): matches name convention" {
                $($function.Name) -match $pattern | Should Be $True
            }
        }
    }
}
