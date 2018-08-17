


$viewer = New-Object CMI.PS.SimpleAspect ('viewer', [string], '')
$viewer.ValidationAttributes.Add(
    (New-Object System.Management.Automation.ValidateSetAttribute ('pdfjs', 'browser', '', $null))
)
$editor = New-Object CMI.PS.SimpleAspect ('editor', [string], '')
$editor.ValidationAttributes.Add(
    (New-Object System.Management.Automation.ValidateSetAttribute ('pdftools', '', $null))
)

$pdf = New-Object CMI.PS.ComplexAspect ('pdf')
$pdf.AddAspect((New-Object CMI.PS.SimpleAspect ('inTabs', [bool], $false)))
$pdf.AddAspect($viewer)
$pdf.AddAspect($editor)

$ui = New-Object CMI.PS.ComplexAspect ('ui', [CMI.PS.ConfigControlAttribute]::Private)
$ui.AddAspect($pdf)

$ConfigurationModel[[CMI.PS.App]::Common].AddAspect($ui)