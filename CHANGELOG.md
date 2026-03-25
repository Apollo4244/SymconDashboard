# Changelog

All notable changes to this project will be documented in this file.

---

## [1.0.5] – Borderless window & kiosk behavior fixes

### Fixed
- **Kiosk mode – resize borders disabled:** In borderless kiosk mode, the resize
  borders are no longer draggable. `GetBorderlessHitTest` returns `HTCLIENT`
  immediately when kiosk is active.
- **Kiosk mode – drag bar hidden:** The drag strip at the top is no longer
  rendered in kiosk mode. WebView2 now fills the entire client area (`0, 0,
  Width, Height`) so there is no movable or right-clickable title area.
- **Kiosk mode – window buttons hidden:** All three caption buttons (kiosk,
  maximize/restore, close) are hidden while kiosk mode is active and restored
  when kiosk mode is exited.
- **Kiosk mode – uses current screen:** Kiosk mode now full-screens on the
  monitor the window is currently on (`Screen.FromHandle(Handle)`), not always
  on the primary monitor. This applies both when toggling and when restoring
  kiosk state on startup.
- **Maximized borderless – resize borders disabled:** When the window is
  maximized in borderless mode, the resize borders are no longer draggable.
  Only the drag strip (HTCAPTION) is kept so that dragging restores and moves
  the window as expected by Windows conventions.
- **Maximized borderless – taskbar no longer covered:** Handling `WM_GETMINMAXINFO`
  limits the maximized size to `Screen.WorkingArea`, so the taskbar stays
  visible on all monitors. Not applied in kiosk mode (intentional full-screen).
- **Maximized borderless – secondary monitor position:** `ptMaxPosition` in
  `WM_GETMINMAXINFO` is now computed relative to the monitor’s own origin
  (`wa.Left − screen.Bounds.Left`, `wa.Top − screen.Bounds.Top`) instead of
  virtual-screen coordinates. Previously, maximizing on a non-primary monitor
  pushed the window completely off-screen.

### Changed
- `ToggleKioskMode`: Kiosk button icon (`_winBtnKiosk.Text`) is only reset to
  `\uE92D` when *exiting* kiosk mode; the update when entering is skipped
  because the button is immediately hidden.
- **Tray menu – borderless toggle:** The alternating “Show title bar” /
  “Hide title bar” item is replaced by a persistent checkmark entry named
  “Borderless mode” (“Rahmenloser Modus” in German). The checkmark reflects
  whether borderless mode is currently active, clearly distinct from the
  separate Kiosk mode toggle.

### Internal
- Added Win32 constant `WM_GETMINMAXINFO = 0x0024`.
- Added private nested structs `POINT` and `MINMAXINFO` for use with
  `Marshal.PtrToStructure<MINMAXINFO>` in the `WM_GETMINMAXINFO` handler.

---

## [1.0.4] – Build artifact cleanup

### Changed
- `dotnet publish` with `-p:PublishSingleFile=true` no longer emits
  `SymconDashboard.pdb` or the NuGet IntelliSense XML files
  (`Microsoft.Web.WebView2.*.xml`) alongside the EXE.
  Controlled via `DebugType` and `AllowedReferenceRelatedFileExtensions`
  in the `.csproj` (active only during single-file publish).

---

## [1.0.3] – Tray icon profile label

### Added
- Tray icon tooltip shows the active profile name:
  *"Symcon Dashboard – profilename"* when launched with `--profile`,
  plain *"Symcon Dashboard"* otherwise.

---

## [1.0.2] – Kiosk mode & command-line options

### Added
- **Kiosk mode** (borderless only): fills the primary screen, sets `TopMost`,
  hides the taskbar entry. Toggled via tray menu or the kiosk caption button
  (`\uE92D` / `\uE92E`). State is persisted across restarts.
- **`--profile <name>`** CLI option: uses `<name>.json` as the settings file
  and `WebView2Data-<name>` as the browser cache folder, allowing multiple
  independent instances with separate configurations.
- **`--no-single-instance`** CLI option: skips the single-instance mutex so
  multiple instances can run without a profile name.
- Pre-kiosk window bounds are saved and restored when exiting kiosk mode.
  The `FormClosing` handler skips overwriting saved bounds while kiosk is active.

---

## [1.0.1] – Zoom control

### Added
- Zoom presets (75 % – 200 %) and a custom zoom input via tray menu.
- Zoom level is persisted in `appsettings.json` and restored on startup.
- Tray menu zoom checkmarks are refreshed live when the menu opens.

---

## [1.0.0] – Initial release

### Added
- Borderless WebView2 shell for IP-Symcon dashboards.
- Borderless mode with custom resize borders, drag strip, and caption buttons
  (close / maximize-restore / kiosk).
- System tray icon with context menu: toggle title bar, change URL, reload,
  reset position, zoom, border color, border width, exit.
- Border color modes: Windows accent color (`system`), auto-detected page
  background (`auto`), or custom hex color.
- Single-instance enforcement via named mutex; second instance brings the
  running window to the foreground.
- Window position, size, maximized state, and zoom factor persisted in
  `appsettings.json` next to the executable.
- First-run URL prompt.
- Error pages for HTTP errors (401, 403, 404, 500, 502, 503) and network
  errors (DNS, connection refused, timeout, …).
- Localized strings (English default + German `Strings.de.resx`).
