# Write-Host ist i.o., da Script fuer Benutzer-Interaktion ausgelegt ist.
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", "")]
param()


$ErrorActionPreference = "Stop"
Import-Module CMIMCInstaller -ErrorAction Stop
[System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null

function Read-YesNo ([string]$title, [string]$message){
    $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes"
    $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No"
    $options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
    $result = $host.ui.PromptForChoice($title, $message, $options, 0)
    Write-Host -ForegroundColor Yellow -Object "$($options[$result].Label)"
    return ($result -eq 0)
}

function Get-ConfigDestination {
    $OpenFileDialog = New-Object System.Windows.Forms.OpenFileDialog
    $OpenFileDialog.initialDirectory = (Get-Item .).FullName
    $OpenFileDialog.filter = "Json (*.json)|*.json"
    $OpenFileDialog.CheckFileExists = $true
    $OpenFileDialog.ShowDialog() | Out-Null
    $OpenFileDialog.filename
}

Write-Host "Enter the frontend configuration file path." -ForegroundColor Yellow
$config= Get-ConfigDestination | Get-CMIMCConfiguration -Schema $FrontendSchema


# tenant name
$TenantID =  Read-Host "Enter the tenant id"
Write-Host $TenantID -ForegroundColor Yellow
$config | Add-CMIMCTenant -TenantName $TenantID

if(Read-YesNo "Sitzungsvorbereitung" "Enable this app?"){
    $config | Add-CMIMCApp -App Sitzungsvorbereitung -EnsureDependencies | Out-Null
}
else {
    $config | Remove-CMIMCApp -App Sitzungsvorbereitung -ErrorAction SilentlyContinue -Confirm:$false | Out-Null
}

if(Read-YesNo "Dossierbrowser" "Enable this app?"){
    $config | Add-CMIMCApp -App Dossierbrowser -EnsureDependencies | Out-Null
} else {
    $config | Remove-CMIMCApp -App Dossierbrowser -ErrorAction SilentlyContinue -Confirm:$false | Out-Null
}


if(Read-YesNo "Zusammenarbeitdritte" "Enable this app?"){
    $config | Add-CMIMCApp -App Zusammenarbeitdritte -EnsureDependencies | Out-Null
} else {
    $config | Remove-CMIMCApp -App Zusammenarbeitdritte -ErrorAction SilentlyContinue -Confirm:$false | Out-Null
}

$config.WriteToFile(($config.OriginPath.FullName), $true)

Write-Host "Enter the frontend reverse proxy file path." -ForegroundColor Yellow
$config |New-CMIMCReverseProxyConfiguration|Out-File -FilePath (Get-ConfigDestination)

