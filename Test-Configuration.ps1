function Test-Configuration {
    [CmdletBinding(SupportsShouldProcess = $False, ConfirmImpact = 'None')]
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
        [ValidateScript( {MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $False, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Alias('Version')]
        [AxSupport]$AxSupport = [AxSupport]::R16_1
    )
    Process {
        if ($PSCmdlet.ParameterSetName -eq 'ByConfiguration') {
            $Tenant = Get-Tenant -TenantName $TenantName -Configuration $Configuration
        }

        # test tenant
        foreach ($t in $Tenant) {
            try {
                
                Write-Output $t
            }
            catch {
                Write-Error $_.Exception -TargetObject $t
            }
        }
    }
}
