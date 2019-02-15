function Get-Tenant {
        <#
    .SYNOPSIS
        Selects tenants from a configuration.
    .OUTPUTS
        Tenant objects or null.
    .PARAMETER TenantName
        Name of the tenants to select. When null, all tenants will be selected.
    .PARAMETER Configuration
        The configuration object to operate on.
    #>
    [CmdletBinding(SupportsShouldProcess = $False, ConfirmImpact = 'None')]
    [OutputType([Tenant])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName = "ByConfiguration")]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $False, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [AllowNull()]
        [ValidateScript( {MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName
    )
    Process {
        # get tenant object by name
        $Tenant = @()
        # if tenant name is null, select all tenants
        if (-not $TenantName) {
            $Tenant = $Configuration.Tenants
        }
        else {
            foreach ($name in $TenantName) {
                $t = $Configuration.GetTenant($name)
                if ($null -eq $t) {
                    Write-Error "A tenant with '$name' was not found in the configuration" -TargetObject $Configuration
                }
                else {
                    $Tenant += $t
                }
            } # end foreach
        }
        Write-Output $Tenant
    }
}
