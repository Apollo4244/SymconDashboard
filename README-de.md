# Symcon Dashboard for Windows

🇬🇧 [English version](README.md)

Ein schlanker, rahmenloser Dashboard-Viewer für [IP-Symcon](https://www.symcon.de/) — oder jede andere webbasierte Oberfläche — entwickelt mit .NET 10, WinForms und WebView2.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4) ![Platform](https://img.shields.io/badge/platform-Windows-0078D6) ![License](https://img.shields.io/badge/license-MIT-green)

---

## Funktionen

- **Rahmenloses Fenster** – standardmäßig ohne Titelleiste, ideal für Dashboard-Einsatz
- **Größenänderung & Verschieben** – Größe an allen Kanten anpassbar, Ziehen über den oberen Streifen
- **System-Tray-Integration** – minimiert in die Taskleiste; Doppelklick oder Neustart stellt das Fenster wieder her
- **Einzelinstanz** – ein erneuter Programmstart stellt das bereits laufende Fenster in den Vordergrund
- **Konfigurierbare Rahmenfarbe**
  - Windows-Akzentfarbe
  - Automatisch von der Seitenhintergrundfarbe erkannt
  - Benutzerdefinierte Hex-Farbe
- **Konfigurierbare Rahmenbreite** – Voreinstellungen oder eigener Wert (2–40 px)
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

1. `Symcon Dashboard for Windows.exe` von der [Releases](https://github.com/Apollo4244/SymconDashboard/releases)-Seite herunterladen
2. `Symcon Dashboard for Windows.exe` starten — keine Installation erforderlich
3. Beim ersten Start die URL der IP-Symcon-Weboberäche eingeben (z. B. `http://192.168.1.10:3777/`)

Kein Installer erforderlich.

---

## Bedienung

| Aktion | So geht's |
|---|---|
| Kontextmenü öffnen | Rechtsklick auf das Tray-Icon **oder** Rechtsklick auf den Drag-Streifen |
| Fenster wiederherstellen | Doppelklick auf das Tray-Icon **oder** Programm erneut starten |
| URL ändern | Tray-Menü → *Start-URL ändern…* |
| Titelleiste ein-/ausblenden | Tray-Menü → *Titelleiste einblenden / ausblenden* |
| Rahmenfarbe ändern | Tray-Menü → *Rahmenlos → Farbe* |
| Rahmenbreite ändern | Tray-Menü → *Rahmenlos → Breite* |
| Fensterposition zurücksetzen | Tray-Menü → *Fensterposition zurücksetzen* |
| Beenden | Tray-Menü → *Beenden* |

Die Einstellungen werden in `appsettings.json` neben der ausführbaren Datei gespeichert und automatisch aktualisiert.

---

## Aus dem Quellcode bauen

**Voraussetzungen**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 oder 11

**Klonen und bauen**

```bash
git clone https://github.com/Apollo4244/SymconDashboard.git
cd "SymconDashboard/Symcon Dashboard for Windows"
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
