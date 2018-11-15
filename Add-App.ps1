function Add-App {
    [CmdletBinding(SupportsShouldProcess = $True, ConfirmImpact = 'Medium')]
    [OutputType([AppConfiguration], [JsonConfiguration])]
    PARAM(
        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName="ByConfiguration")]
        [ValidateNotNull()]
        [JsonConfiguration]$Configuration,

        [parameter(Mandatory = $True, Position = 0, ValueFromPipelineByPropertyName = $True, ValueFromPipeline = $true, ParameterSetName="ByTenant")]
        [ValidateNotNull()]
        [Tenant[]]$Tenant,

        [parameter(Mandatory = $True, Position = 1, ValueFromPipelineByPropertyName = $True, ParameterSetName="ByConfiguration")]
        [ValidateNotNullOrEmpty()]
        [ValidateScript({ MustBeValidTenantName $_ })]
        [String[]]$TenantName,

        [parameter(Mandatory = $True, Position = 2, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [App]$App,

        [parameter(Mandatory = $False, Position = 3, ValueFromPipelineByPropertyName = $True)]
        [ValidateNotNull()]
        [Switch]$EnsureDependencies,

        [parameter(Mandatory = $False, Position = 4, ParameterSetName="ByConfiguration")]
        [switch]$Passthru
    )
    Process {
        if($PSCmdlet.ParameterSetName -eq 'ByConfiguration'){
            # get tenant object by name
            $Tenant = @()
            foreach($name in $TenantName){
                $t = $Configuration.GetTenant($name)
                if($null -eq $t){
                    Write-Error "A tenant with '$name' was not found in the configuration" -TargetObject $Configuration
                } else {
                    $Tenant += $t
                }
            }
        }

        # enable app
        foreach($t in $Tenant){
            try{
                if(!$t.Has($App)){
                    if($PSCmdlet.ShouldProcess($Configuration, "Add app $App to tenant $($t.Name)")){
                        $t.Add($App, $EnsureDependencies)
                    }
                }
                if(-not $Passthru){
                    Write-Output $t[$App]
                }
            }
            catch{
                Write-Error $_.Exception -TargetObject $t
            }
        }

        if($Passthru){
            Write-Output $Configuration
        }
    }
}
