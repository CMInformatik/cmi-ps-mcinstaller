#Requires -PSEdition Desktop

# Invoke-Expression for New-FeatureProxy result, no user input accepted when using Invoke-Expression.
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingInvokeExpression", "")]
Param()

# Load configurator library
Get-ChildItem -Path $PSScriptRoot\Internal\cmi.mobileclients.config\*.dll | ForEach-Object {
    [Reflection.Assembly]::LoadFile($_.FullName)
}

$accelerators = [PSObject].Assembly.GetType('System.Management.Automation.TypeAccelerators')
$accelerators::Add("App", "cmi.mobileclients.config.ModelContract.App")
$accelerators::Add("JsonConfiguration", "cmi.mobileclients.config.JsonConfiguration")
$accelerators::Add("AppConfiguration", "cmi.mobileclients.config.ModelContract.Components.IAppConfiguration")
$accelerators::Add("SimpleAspect", "cmi.mobileclients.config.ModelContract.Components.ISimpleAspect")
$accelerators::Add("Tenant", "cmi.mobileclients.config.ModelContract.Components.ITenant")

# Define configuration schema
Set-Variable -Name Schema -Value (New-Object 'cmi.mobileclients.config.DefaultSchema.DefaultSchema') -Option ReadOnly -Force -Scope Global

# Dot source module functions
Get-ChildItem -Path $PSScriptRoot\Internal\*.ps1 -Recurse | ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
    }
}
Get-ChildItem -Path $PSScriptRoot\*.ps1 | ForEach-Object {
    if (-Not $_.FullName.EndsWith("Tests.ps1")) {
        . $_.FullName
        Export-ModuleMember -Function $_.BaseName
    }
}

# Generate feature functions
foreach ($app in $Schema.Keys) {
    $features = ($Schema[$app]['service'].Aspects.Values |
            Where-Object { $_ -is [SimpleAspect] -or ([SimpleAspect]$_).Type -eq [bool] } |
            Select-Object -ExpandProperty Name) -join ','
    if(![string]::IsNullOrWhiteSpace($features)){
        $enumName = "$($app)Features"
        Add-Type -TypeDefinition "public enum $enumName { $features }" -ErrorAction Stop
        $functions = New-FeatureProxy $app ([System.Type]("$enumName[]"))
        $functions | ForEach-Object {
            Invoke-Expression $_.Definition
            Export-ModuleMember -Function $_.FunctionName
        }
    }
}