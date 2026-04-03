# Symcon Dashboard for Windows

🇬🇧 [English version](README.md)

Ein schlanker, rahmenloser Dashboard-Viewer für [IP-Symcon](https://www.symcon.de/) — oder jede andere webbasierte Oberfläche — entwickelt mit .NET 10, WinForms und WebView2.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4) ![Platform](https://img.shields.io/badge/platform-Windows-0078D6) ![License](https://img.shields.io/badge/license-MIT-green)

---

## Funktionen

- **Rahmenloses Fenster** – standardmäßig ohne Titelleiste, ideal für Dashboard-Einsatz
- **Größenänderung & Verschieben** – Größe an allen Kanten anpassbar, Ziehen über den oberen Streifen
- **Kantenfang** – beim Verschieben oder Skalieren rasten Fensterkanten automatisch an den WorkingArea-Grenzen jedes angeschlossenen Monitors ein, sobald sie sich auf unter 16 px annähern; 32 px vom Rand wegziehen zum Lösen
- **Drag-Leisten-Schaltflächen** – vier Steuerschaltflächen im oberen Streifen: Kiosk · Minimieren · Maximieren/Wiederherstellen · Schließen (beendet die App). Ein Klick auf das Taskleistensymbol wechselt zwischen Minimieren und Wiederherstellen.
- **System-Tray-Integration** – Rechtsklick auf das Tray-Icon öffnet das vollständige Menü; Doppelklick oder Neustart stellt das Fenster wieder her
- **Einzelinstanz** – ein erneuter Programmstart stellt das bereits laufende Fenster in den Vordergrund
- **Mehrere Seiten** – beliebig viele benannte Seiten (Name + URL) konfigurierbar; Umschalten über das Tray-Untermenü *Seiten*, die ⊞-Schaltfläche in der Drag-Leiste oder den Seiten-Manager. Der erste Eintrag ist die Startseite.
- **Seiten-Manager** – eigener Dialog zum Hinzufügen, Umbenennen, Sortieren und Entfernen von Seiten; *Übernehmen* speichert sofort, ohne den Dialog zu schließen.
- **Taskleisten-Symbol** – unabhängig vom rahmenlosen Modus steuerbar, ob die App in der Windows-Taskleiste erscheint.
- **Konfigurierbare Rahmenfarbe**
  - Windows-Akzentfarbe
  - Automatisch von der Seitenhintergrundfarbe erkannt
  - Benutzerdefinierte Hex-Farbe
- **Konfigurierbare Rahmenbreite** – Voreinstellungen oder eigener Wert (2–40 px)
- **Konfigurierbarer Zoom** – Voreinstellungen (75–200 %) oder eigener Wert (25–500 %), wird beim Neustart wiederhergestellt
- **Kiosk-Modus** – Vollbild-Sperrung per Klick: `TopMost` + voller aktueller Monitor; umschalten über Drag-Leisten-Schaltfläche oder Tray-Menü
- **Einstellungen werden gespeichert** – Fensterposition, -größe, URL und alle Optionen werden automatisch gesichert
- **Erststart-Einrichtung** – beim ersten Start wird nach einer URL gefragt
- **Fehlerseiten** – übersichtliche Fehlerseiten bei HTTP- und Netzwerkfehlern
- **Lokalisierung** – Englisch und Deutsch, automatisch anhand der Windows-Spracheinstellung gewählt

---

## Voraussetzungen

| Komponente | Version |
|---|---|
| Windows | 10 oder 11 |
| [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) | beliebig (in Windows 11 und Microsoft Edge enthalten) |

---

## Installation

1. `SymconDashboard.exe` von der [Releases](https://github.com/Apollo4244/SymconDashboard/releases)-Seite herunterladen
2. **Die EXE in einen eigenen Ordner verschieben** (z. B. `C:\Tools\SymconDashboard\`) — nicht direkt aus dem Download-Ordner ausführen
3. `SymconDashboard.exe` starten — keine Installation erforderlich
4. Beim ersten Start die URL der IP-Symcon-Weboberfläche eingeben (z. B. `http://192.168.1.10:3777/`)

> **Hinweis:** Die App legt zusätzliche Dateien (`appsettings.json`, WebView2-Cache) neben der EXE ab. Ein eigener Ordner verhindert Unordnung im Download-Ordner.

Kein Installer erforderlich.

---

## Bedienung

| Aktion | So geht's |
|---|---|
| Kontextmenü öffnen | Rechtsklick auf das Tray-Icon **oder** Rechtsklick auf den Drag-Streifen |
| Fenster wiederherstellen | Doppelklick auf das Tray-Icon **oder** Taskleistensymbol anklicken **oder** Programm erneut starten |
| Fenster minimieren | `_`-Schaltfläche in der Drag-Leiste **oder** Taskleistensymbol anklicken |
| Seite wechseln | Tray-Menü → *Seiten* → Seitenname **oder** ⊞-Schaltfläche in der Drag-Leiste |
| Seiten verwalten | Tray-Menü → *Seiten → Seiten verwalten…* |
| Rahmenlosen Modus umschalten | Tray-Menü → *Rahmenloser Modus* (Häkchen) |
| Rahmenfarbe ändern | Tray-Menü → *Rahmenlos → Farbe* |
| Rahmenbreite ändern | Tray-Menü → *Rahmenlos → Breite* |
| Zoom ändern | Tray-Menü → *Zoom* **oder** Strg+Plus / Strg+Minus (Strg+0 setzt auf konfigurierten Zoom zurück) |
| Kiosk-Modus umschalten | Kiosk-Schaltfläche in der Drag-Leiste **oder** Tray-Menü → *Rahmenlos → Kiosk-Modus* |
| Taskleisten-Symbol umschalten | Tray-Menü → *Taskleisten-Symbol* (Häkchen) |
| Fensterposition zurücksetzen | Tray-Menü → *Fensterposition zurücksetzen* |
| Beenden | Schließen-Schaltfläche (✕) in der Drag-Leiste **oder** Tray-Menü → *Beenden* |

Die Einstellungen werden in `appsettings.json` neben der ausführbaren Datei gespeichert und automatisch aktualisiert.

---

## Kommandozeilenoptionen

| Option | Beschreibung |
|---|---|
| `--profile <Name>` | Verwendet `<Name>.json` als Einstellungsdatei und einen eigenen WebView2-Cache – ermöglicht mehrere unabhängige Instanzen gleichzeitig |
| `--no-single-instance` | Übergeht den Single-Instance-Check (kein Mutex); nützlich für Skripte oder Testzwecke |

Beide Optionen können kombiniert werden. Beispiele:

```
SymconDashboard.exe --profile schlafzimmer
SymconDashboard.exe --profile küche
SymconDashboard.exe --no-single-instance --profile debug
```

> **Hinweis:** `--no-single-instance` ohne `--profile` teilt den WebView2-Cache zwischen den Instanzen, was zum Fehler der zweiten Instanz führen kann. Für zuverlässigen Parallelbetrieb immer mit `--profile` kombinieren.

---

## Aus dem Quellcode bauen

**Voraussetzungen**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 oder 11

**Klonen und bauen**

```bash
git clone https://github.com/Apollo4244/SymconDashboard.git
cd "WebAppHost/Symcon Dashboard for Windows"
dotnet build
```

**Starten**

```bash
dotnet run
```

Oder `Symcon Dashboard for Windows.csproj` in Visual Studio 2022 oder neuer öffnen.

---

## Lizenz

MIT-Lizenz – Details siehe [LICENSE](LICENSE).
