function MustBeValidTenantName {
    <#
.EXAMPLE
[parameter( Position = 1, ValueFromPipeline = $True)]
[ValidateScript( { MustBeValidTenantName $_ })]
[String]$TenantName
    #>
    PARAM(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string]$TenantName,
        [AllowNull()]
        [Parameter(Mandatory = $false)]
        [string]$ErrMsg = $null
    )
    Process {
        if ([string]::IsNullOrWhiteSpace($TenantName) -or ![JsonConfiguration]::TenantNamePattern.Match($TenantName)) {
            if ([string]::IsNullOrWhiteSpace($ErrMsg)) {
                Throw [System.Management.Automation.ValidationMetadataException] "'$TenantName' entspricht nicht der Vorlage $([JsonConfiguration]::TenantNamePattern)"
            }
            Throw [System.Management.Automation.ValidationMetadataException] $ErrMsg
        }
        return $true
    }
}