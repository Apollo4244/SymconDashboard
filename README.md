# Symcon Dashboard for Windows

🇩🇪 [Deutsche Version](README-de.md)

A lightweight, borderless dashboard viewer for [IP-Symcon](https://www.symcon.de/) — or any web-based interface — built with .NET 10, WinForms and WebView2.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4) ![Platform](https://img.shields.io/badge/platform-Windows-0078D6) ![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Borderless window** – frameless display by default, ideal for dashboard use
- **Resizable & draggable** – resize from all edges, drag from the top bar
- **System tray integration** – minimizes to tray; double-click or re-launch to restore
- **Single-instance** – launching the app a second time restores the existing window
- **Configurable border color**
  - Windows accent color
  - Auto-detected from the page background
  - Custom hex color
- **Configurable border width** – presets or custom value (2–40 px)
- **Persistent settings** – window position, size, URL and all preferences are saved automatically
- **First-run setup** – prompts for a URL on the first launch
- **Error pages** – friendly error screens for HTTP and network failures
- **Localization** – English and German, automatically selected from Windows language settings

---

## Requirements

| Component | Version |
|---|---|
| Windows | 10 or 11 |
| [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) | any (included with Windows 11 and Microsoft Edge) |

---

## Installation

1. Download `Symcon Dashboard for Windows.exe` from the [Releases](https://github.com/Apollo4244/SymconDashboard/releases) page
2. Run `Symcon Dashboard for Windows.exe` — no installation required
3. On first launch, enter the URL of your IP-Symcon web front-end (e.g. `http://192.168.1.10:3777/`)

No installer required.

---

## Usage

| Action | How |
|---|---|
| Open context menu | Right-click the tray icon **or** right-click the drag bar |
| Restore window | Double-click the tray icon **or** launch the app again |
| Change URL | Tray menu → *Change start URL…* |
| Toggle title bar | Tray menu → *Show / Hide title bar* |
| Change border color | Tray menu → *Borderless → Color* |
| Change border width | Tray menu → *Borderless → Width* |
| Reset window position | Tray menu → *Reset window position* |
| Exit | Tray menu → *Exit* |

Settings are stored in `appsettings.json` next to the executable and are updated automatically.

---

## Building from Source

**Prerequisites**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 or 11

**Clone and build**

```bash
git clone https://github.com/Apollo4244/SymconDashboard.git
cd "SymconDashboard/Symcon Dashboard for Windows"
dotnet build
```

**Run**

```bash
dotnet run
```

Or open `Symcon Dashboard for Windows.csproj` in Visual Studio 2022 or later.

---

## License

MIT License – see [LICENSE](LICENSE) for details.
