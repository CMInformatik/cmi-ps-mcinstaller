function Remove-App {
        <#
    .SYNOPSIS
        Removes apps from tenants.
    .DESCRIPTION
        Removes apps from tenants.
        When a tenant or app can not be processed, an output on the error stream will be generated.
    .EXAMPLE
        PS> Remove-App -Configuration $config -TenantName cmi,talus,abraxas -App Dossierbrowser,Sitzungsvorbereitung
        Removes the given apps from the tenants cmi, talus and abraxas.
    .EXAMPLE
        PS> Remove-App -Configuration $config -App Dossierbrowser
        Removes the app 'Dossierbrowser' from all tenants.
    .EXAMPLE
        PS> Remove-App -Configuration $config
        Removes all apps (except common) from all tenants.
    .OUTPUTS
        Passthru off: The tenant object or array of tenant objects.
        Passthru on: The configuration object.
    .PARAMETER App
        List of apps to add to the tenants.
        Does nothing, when the app is not present on the tenant.
        When not present, all apps will be removed from the tenants (except common).
    .PARAMETER TenantName
        List of tenant names to operate on.
        When not present, the operation will be applied on all tenants in the configuration.
    .PARAMETER Tenant
        List of tenant objects to operate on.
    .PARAMETER Configuration
        The configuration object to operate on.
    .PARAMETER Passthru
        Instead of the normal output, the configuration object will be returned.
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'High', DefaultParameterSetName="ByConfiguration")]
    [OutputType([Tenant], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName = "ByConfiguration")]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName = "ByTenant")]
        [ValidateNotNull()]
        [Tenant[]]$Tenant,

        [parameter(Mandatory = $False, Position = 1, ValueFromPipelineByPropertyName = $True, ParameterSetName = "ByConfiguration")]
        [AllowNull()]
        [ValidateScript({MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $False, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [App[]]$App,

        [parameter(Mandatory = $False, Position = 3, ParameterSetName = "ByConfiguration")]
        [switch]$Passthru
    )
    Begin{
        if(-not $App){
            # select all apps
            $App = [cmi.mobileclients.config.ModelContract.McSymbols]::Apps | Where-Object { $_ -ne [App]::Common }
        }
    }
    Process {
        if ($PSCmdlet.ParameterSetName -eq 'ByConfiguration') {
            $Tenant = Get-Tenant -TenantName $TenantName -Configuration $Configuration
        }

        # remove app
        foreach ($t in $Tenant) {
            try {
                foreach ($a in $App) {
                    if ($t.Has($a)) {
                        if ($PSCmdlet.ShouldProcess($Configuration, "Remove app $a from tenant $($t.Name)")) {
                            $t.Remove($a)
                        }
                    }
                }
                if (-not $Passthru) {
                    Write-Output $t
                }
            }
            catch {
                Write-Error $_.Exception -TargetObject $t
            }
        }

        if ($Passthru) {
            Write-Output $Configuration
        }
    }
}
