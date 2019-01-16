function New-ReverseProxyConfiguration {
        <#
    .SYNOPSIS
        Generates a reverse proxy configuration based on the given json configuration. 
    .DESCRIPTION
        Generates a reverse proxy configuration based on the given json configuration. 
        The configuration is only compatible with a CMI relay server setup.
    .EXAMPLE
        PS> New-ReverseProxyConfiguration -Configuration $conf
        Returns a JSON configuration string using the default relay server url.
    .EXAMPLE
        PS> New-ReverseProxyConfiguration -Configuration $conf -RelayServer https://relay.cmiaxioma.ch | Out-File -FilePath "C:\config.reverseproxy.json"
        Returns a JSON configuration string. The configuration uses the given relay server. 
        In this example the configuration is written to a file.
    .OUTPUTS
        JSON string representing the reverse proxy configuration.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER RelayServer
        Base URL of the relay server.
    #>
    [CmdletBinding(SupportsShouldProcess = $false, ConfirmImpact = 'None')]
    [OutputType([string])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $false, Position = 1, ValueFromPipelineByPropertyName = $True )]
        [ValidateNotNull()]
        [uri]$RelayServer = [uri]"https://relay.cmiaxioma.ch"
    )   
    BEGIN {
        if ($null -eq $RelayServer.Host) {
            throw [ArgumentException] "Host part is required for Relay URL"
        }
        $RelayServer = New-Object -TypeName System.Uri -ArgumentList ($RelayServer, "relay/")
        Write-Verbose "Relay Server URL: $RelayServer"

        function UrlObject ([string]$URIPath) {
            $uri = New-Object -TypeName System.Uri -ArgumentList ($RelayServer, $URIPath)
            Return @{"url" = $uri}
        }
        function BuildDBURL ([Tenant]$tenant) {
            $p1 = "$($tenant.Name)/webapipublic/api/dossierbrowser/Public/"
            $p2 = "GetSettings?app=cmi.dossierbrowser&tenant=$($tenant.Name)"
            return UrlObject "$p1$p2"
        }
        function BuildSVURL ([Tenant]$tenant) {
            $p1 = "$($tenant.Name)/webapipublic/api/sitzungsvorbereitung/Public/"
            $p2 = "GetSettings?app=cmi.sitzungsvorbereitung&tenant=$($tenant.Name)"
            return UrlObject "$p1$p2"
        }
        function BuildZDURL ([Tenant]$tenant) {
            $p1 = "$($tenant.Name)/webapipublic/api/zusammenarbeitdritte/Public/"
            $p2 = "GetSettings?app=cmi.zusammenarbeitdritte&tenant=$($tenant.Name)"
            return UrlObject "$p1$p2"
        }
    }
    PROCESS {
        $proxyConf = [ordered]@{}
        foreach ($tenant in $Configuration.Tenants) {
            $proxyConf["$($tenant.Name)pri"] = UrlObject "$($tenant.Name)/webapiprivate"
            $proxyConf["$($tenant.Name)pub"] = UrlObject "$($tenant.Name)/webapipublic"
            if ($tenant.Has([App]::Dossierbrowser)) {
                $proxyConf["$($tenant.Name)db"] = BuildDBURL $tenant
            }
            if ($tenant.Has([App]::Sitzungsvorbereitung)) {
                $proxyConf["$($tenant.Name)sv"] = BuildSVURL $tenant
            }
            if ($tenant.Has([App]::Zusammenarbeitdritte)) {
                $proxyConf["$($tenant.Name)zd"] = BuildZDURL $tenant
            }
        }
        Write-Output $proxyConf | ConvertTo-Json -Depth 99
    } # end process
} # end function