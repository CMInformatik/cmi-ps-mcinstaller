# cmi-ps-mcinstaller
Powershell-Modul zur Installation der Mobilen Clients.

Der derzeitige Stand umfasst primaer das Erzeugen und Bearbeiten einer Konfigurationsdatei fuer die mobilen Clients.
Das IIS-Setup und andere Installationsschritte sind (noch?) nicht enthalten.

**Achtung: Das Interface ist noch nicht stabil, es gibt noch keinen 1.0-Release**

```powershell
# Modul import
Import-Module <path to>\CMIMCInstaller

# Kommandos anzeigen
Get-Command -Module CMIMCInstaller

# Hilfe zu einem Kommando
Get-Help New-CMIMCCOnfiguration
```

## Einsteig ##

Bestehende Konfigurationen (*config.mandanten.json*) können mit *Get-CMIMCConfiguration* geladen werden oder mit *New-CMIMCConfiguration* erzeugt werden.
Beide Kommandos liefern ein **[JsonConfiguration]**-Objekt. Die meisten anderen Kommandos erwarten ein **[JsonConfiguration]**-Objekt um Änderungen an der Konfiguration vorzunehmen.
Um ein Kommando zu zwingen, das  **[JsonConfiguration]**-Objekt als Return-Objekt zu liefen, wird der Parameter **-Passthru** verwendet.
Dies ist vorallem in der Pipeline hilfreich. Mit *Save-CMIMCConfiguration* werden Änderungen an der Konfiguration gespeichert.

Das folgende Beispiel tut folgendes:

* Neue Konfiguration erzeugen
* Die Mandanten schwerzenwil und schwerzenwiltest hinzufuegen
* Für beide Mandanten die Apps Dossierbrowser und Sitzungsvorbereitung aktivieren
* Für beide Mandanten die Features allowDokumenteAddNew, allowDokumenteAddNewVersion und allowDokumenteAnnotations aktivieren
* Die Konfiguration als Datei speichern

```powershell
New-CMIMCConfiguration |
Add-CMIMCTenant -Name 'schwerzenwil', 'schwerzenwiltest' -Passthru |
Add-CMIMCApp -App Dossierbrowser,Sitzungsvorbereitung -Passthru |
Enable-CMIMCSitzungsvorbereitungFeature -Feature allowDokumenteAddNew,allowDokumenteAddNewVersion,allowDokumenteAnnotations -Passthru |
Save-CMIMCConfiguration -Path .\config.mandanten.json 
```

Operationen koennen auf bestimmte Mandaten mit dem Parameter **-TenantName** eingeschraenkt werden.
Das folgende Beispiel aktiviert Zusammenarbeitdritte nur für den Mandanten *schwerzenwil*:

```powershell
Get-CMIMCConfiguration -Path .\config.mandanten.json |
Add-CMIMCApp -TenantName 'schwerzenwil' -App Zusammenarbeitdritte -Passthru |
Save-CMIMCConfiguration -Path .\config.mandanten.json -Force
```

