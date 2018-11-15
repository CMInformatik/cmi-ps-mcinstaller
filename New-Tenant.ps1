function New-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium')]
    [OutputType([Tenant], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,

        [parameter(Mandatory = $False, Position = 2)]
        [switch]$Passthru
    )
    Process {
        foreach($name in $TenantName){
            try{
                $tenant = $Configuration.GetTenant($name)
                if(!$tenant){
                    if($PSCmdlet.ShouldProcess($Configuration, "Add tenant $name")){
                        $Configuration.AddTenant($name) | Out-Null
                    }
                }
                if(-not $Passthru){
                    Write-Output $Configuration[$name]
                }
            }
            catch{
                Write-Error $_.Exception
            }
        }
        if($Passthru){
            Write-Output $Configuration
        }
    }
}