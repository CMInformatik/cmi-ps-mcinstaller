function Add-ReverseProxyConfiguration {

    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '')] # no system changes
    [CmdletBinding(SupportsShouldProcess = $false, ConfirmImpact = 'None')]
    [OutputType([string])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true )]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $false, Position = 1, ValueFromPipelineByPropertyName = $True )]
        [ValidateNotNull()]
        [uri]$RelayServer = [uri]"https://relay.cmiaxioma.ch",

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        #[ValidateScript({ MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 3, ValueFromPipelineByPropertyName = $True)]
        #[ValidateScript({ MustBeValidTenantName $_ })]
        [String]$ProxyConfigurationPath 
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
        #$fullName = (Get-Item -Path $ProxyConfigurationPath -ErrorAction Stop).FullName
        #Write-Host "Reading configuration from $fullName"
        $proxyConf = Get-Content -Path $ProxyConfigurationPath -raw | ConvertFrom-Json             
        #Write-Host "Config:"
        #Write-Host $proxyConf
        foreach ($tenant in $Configuration.Tenants) {
            if($tenant.Name -in $TenantName){
                #$proxyConfAdd-Member –MemberType NoteProperty –Name "$($tenant.Name)pri" -Value (UrlObject "$($tenant.Name)/webapiprivate")
                $proxyConf|Add-Member –MemberType NoteProperty –Name "$($tenant.Name)pri" -Value (UrlObject "$($tenant.Name)/webapiprivate")
                #$proxyConf["$($tenant.Name)pub"] = UrlObject "$($tenant.Name)/webapipublic"
                $proxyConf|Add-Member –MemberType NoteProperty –Name "$($tenant.Name)pub" -Value (UrlObject "$($tenant.Name)/webapipublic")
                if ($tenant.Has([App]::Dossierbrowser)) {
                    #$proxyConf["$($tenant.Name)db"] = BuildDBURL $tenant
                    $proxyConf|Add-Member –MemberType NoteProperty –Name "$($tenant.Name)db" -Value (BuildDBURL $tenant)
                }
                if ($tenant.Has([App]::Sitzungsvorbereitung)) {
                    #$proxyConf["$($tenant.Name)sv"] = BuildSVURL $tenant
                    $proxyConf|Add-Member –MemberType NoteProperty –Name "$($tenant.Name)sv" -Value (BuildSVURL $tenant)
                }
                if ($tenant.Has([App]::Zusammenarbeitdritte)) {
                    #$proxyConf["$($tenant.Name)zd"] = BuildZDURL $tenant
                    $proxyConf|Add-Member –MemberType NoteProperty –Name "$($tenant.Name)zd" -Value (BuildZDURL $tenant)
                }
                #
            }
        }
          $proxyConf | ConvertTo-Json -Depth 99 | Out-File -FilePath $ProxyConfigurationPath
    }
}