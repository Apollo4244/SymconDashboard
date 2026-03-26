# Symcon Dashboard for Windows

🇩🇪 [Deutsche Version](README-de.md)

A lightweight, borderless dashboard viewer for [IP-Symcon](https://www.symcon.de/) — or any web-based interface — built with .NET 10, WinForms and WebView2.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4) ![Platform](https://img.shields.io/badge/platform-Windows-0078D6) ![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Borderless window** – frameless display by default, ideal for dashboard use
- **Resizable & draggable** – resize from all edges, drag from the top strip
- **Drag bar window controls** – four caption buttons in the top strip: Kiosk · Minimize · Maximize/Restore · Close (exits the app). Clicking the Windows taskbar button toggles minimize/restore.
- **System tray integration** – right-click the tray icon for the full menu; double-click or re-launch to restore the window
- **Single-instance** – launching the app a second time restores the existing window
- **Multiple pages** – configure any number of named pages (name + URL); switch instantly via the tray *Pages* submenu, the drag-bar list button (⊞), or the Page Manager dialog. The first entry is the startup page.
- **Page Manager** – dedicated dialog to add, rename, reorder and remove pages; *Apply* persists changes immediately without closing the dialog.
- **Taskbar icon toggle** – independently control whether the app appears in the Windows taskbar, regardless of borderless mode.
- **Configurable border color**
  - Windows accent color
  - Auto-detected from the page background
  - Custom hex color
- **Configurable border width** – presets or custom value (2–40 px)
- **Configurable zoom level** – presets (75–200 %) or custom value (25–500 %), saved across restarts
- **Kiosk mode** – one-click full-screen lockdown: `TopMost` + full current-screen bounds; toggle via drag bar button or tray menu
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

1. Download `SymconDashboard.exe` from the [Releases](https://github.com/Apollo4244/SymconDashboard/releases) page
2. **Move the EXE to a dedicated folder** (e.g. `C:\Tools\SymconDashboard\`) — do not run it directly from your Downloads folder
3. Run `SymconDashboard.exe` — no installation required
4. On first launch, enter the URL of your IP-Symcon web front-end (e.g. `http://192.168.1.10:3777/`)

> **Tip:** The app creates additional files (`appsettings.json`, WebView2 cache) next to the EXE. Keeping it in its own folder prevents clutter in your Downloads folder.

No installer required.

---

## Usage

| Action | How |
|---|---|
| Open context menu | Right-click the tray icon **or** right-click the drag bar |
| Restore window | Double-click the tray icon **or** click the taskbar button **or** launch the app again |
| Minimize window | Drag bar `_` button **or** click the taskbar button |
| Switch page | Tray menu → *Pages* → page name **or** drag-bar ⊞ button |
| Manage pages | Tray menu → *Pages → Manage pages…* |
| Change URL | Tray menu → *Change start URL…* |
| Toggle borderless mode | Tray menu → *Borderless mode* (checkmark) |
| Change border color | Tray menu → *Borderless → Color* |
| Change border width | Tray menu → *Borderless → Width* |
| Change zoom level | Tray menu → *Zoom* **or** Ctrl+Plus / Ctrl+Minus (Ctrl+0 resets to configured level) |
| Toggle kiosk mode | Drag bar kiosk button **or** Tray menu → *Borderless → Kiosk mode* |
| Toggle taskbar icon | Tray menu → *Taskbar icon* (checkmark) |
| Reset window position | Tray menu → *Reset window position* |
| Exit | Close button (✕) in drag bar **or** Tray menu → *Exit* |

Settings are stored in `appsettings.json` next to the executable and are updated automatically.

---

## Command-line Options

| Option | Description |
|---|---|
| `--profile <name>` | Use `<name>.json` as the settings file and a separate WebView2 cache — enables multiple independent instances side by side |
| `--no-single-instance` | Skip the single-instance check entirely (no mutex); useful for scripted or testing scenarios |

Both options can be combined. Examples:

```
SymconDashboard.exe --profile bedroom
SymconDashboard.exe --profile kitchen
SymconDashboard.exe --no-single-instance --profile debug
```

> **Note:** `--no-single-instance` without `--profile` shares the WebView2 cache between instances, which may cause the second instance to fail. Always pair it with `--profile` for reliable parallel use.

---

## Building from Source

**Prerequisites**

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 or 11

**Clone and build**

```bash
git clone https://github.com/Apollo4244/SymconDashboard.git
cd "WebAppHost/Symcon Dashboard for Windows"
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
