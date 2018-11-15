function New-Tenant {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName="__AllParameterSets")]
    [OutputType([Tenant])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName
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
                Write-Output $Configuration[$name]
            }
            catch{
                Write-Error $_.Exception
            }
        }
    }
}