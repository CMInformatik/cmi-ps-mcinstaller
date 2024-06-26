function New-FeatureProxy {
    # there is no system change
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseShouldProcessForStateChangingFunctions", "")]
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
                [ValidateNotNullOrEmpty()]
                [string]$FunctionName,
                [ValidateNotNullOrEmpty()]
                [string]$FunctionHelpText,
                [bool]$FeatureValue,
                [ValidateNotNullOrEmpty()]
                [string]$FeatureHelpText
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
    
            # add function
            $stringBuilder = New-Object System.Text.StringBuilder
            [void]$stringBuilder.AppendLine("Function $FunctionName {")

            # add comments
            [void]$stringBuilder.AppendLine("<#")
            $help = Get-Help Set-Aspect

            [void]$stringBuilder.AppendLine(".SYNOPSIS")
            [void]$stringBuilder.AppendLine($FunctionHelpText)

            foreach ($key in $MetaData.Parameters.Keys) {
                $paramHelp = $help.parameters.parameter | Where-Object { $_.name -eq $key -and $null -ne $_.description }
                if ($paramHelp) {
                    [void]$stringBuilder.AppendLine(".PARAMETER $key")
                    [void]$stringBuilder.AppendLine("$($paramHelp.description.Text)")
                }
            }

            [void]$stringBuilder.AppendLine(".PARAMETER Feature")
            [void]$stringBuilder.AppendLine($FeatureHelpText)
            [void]$stringBuilder.AppendLine("#>")

            # add param
            [void]$stringBuilder.AppendLine([System.Management.Automation.ProxyCommand]::GetCmdletBindingAttribute($MetaData))
            [void]$stringBuilder.AppendLine("PARAM(")
            [void]$stringBuilder.AppendLine([System.Management.Automation.ProxyCommand]::GetParamBlock($MetaData))
            [void]$stringBuilder.AppendLine(")")

            # add begin
            [void]$stringBuilder.AppendLine("Begin{")
            [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"App`"] = [App]`"$($App.ToString())`"")
            if ($FeatureValue) {
                [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"Value`"] = `$true")
            }
            else {
                [void]$stringBuilder.AppendLine("`t`t`$PSBoundParameters[`"Value`"] = `$false")
            }
            [void]$stringBuilder.AppendLine("`t}")
    
            # add process
            [void]$stringBuilder.AppendLine("Process{")
            $ProcessBlock = {

                $PSBoundParameters["EnsureDependencies"] = $true
                [void]$PSBoundParameters.Remove("Feature")
                $PSBoundParameters["AspectPath"] = @()

                foreach ($f in $Feature) {
                    $PSBoundParameters["AspectPath"] += "service.$($f.ToString())"
                }
                Write-Verbose "Proxy $($MyInvocation.MyCommand) calls $(Get-Command Set-Aspect -ErrorAction Stop) with:"
                $PSBoundParameters.Keys | ForEach-Object {
                    Write-Verbose "Proxied Parameter '$_' -> $($PSBoundParameters[$_])"
                }
                Set-Aspect @PSBoundParameters
            }.ToString()
            [void]$stringBuilder.AppendLine($ProcessBlock)
            [void]$stringBuilder.AppendLine("`t}")

            [void]$stringBuilder.AppendLine("}")

            # result
            Write-Output @{
                Definition   = ($stringBuilder.ToString())
                FunctionName = $FunctionName
            }
        }

        Write-Verbose "Create Enable-/Disable-$($App)Feature proxies for Set-Aspect"
        $PSBoundParameters.Keys | ForEach-Object {
            Write-Verbose "Create Proxy with: $_ -> $($PSBoundParameters[$_])"
        }
        CreateProxyFunction "Enable-$($App)Feature" "Enables features of the app $App." $true "Feature to enable."
        CreateProxyFunction "Disable-$($App)Feature" "Disables features of the app $App." $false "Feature to disable."
    }
}