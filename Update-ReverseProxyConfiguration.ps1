function Update-ReverseProxyConfiguration {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '')] # no system changes
    [CmdletBinding(SupportsShouldProcess = $false, ConfirmImpact = 'None')]
    [OutputType([string])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true )]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        #[ValidateScript({ MustBeValidTenantName $_ })]
        [Alias('Name')]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 3, ValueFromPipelineByPropertyName = $True)]
        #[ValidateScript({ MustBeValidTenantName $_ })]
        [String]$ProxyConfigurationPath,

        [parameter(Mandatory = $false, Position = 2, ValueFromPipelineByPropertyName = $True )]
        [ValidateNotNull()]
        [uri]$RelayServer = [uri]"https://relay.cmiaxioma.ch"

    )   
    BEGIN {
        
    }
    PROCESS {
        $Configuration | Remove-ReverseProxyConfiguration -TenantName $TenantName -ProxyConfigurationPath $ProxyConfigurationPath
        $Configuration|Add-ReverseProxyConfiguration -TenantName $TenantName -RelayServer $RelayServer -ProxyConfigurationPath $ProxyConfigurationPath

    }

}
