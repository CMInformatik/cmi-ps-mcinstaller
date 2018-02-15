function New-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium')]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [Alias('FullName', 'Path')]
        [String]$ConfigurationFile,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String]$Name,

        [parameter(Mandatory = $False, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [System.Uri]$WebServerBaseUri = "https://mobile.cmiaxioma.ch/",

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [string]$Title
    )
    Begin {
        function Join-Uri {
            param([System.Uri]$Uri, [string]$RelativeUri)
            $result = $null
            if(-not [System.Uri]::TryCreate($Uri, $RelativeUri, [ref]$result)){
                throw "Can not join $($Uri.ToString()) and $RelativeUri to a valid uri"
            }
            return ([System.Uri]$result)
        }
    }
    Process {
        # Datei anlegen
        if (-not (Test-Path $ConfigurationFile -PathType Leaf)) {
            Write-Verbose "$ConfigurationFile not found. Creating file."
            [void] (New-Item -Path $ConfigurationFile -ItemType File -ErrorAction Stop)
        }

        # Datei lesen
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
        if (-not $data) {
            $data = [PSCustomObject]@{}
        }

        # Parameter zusammenstellen
        if (-not $Title) {
            $Title = "Mobile Clients $Name"
        }
        $mobileClientsUri = Join-Uri $WebServerBaseUri "mobileclients"
        $mobileClientsProxyUri = Join-Uri $mobileClientsUri "proxy/$($Name)sv"

        # Mandant hinzufuegen
        if ($data.$Name) {
            throw [System.ArgumentException] "Tenant $Name already exists"
        }
        $data | Add-Member -MemberType NoteProperty -Name $Name -Value ([ordered]@{
            common        = [ordered]@{
                api = [ordered]@{
                    server  = $mobileClientsUri
                    public  = "/proxy/$($Name)pub"
                    private = "/proxy/$($Name)pri"
                }
            }
            mobileclients = [ordered]@{
                info = $Title
                boot = [ordered]@{
                    _internal = $True
                    settings = $mobileClientsProxyUri
                }
                api = [ordered]@{
                    server = $mobileClientsUri
                    public  = "/$($Name)"
                    private = "/$($Name)"
                }
            }
        }) # Ende Add-Member

        # Datei schreiben
        $data | ConvertTo-Json -Depth 99 | Set-Content -Path $ConfigurationFile -ErrorAction Stop
    }
}

#New-CMIMCTenant -Name dc3 -ConfigurationFile test.json -Verbose