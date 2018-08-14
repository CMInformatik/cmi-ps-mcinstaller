function HasProperty {
    [CmdletBinding(DefaultParameterSetName="ByName")] 
    PARAM(
        [Parameter(Mandatory = $true, Position=0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true )]
        [ValidateNotNull()]
        [psobject]$Object,
        [Parameter(Mandatory = $true, Position=1, ValueFromPipelineByPropertyName = $true, ParameterSetName='ByName' )]
        [ValidateNotNullOrEmpty()]
        [string]$Name,
        [Parameter(Mandatory = $true, Position=1, ValueFromPipelineByPropertyName = $true, ParameterSetName='ByPath' )]
        [ValidateNotNullOrEmpty()]
        [string]$Path
    )
    Process {
        #ByName
        if($Name){
            return($Object.PSObject.Properties.Name -contains $Name)
        }
        #ByPath
        $properties = $Path -split '\.'
        $currentObj = $Object
        foreach($property in $properties){
            if($currentObj | HasProperty -Name $property){
                $currentObj = $currentObj.$property
            } else {
                return $false
            }
        }
        return $true
    }
}
