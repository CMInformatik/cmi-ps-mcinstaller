function Remove-ReverseProxyConfiguration {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '')] # no system changes
    [CmdletBinding(SupportsShouldProcess = $false, ConfirmImpact = 'None')]
    [OutputType([string])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true )]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [String]$ProxyConfigurationPath 
    )   
    BEGIN {
        function Remove-TenantUrls {
            param (
                $proxyConf,
                $tenantName
            )

            $propertiesToRemove = @(
                "$tenantName`pri",
                "$tenantName`pub",
                "$tenantName`db",
                "$tenantName`sv",
                "$tenantName`zd"
            )

            foreach ($property in $propertiesToRemove) {
                if ($proxyConf.PSObject.Properties.Match($property).Count -gt 0) {
                    $proxyConf.PSObject.Properties.Remove($property)
                }
            }
        }
    }
    PROCESS {
        if (Test-Path -Path $ProxyConfigurationPath) {
            $proxyConf = Get-Content -Path $ProxyConfigurationPath -Raw | ConvertFrom-Json
        } else {
            throw "The file at path $ProxyConfigurationPath does not exist."
        }

        if ($null -eq $proxyConf) {
            throw "Failed to read configuration or configuration is empty."
        }

        foreach ($tenant in $Configuration.Tenants) {
            if ($tenant.Name -in $TenantName) {
                Remove-TenantUrls -proxyConf $proxyConf -tenantName $tenant.Name
            }
        }

        $proxyConf | ConvertTo-Json -Depth 99 | Out-File -FilePath $ProxyConfigurationPath -Force
    }
}
