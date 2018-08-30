function Add-App {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName="__AllParameterSets")]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript( { MustBeFile $_ })]
        [Alias('FullName', 'Path')]
        [String]$ConfigurationFile,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern('^[A-Za-z0-9\-_]+$')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [cmi.ps.mcschema.App]$App,

        [parameter(Mandatory = $False, Position = [int]::MaxValue, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [switch]$NoAppDirectory
    )
    Process {
        # Datei lesen
        $data = Get-Content -Path $ConfigurationFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop

        foreach($tenant in $TenantName){
            if(-not(HasProperty $data $tenant)){
                Write-Error "Tenant $tenant could not be found in $ConfigurationFile" -TargetObject $tenant
                continue
            }
            if(HasProperty $data.$tenant $App) {
                Write-Error "App $App is already enabled for tenant $tenant" -TargetObject $tenant
                continue
            }

            if(-not $NoAppDirectory){
                if(-not (HasProperty $data -Path "$tenant.common.appDirectory")){
                    Write-Verbose "Adding appDirectory to common"
                    $data.$tenant.common | Add-Member -MemberType NoteProperty -Name "appDirectory" -Value ([pscustomobject]@{}) -ErrorAction Stop
                    # ToDo: Add "_private": true ?
                }
    
                [System.Uri]$baseUri = $data.$tenant.common.api.server
                if(-not $baseUri){
                    Write-Error "Tenant $tenant does not have required common.api.server configuration" -TargetObject $tenant
                    continue
                }

                switch ($App) {
                    'Dossierbrowser' {
                        $data.$tenant.common.appDirectory | Add-Member -MemberType NoteProperty -Name dossierbrowser -ErrorAction Stop -Value (@{
                            web = Join-Uri $baseUri.GetLeftPart([System.UriPartial]::Authority) "dossierbrowser\$tenant"
                            app = "cmidossierbrowser://"
                            dossierDetail = "/Abstr/{GUID}"
                        })
                    }
                    'Sitzungsvorbereitung' {
                        $data.$tenant.common.appDirectory | Add-Member -MemberType NoteProperty -Name sitzungsvorbereitung -ErrorAction Stop -Value (@{
                            web = Join-Uri $baseUri.GetLeftPart([System.UriPartial]::Authority) "sitzungsvorbereitung\$tenant"
                            app = "cmisitzungsvorbereitung://"
                            sitzungDetail = "/{Gremium}/{Jahr}/{GUID}"
                            traktandumDetail = "/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}"
                        })
                    } 
                    'Zusammenarbeitdritte' {
                        $data.$tenant.common.appDirectory | Add-Member -MemberType NoteProperty -Name zusammenarbeitdritte -ErrorAction Stop -Value (@{
                            web = Join-Uri $baseUri.GetLeftPart([System.UriPartial]::Authority) "zusammenarbeitdritte\$tenant"
                            aktivitaetDetail = "/Aktivitaet/{GUID}"
                        })
                    }
                    Default {}
                }
            }

            $data.$tenant | Add-Member -MemberType NoteProperty -Name $App -Value ([pscustomobject]@{}) -ErrorAction Stop

            if($PSCmdlet.ShouldProcess($ConfigurationFile, "Add $tenant.$App")){
                $data | ConvertTo-Json -Depth 99 -ErrorAction Stop | Set-Content -Path $ConfigurationFile -ErrorAction Stop
            }
        }
    }
}
