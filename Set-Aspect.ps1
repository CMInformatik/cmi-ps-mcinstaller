function Set-Aspect {
    <#
    .Synopsis
    Test
    #>
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium', DefaultParameterSetName = "__AllParameterSets")]
    [OutputType([Tenant])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,
    
        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True)]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,
    
        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $True, Position = 3, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$AspectPath,

        [parameter(Mandatory = $True, Position = 4, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true)]
        [object]$Value,

        [parameter(Mandatory = $False, Position = 5, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies
    )
    Process {
        foreach($name in $TenantName){
            try{
                #ToDo: SupportsShouldProcess
                $tenant = $Configuration[$name]
                Write-Verbose "$($tenant.Name).$App.$AspectPath = $Value"
                if(!$tenant.Has($App)){
                    Write-Error "App '$App' is not enabled for tenant $($tenant.Name)" -TargetObject $Configuration
                }
                $tenant[$App].Set($AspectPath, $Value, $EnsureDependencies)
                Write-Output $tenant[$App]
            }
            catch{
                Write-Error $_.Exception
            }
        }
    }
}