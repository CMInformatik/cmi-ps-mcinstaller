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
    $OpenFileDialog.initialDirectory = [environment]::getfolderpath("Desktop")
    $OpenFileDialog.filter = "Json (*.json)|*.json"
    $OpenFileDialog.CheckFileExists = $false
    $OpenFileDialog.ShowDialog() | Out-Null
    $OpenFileDialog.filename
}

# tenant name
$TenantID =  Read-Host "Enter the tenant id"
Write-Host $TenantID -ForegroundColor Yellow

# function level
$AxVersion = $null
do {
    $options = [Enum]::GetNames([AxSupport])
    $input = Read-Host "Enter the CMI Server function level: $($options -join ', ')"
    if($options -contains $input){
        $AxVersion = [AxSupport]::$input
        Write-Host $AxVersion -ForegroundColor Yellow
        break;
    }
    Write-Host "Invalid function level" -ForegroundColor Red
} while (!$AxVersion)

$config= New-CMIMCConfiguration -Schema $DefaultSchema
$config | Add-CMIMCTenant -TenantName $TenantID

# Common
$commonOptional = @('service.allowDokumenteOpenExternal')

foreach($feature in (Get-CMIMCFeature -App Common -AxSupport $AxVersion)){
    $enable = $true
    if($commonOptional -contains $feature.AspectPath){
        $enable = Read-YesNo $feature.Name "Enable this feature?"
    }
    if($enable){
        $config | Enable-CMIMCCommonFeature -Feature $feature.Name | Out-Null
    } else {
        $config | Disable-CMIMCCommonFeature -Feature $feature.Name  | Out-Null
    }
}

# BuiltIn
if(Read-YesNo "account.changePassword" "Enable this feature?"){
    $config | Set-CMIMCAspect -App Common -AspectPath "account.changePassword" -Value $true -EnsureDependencies | Out-Null
}
if(Read-YesNo "account.resetPassword" "Enable this feature?"){
    $config | Set-CMIMCAspect -App Common -AspectPath "account.resetPassword" -Value $true -EnsureDependencies | Out-Null
}

# SV
$svOptional = @('service.allowDokumenteAnnotations')
if(Read-YesNo "Sitzungsvorbereitung" "Enable this app?"){
    $config | Add-CMIMCApp -App Sitzungsvorbereitung -EnsureDependencies | Out-Null
    foreach($feature in (Get-CMIMCFeature -App Sitzungsvorbereitung -AxSupport $AxVersion)){
        $enable = $true
        if($svOptional -contains $feature.AspectPath){
            $enable = Read-YesNo $feature.Name "Enable this feature?"
        }
        if($enable){
            $config | Enable-CMIMCSitzungsvorbereitungFeature -Feature $feature.Name | Out-Null
        } else {
            $config | Disable-CMIMCSitzungsvorbereitungFeature -Feature $feature.Name  | Out-Null
        }
    }
}

# DB, ZD
if(Read-YesNo "Dossierbrowser" "Enable this app?"){
    $config | Add-CMIMCApp -App Dossierbrowser -EnsureDependencies | Out-Null
}
if(Read-YesNo "Zusammenarbeitdritte" "Enable this app?"){
    $config | Add-CMIMCApp -App Zusammenarbeitdritte -EnsureDependencies | Out-Null
}

$config.WriteToFile((Get-ConfigDestination), $false)