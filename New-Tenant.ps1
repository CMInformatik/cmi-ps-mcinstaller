function New-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName="__AllParameterSets")]
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

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True, ParameterSetName="LandingPage")]
        [ValidateNotNullOrEmpty()]
        [string]$Title,

        [parameter(Mandatory = $False, Position = 4, ValueFromPipelineByPropertyName = $True, ParameterSetName="LandingPage")]
        [ValidateNotNullOrEmpty()]
        [switch]$ConfigureLandingPage
    )
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
        $mobileClientsUri = Join-Uri $WebServerBaseUri "mobileclients"

        # Mandant hinzufuegen
        if ($data.$Name) {
            throw [System.ArgumentException] "Tenant $Name already exists"
        }
        Write-Verbose "Adding tenant $Name with minimal common configuration"
        $data | Add-Member -MemberType NoteProperty -Name $Name -ErrorAction Stop -Value ([ordered]@{
            common        = [ordered]@{
                api = [ordered]@{
                    server  = $mobileClientsUri
                    public  = "/proxy/$($Name)pub"
                    private = "/proxy/$($Name)pri"
                }
            }
        }) # Ende Add-Member
        if($ConfigureLandingPage){
            if (-not $Title) {
                $Title = "Mobile Clients $Name"
            }
            Write-Verbose "Adding app mobileclients to tenant $Name"
            $data.$Name.Add("mobileclients", ([ordered]@{
                info = $Title
                boot = [ordered]@{
                    _internal = $True
                    settings = Join-Uri $mobileClientsUri "proxy/$($Name)sv"
                }
                api = [ordered]@{
                    _extend = $true
                    public  = "/$($Name)"
                    private = "/$($Name)"
                }
            })) # Ende Add-Member
        } # Ende if

        # Datei schreiben
        if($PSCmdlet.ShouldProcess($ConfigurationFile, "Add $Name")){
            $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
        }
    }
}

#New-CMIMCTenant -Name dc3 -ConfigurationFile test.json -Verbose