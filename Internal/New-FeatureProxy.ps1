function New-FeatureProxy {
    [CmdletBinding()]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Type]$ParameterType
    )
    Process {

        function CreateProxyFunction {
            param (
                [string]$FunctionName,
                [bool]$FeatureValue
            )
            $MetaData = New-Object System.Management.Automation.CommandMetaData (Get-Command Set-Aspect -ErrorAction Stop)
            $parameter = New-Object System.Management.Automation.ParameterMetadata @($MetaData.Parameters["AspectPath"] )
            $parameter.ParameterType = $ParameterType
            $parameter.Name = "Feature"
    
            [Void]$MetaData.Parameters.Remove('AspectPath')
            [Void]$MetaData.Parameters.Remove('App')
            [Void]$MetaData.Parameters.Remove('Value')
            [Void]$MetaData.Parameters.Remove('EnsureDependencies')
            [Void]$MetaData.Parameters.Add("Feature", $parameter)
    
            $stringBuilder = New-Object System.Text.StringBuilder
            [void]$stringBuilder.AppendLine("Function $FunctionName {")
            [void]$stringBuilder.AppendLine([System.Management.Automation.ProxyCommand]::GetCmdletBindingAttribute($MetaData))
            [void]$stringBuilder.AppendLine("PARAM(")
            [void]$stringBuilder.AppendLine([System.Management.Automation.ProxyCommand]::GetParamBlock($MetaData))
            [void]$stringBuilder.AppendLine(")")
    
            [void]$stringBuilder.AppendLine("Begin{")
            [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"App`"] = [App]`"$($App.ToString())`"")
            if($FeatureValue){
                [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"Value`"] = `$true")
            }
            else {
                [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"Value`"] = `$false")
            }
            [void]$stringBuilder.AppendLine("`t}")
    
            [void]$stringBuilder.AppendLine("Process{")
            $ProcessBlock = {
                $PSBoundParameters["AspectPath"] = "service.$($Feature.ToString())"
                $PSBoundParameters["EnsureDependencies"] = $true
                $PSBoundParameters.Remove("Feature")
    
                Write-Verbose "Proxy $($MyInvocation.MyCommand) calls $(Get-Command Set-Aspect -ErrorAction Stop) with:"
                $PSBoundParameters.Keys | ForEach-Object {
                    Write-Verbose "Proxied Parameter '$_' -> $($PSBoundParameters[$_])"
                }
                Set-Aspect @PSBoundParameters
            }.ToString()
            [void]$stringBuilder.AppendLine($ProcessBlock)
            [void]$stringBuilder.AppendLine("`t}")

            [void]$stringBuilder.AppendLine("}")
            Write-Output @{
                Definition = ($stringBuilder.ToString())
                FunctionName = $FunctionName
            }
        }

        Write-Verbose "Create Enable-/Disable-$($App)Feature proxies for Set-Aspect"
        $PSBoundParameters.Keys | ForEach-Object {
            Write-Verbose "Create Proxy with: $_ -> $($PSBoundParameters[$_])"
        }
        CreateProxyFunction "Enable-$($App)Feature" $true
        CreateProxyFunction "Disable-$($App)Feature" $false
    }
}