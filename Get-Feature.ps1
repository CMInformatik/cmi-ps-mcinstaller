function Get-Feature {
    <#
    .SYNOPSIS
        Returns all supported boolean app features which can be turned off or on.
    .EXAMPLE
        PS> Get-CMIMCFeature
        Returns all features for all apps for all supported Axioma versions.
    .OUTPUTS
        SimpleAspect objects representing the features.
    .PARAMETER App
        Filter features for the given app.
    .PARAMETER AxSupport
        Filter features for the given Axioma version.
    .PARAMETER Schema
        The configuration model.
    #>
    [CmdletBinding(SupportsShouldProcess = $false)]
    [OutputType([SimpleAspect])]
    PARAM(
        [parameter(Mandatory = $False, Position = 0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [App[]]$App,

        [parameter(Mandatory = $False, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [AxSupport]$AxSupport,

        [parameter(Mandatory = $False, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Schema]$Schema = $DefaultSchema
    )

    BEGIN {
        if ($null -eq $App) {
            $App = $Schema.Keys
        }
    }

    PROCESS {
        foreach ($currentApp in $App) {
            Write-Verbose "Liste der App-Features fuer $currentApp ermitteln"
            #Liste der App-Features ermitteln
            $features = $Schema[$currentApp]['service'].Aspects.Values | Where-Object { 
                $_ -is [SimpleAspect] -and ([SimpleAspect]$_).Type -eq [bool] 
            } 
            Write-Verbose "Verfuegbare Features: $(($features|Select-Object -ExpandProperty name) -join ", ")"

            #Features gemaess AxSupport Filtern
            if ($features -and ($null -ne $AxSupport)) {
                Write-Verbose "Featurefilter auf $AxSupport"
                $features = $features | Where-Object {
                    $_.AxSupport -le $AxSupport
                } 
            }
            foreach ($feature in $features){
                Add-Member -InputObject $feature -MemberType NoteProperty -Name AspectPath -Value $feature.GetAspectPath() -Force
                Add-Member -InputObject $feature -MemberType NoteProperty -Name App -Value $currentApp -Force
                Write-Output $feature
            }
        }

        
    } # End Process
}
