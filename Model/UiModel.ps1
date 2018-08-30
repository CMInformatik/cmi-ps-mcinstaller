


$viewer = New-Object SimpleAspect ('viewer', [string], '')
$viewer.ValidationAttributes.Add(
    (New-Object System.Management.Automation.ValidateSetAttribute ('pdfjs', 'browser', '', $null))
)
$editor = New-Object SimpleAspect ('editor', [string], '')
$editor.ValidationAttributes.Add(
    (New-Object System.Management.Automation.ValidateSetAttribute ('pdftools', '', $null))
)

$pdf = New-Object ComplexAspect ('pdf')
$pdf.AddAspect((New-Object SimpleAspect ('inTabs', [bool], $false)))
$pdf.AddAspect($viewer)
$pdf.AddAspect($editor)

$ui = New-Object ComplexAspect ('ui', [ConfigControlAttribute]::Private)
$ui.AddAspect($pdf)

$ConfigurationModel[[App]::Common].AddAspect($ui)