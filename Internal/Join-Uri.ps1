function Join-Uri {
    param([System.Uri]$Uri, [string]$RelativeUri)
    $result = $null
    if(-not [System.Uri]::TryCreate($Uri, $RelativeUri, [ref]$result)){
        throw "Can not join $($Uri.ToString()) and $RelativeUri to a valid uri"
    }
    return ([System.Uri]$result)
}
