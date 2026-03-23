using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SymconDashboard
{
    public partial class Form1 : Form
    {
        private readonly AppSettings _settings;
        private NotifyIcon _trayIcon = null!;
        private ToolStripMenuItem _trayToggleItem = null!;
        private ToolStripMenuItem _rahmenlosMenu  = null!;
        private ToolStripMenuItem _zoomMenu       = null!;
        private ToolStripMenuItem _kioskMenuItem  = null!;
        private Color?            _autoDetectedColor;
        private Label?            _winBtnKiosk;
        private Label?            _winBtnMaxRestore;
        private Label?            _winBtnClose;
        private EventWaitHandle?  _activateEvent;
        private CancellationTokenSource? _activateCts;

        #region Win32 System-Menü + Borderless Resize/Drag
        private const int  WM_SYSCOMMAND         = 0x0112;
        private const int  WM_NCHITTEST          = 0x0084;
        private const int  WM_NCLBUTTONDOWN      = 0x00A1;
        private const int  WM_NCRBUTTONUP        = 0x00A5;
        // WM_NCHITTEST Rückgabewerte
        private const int HTCLIENT               = 1;
        private const int HTCAPTION              = 2;
        private const int HTLEFT                 = 10;
        private const int HTRIGHT                = 11;
        private const int HTTOP                  = 12;
        private const int HTTOPLEFT              = 13;
        private const int HTTOPRIGHT             = 14;
        private const int HTBOTTOM               = 15;
        private const int HTBOTTOMLEFT           = 16;
        private const int HTBOTTOMRIGHT          = 17;

        private int ResizeBorder  => _settings.Window.BorderSize;
        private int DragBarHeight => Math.Max(_settings.Window.BorderSize, 20);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetColorizationColor(
            out uint color, [MarshalAs(UnmanagedType.Bool)] out bool opaqueBlend);
        #endregion

        public Form1()
        {
            InitializeComponent();
            _settings = AppSettingsService.Load();
            if (_settings.Window.BorderlessBackColor == "auto" &&
                _settings.Window.AutoDetectedColor is { } storedColor)
            {
                try { _autoDetectedColor = ColorTranslator.FromHtml(storedColor); } catch { }
            }
            ApplyWindowBounds();
            LoadAppIcon();
            InitTrayIcon();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        // Win32-Icon aus der .exe laden – dasselbe Icon das <ApplicationIcon> in der .csproj setzt.
        // ExtractAssociatedIcon liest die Win32-Ressource direkt, kein GetManifestResourceStream nötig.
        private void LoadAppIcon()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
                   ?? SystemIcons.Application;
        }

        private void InitTrayIcon()
        {
            _trayToggleItem = new ToolStripMenuItem(TrayToggleLabel());
            _trayToggleItem.Click += (_, _) => ToggleTitleBar();

            var urlItem = new ToolStripMenuItem(Strings.TrayChangeUrl);
            urlItem.Click += (_, _) => ChangeStartUrl();

            var reloadItem = new ToolStripMenuItem(Strings.TrayReload);
            reloadItem.Click += (_, _) => webView.Reload();

            var resetPosItem = new ToolStripMenuItem(Strings.TrayResetPosition);
            resetPosItem.Click += (_, _) => ResetWindowPosition();

            _rahmenlosMenu = new ToolStripMenuItem(Strings.TrayBorderless)
            {
                Enabled = FormBorderStyle == FormBorderStyle.None
            };
            _rahmenlosMenu.DropDownItems.Add(BuildColorModeMenu());
            _rahmenlosMenu.DropDownItems.Add(BuildBorderSizeMenu());
            _rahmenlosMenu.DropDownItems.Add(new ToolStripSeparator());
            _kioskMenuItem = new ToolStripMenuItem(Strings.TrayKioskMode)
            {
                Checked = _settings.Window.IsKioskMode
            };
            _kioskMenuItem.Click += (_, _) => ToggleKioskMode();
            _rahmenlosMenu.DropDownItems.Add(_kioskMenuItem);

            var exitItem = new ToolStripMenuItem(Strings.TrayExit);
            exitItem.Click += (_, _) => Application.Exit();

            var trayMenu = new ContextMenuStrip();
            trayMenu.Opening += TrayMenu_Opening;
            trayMenu.Items.Add(_trayToggleItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(urlItem);
            trayMenu.Items.Add(reloadItem);
            trayMenu.Items.Add(resetPosItem);
            _zoomMenu = BuildZoomMenu();
            trayMenu.Items.Add(_zoomMenu);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(_rahmenlosMenu);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(exitItem);

            _trayIcon = new NotifyIcon
            {
                Text = Program.Profile is { } p ? $"Symcon Dashboard \u2013 {p}" : "Symcon Dashboard",
                Icon = Icon ?? SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            // Doppelklick: Fenster in den Vordergrund bringen
            _trayIcon.DoubleClick += (_, _) =>
            {
                if (!Visible) Show();
                if (WindowState == FormWindowState.Minimized)
                    WindowState = _settings.Window.Maximized
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
                Activate();
            };
        }

        private string TrayToggleLabel() =>
            FormBorderStyle == FormBorderStyle.None
                ? Strings.TrayShowTitleBar
                : Strings.TrayHideTitleBar;

        private ToolStripMenuItem BuildColorModeMenu()
        {
            var menu = new ToolStripMenuItem(Strings.TrayColor);

            void AddMode(string label, string tag)
            {
                var item = new ToolStripMenuItem(label) { Tag = tag };
                item.Checked = string.Equals(_settings.Window.BorderlessBackColor, tag,
                    StringComparison.OrdinalIgnoreCase);
                item.Click += (_, _) => SetBorderColorMode(tag, menu);
                menu.DropDownItems.Add(item);
            }

            AddMode(Strings.TrayColorSystem, "system");
            AddMode(Strings.TrayColorAuto,   "auto");

            var customItem = new ToolStripMenuItem(Strings.TrayCustom) { Tag = (string?)null };
            customItem.Checked = _settings.Window.BorderlessBackColor.StartsWith('#');
            customItem.Click += (_, _) =>
            {
                string current = _settings.Window.BorderlessBackColor.StartsWith('#')
                    ? _settings.Window.BorderlessBackColor : "#1e1e2e";
                string? input = ShowInputDialog("Rahmenfarbe", "Hex-Farbe eingeben (z. B. #2d2d2d):", current);
                if (input is not null)
                    SetBorderColorMode(input.StartsWith('#') ? input : '#' + input, menu);
            };
            menu.DropDownItems.Add(customItem);

            return menu;
        }

        private void SetBorderColorMode(string value, ToolStripMenuItem menu)
        {
            if (value != "auto")
            {
                _autoDetectedColor = null;
                _settings.Window.AutoDetectedColor = null;
            }
            _settings.Window.BorderlessBackColor = value;
            AppSettingsService.Save(_settings);
            ApplyBorderColor();
            if (value == "auto")
                DetectPageBackgroundColor();
            foreach (ToolStripMenuItem item in menu.DropDownItems.OfType<ToolStripMenuItem>())
                item.Checked = item.Tag is string tag
                    ? string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)
                    : value.StartsWith('#');
        }

        private static readonly int[] ZoomPresets       = [75, 90, 100, 110, 125, 150, 175, 200];
        private static readonly int[] BorderSizePresets = [4, 6, 8, 10, 12, 16];

        private ToolStripMenuItem BuildBorderSizeMenu()
        {
            var menu = new ToolStripMenuItem(Strings.TrayBorderWidth);

            foreach (int px in BorderSizePresets)
            {
                var item = new ToolStripMenuItem($"{px} px") { Tag = px };
                item.Checked = _settings.Window.BorderSize == px;
                item.Click += (_, _) => SetBorderSize(px, menu);
                menu.DropDownItems.Add(item);
            }

            menu.DropDownItems.Add(new ToolStripSeparator());

            var customItem = new ToolStripMenuItem(Strings.TrayCustom) { Tag = "custom" };
            customItem.Checked = !BorderSizePresets.Contains(_settings.Window.BorderSize);
            customItem.Click += (_, _) =>
            {
                string? input = ShowInputDialog(Strings.DlgBorderTitle, Strings.DlgBorderPrompt,
                    _settings.Window.BorderSize.ToString());
                if (input is null) return;
                if (int.TryParse(input, out int px) && px is >= 2 and <= 40)
                    SetBorderSize(px, menu);
                else
                    MessageBox.Show(Strings.DlgBorderInvalid,
                        Strings.DlgInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            menu.DropDownItems.Add(customItem);

            return menu;
        }

        private void SetBorderSize(int px, ToolStripMenuItem menu)
        {
            _settings.Window.BorderSize = px;
            AppSettingsService.Save(_settings);
            ApplyWebViewBounds();
            foreach (ToolStripMenuItem item in menu.DropDownItems.OfType<ToolStripMenuItem>())
                item.Checked = item.Tag switch
                {
                    int itemPx   => itemPx == px,
                    "custom"     => !BorderSizePresets.Contains(px),
                    _            => false
                };
        }

        private ToolStripMenuItem BuildZoomMenu()
        {
            var menu = new ToolStripMenuItem(Strings.TrayZoom);

            foreach (int pct in ZoomPresets)
            {
                double factor = pct / 100.0;
                var item = new ToolStripMenuItem($"{pct} %") { Tag = factor };
                item.Checked = Math.Abs(_settings.Window.ZoomFactor - factor) < 0.01;
                item.Click += (_, _) => SetZoom(factor, menu);
                menu.DropDownItems.Add(item);
            }

            menu.DropDownItems.Add(new ToolStripSeparator());

            var customItem = new ToolStripMenuItem(Strings.TrayCustom) { Tag = "custom" };
            customItem.Checked = !ZoomPresets.Any(p => Math.Abs(_settings.Window.ZoomFactor - p / 100.0) < 0.01);
            customItem.Click += (_, _) =>
            {
                int currentPct = (int)Math.Round(_settings.Window.ZoomFactor * 100);
                string? input = ShowInputDialog(Strings.DlgZoomTitle, Strings.DlgZoomPrompt, currentPct.ToString());
                if (input is null) return;
                if (int.TryParse(input, out int pct) && pct is >= 25 and <= 500)
                    SetZoom(pct / 100.0, menu);
                else
                    MessageBox.Show(Strings.DlgZoomInvalid,
                        Strings.DlgInvalidInput, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            menu.DropDownItems.Add(customItem);

            return menu;
        }

        private void SetZoom(double factor, ToolStripMenuItem menu)
        {
            _settings.Window.ZoomFactor = factor;
            if (webView.CoreWebView2 is not null)
                webView.ZoomFactor = factor;
            AppSettingsService.Save(_settings);
            foreach (ToolStripMenuItem item in menu.DropDownItems.OfType<ToolStripMenuItem>())
                item.Checked = item.Tag switch
                {
                    double itemFactor => Math.Abs(itemFactor - factor) < 0.01,
                    "custom"          => !ZoomPresets.Any(p => Math.Abs(factor - p / 100.0) < 0.01),
                    _                 => false
                };
        }

        private void TrayMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (webView.CoreWebView2 is null) return;
            double current = webView.ZoomFactor;
            foreach (ToolStripMenuItem item in _zoomMenu.DropDownItems.OfType<ToolStripMenuItem>())
                item.Checked = item.Tag switch
                {
                    double f => Math.Abs(f - current) < 0.01,
                    "custom" => !ZoomPresets.Any(p => Math.Abs(current - p / 100.0) < 0.01),
                    _        => false
                };
        }

        private bool PromptForUrl()
        {
            string? input = ShowInputDialog(Strings.DlgUrlTitle, Strings.DlgUrlPrompt, _settings.StartUrl);
            if (input is null || input == _settings.StartUrl) return false;
            if (!Uri.TryCreate(input, UriKind.Absolute, out _))
            {
                MessageBox.Show(Strings.DlgUrlInvalid, Strings.DlgError,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            _settings.StartUrl = input;
            AppSettingsService.Save(_settings);
            return true;
        }

        private void ChangeStartUrl()
        {
            if (PromptForUrl())
                webView.CoreWebView2?.Navigate(_settings.StartUrl);
        }

        private void ResetWindowPosition()
        {
            var area = (Screen.PrimaryScreen ?? Screen.AllScreens[0]).WorkingArea;
            WindowState = FormWindowState.Normal;
            Location = new Point(
                area.Left + (area.Width  - Width)  / 2,
                area.Top  + (area.Height - Height) / 2);
            _settings.Window.Maximized = false;
            _settings.Window.Left = Left;
            _settings.Window.Top  = Top;
            AppSettingsService.Save(_settings);
        }

        private string? ShowInputDialog(string title, string prompt, string initialValue = "")
        {
            using var form = new Form
            {
                Text                = title,
                AutoScaleMode       = AutoScaleMode.Font,
                AutoScaleDimensions = new SizeF(7F, 15F),
                FormBorderStyle     = FormBorderStyle.FixedDialog,
                StartPosition       = FormStartPosition.CenterParent,
                ClientSize          = new Size(580, 180),
                MinimizeBox = false, MaximizeBox = false
            };
            var label   = new Label   { Text = prompt,       Left = 12, Top = 14, Width = 556, Height = 20 };
            var textBox = new TextBox { Text = initialValue,  Left = 12, Top = 42, Width = 556 };
            var ok      = new Button  { Text = "OK",          Left = 280, Top = 132, Width = 140, Height = 34,
                                        DialogResult = DialogResult.OK };
            var cancel  = new Button  { Text = Strings.DlgCancel, Left = 428, Top = 132, Width = 140, Height = 34,
                                        DialogResult = DialogResult.Cancel };
            form.Controls.AddRange([label, textBox, ok, cancel]);
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            return form.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
        }

        private void ApplyWindowBounds()
        {
            var win = _settings.Window;

            // FormBorderStyle ZUERST setzen – sonst passt WinForms die Bounds nachträglich an
            // (DWM-Rahmen wird herausgerechnet) und das Fenster schrumpft bei jedem Neustart.
            if (win.HideTitleBar)
            {
                ShowInTaskbar   = false;
                FormBorderStyle = FormBorderStyle.None;
            }

            var bounds = new Rectangle(win.Left, win.Top, win.Width, win.Height);
            if (IsVisibleOnAnyScreen(bounds))
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = bounds;
            }
            else
            {
                // Gespeicherte Position liegt außerhalb aller Monitore → mittig auf Primärmonitor
                StartPosition = FormStartPosition.CenterScreen;
                Size = new Size(win.Width, win.Height);
            }

            if (win.Maximized)
                WindowState = FormWindowState.Maximized;

            ApplyWebViewBounds();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            UpdateWindowButtonAppearance();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_winBtnMaxRestore is not null && FormBorderStyle == FormBorderStyle.None)
                _winBtnMaxRestore.Text = WindowState == FormWindowState.Maximized
                    ? "\uE923" : "\uE922";
        }

        protected override void WndProc(ref Message m)
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)GetBorderlessHitTest(m.LParam);
                    return;
                }

                // Resize-Befehl über SC_SIZE an Windows delegieren (kein WS_THICKFRAME nötig)
                if (m.Msg == WM_NCLBUTTONDOWN)
                {
                    int ht = m.WParam.ToInt32();
                    if (ht is >= HTLEFT and <= HTBOTTOMRIGHT)
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_SYSCOMMAND, (IntPtr)(0xF000 | (ht - 9)), IntPtr.Zero);
                        return;
                    }
                }

                // Rechtsklick auf den Drag-Streifen ("Titelleiste") → Kontextmenü zeigen
                if (m.Msg == WM_NCRBUTTONUP && m.WParam.ToInt32() == HTCAPTION)
                {
                    int sx = unchecked((short)(m.LParam.ToInt32() & 0xFFFF));
                    int sy = unchecked((short)((m.LParam.ToInt32() >> 16) & 0xFFFF));
                    _trayIcon.ContextMenuStrip?.Show(new Point(sx, sy));
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void RestoreAndActivate()
        {
            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized)
                WindowState = _settings.Window.Maximized
                    ? FormWindowState.Maximized : FormWindowState.Normal;
            SetForegroundWindow(Handle);
        }

        private int GetBorderlessHitTest(IntPtr lParam)
        {
            // Screen-Koordinaten korrekt auch für negative Monitor-Positionen auslesen
            int sx = unchecked((short)(lParam.ToInt32() & 0xFFFF));
            int sy = unchecked((short)((lParam.ToInt32() >> 16) & 0xFFFF));
            var pt = PointToClient(new Point(sx, sy));

            // Buttons zuerst prüfen – damit sie auch bei kleinen BorderSize-Werten
            // nicht durch die Resize-Checks überlagert werden.
            if (_winBtnClose is { Visible: true } &&
                pt.Y < ResizeBorder + DragBarHeight &&
                (_winBtnClose.Bounds.Contains(pt) ||
                 _winBtnMaxRestore!.Bounds.Contains(pt) ||
                 _winBtnKiosk!.Bounds.Contains(pt)))
                return HTCLIENT;

            bool atLeft   = pt.X < ResizeBorder;
            bool atRight  = pt.X >= ClientSize.Width  - ResizeBorder;
            bool atTop    = pt.Y < ResizeBorder;
            bool atBottom = pt.Y >= ClientSize.Height - ResizeBorder;

            if (atTop    && atLeft)  return HTTOPLEFT;
            if (atTop    && atRight) return HTTOPRIGHT;
            if (atBottom && atLeft)  return HTBOTTOMLEFT;
            if (atBottom && atRight) return HTBOTTOMRIGHT;
            if (atLeft)   return HTLEFT;
            if (atRight)  return HTRIGHT;
            if (atTop)    return HTTOP;
            if (atBottom) return HTBOTTOM;

            // Drag-Streifen: der Bereich oberhalb von WebView2
            if (pt.Y < ResizeBorder + DragBarHeight)
                return HTCAPTION;

            return HTCLIENT;
        }

        // WebView2 im Borderless-Modus einrücken, damit Resize/Drag-Ränder freibleiben.
        private void ApplyWebViewBounds()
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                webView.Dock   = DockStyle.None;
                webView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                               | AnchorStyles.Left | AnchorStyles.Right;
                webView.SetBounds(
                    ResizeBorder,
                    ResizeBorder + DragBarHeight,
                    ClientSize.Width  - 2 * ResizeBorder,
                    ClientSize.Height - 2 * ResizeBorder - DragBarHeight);
                ApplyBorderColor();
                EnsureWindowButtons();
                PositionWindowButtons();
            }
            else
            {
                webView.Anchor = AnchorStyles.None;
                webView.Dock   = DockStyle.Fill;
                BackColor      = SystemColors.Control;
                if (_winBtnClose is not null)
                {
                    _winBtnKiosk!.Visible      = false;
                    _winBtnMaxRestore!.Visible = false;
                    _winBtnClose.Visible       = false;
                }
            }
        }

        private void EnsureWindowButtons()
        {
            if (_winBtnClose is null)
            {
                _winBtnKiosk = MakeWinBtn(_settings.Window.IsKioskMode ? "\uE92E" : "\uE92D", ToggleKioskMode);
                _winBtnMaxRestore = MakeWinBtn("\uE922", () =>
                    WindowState = WindowState == FormWindowState.Maximized
                        ? FormWindowState.Normal : FormWindowState.Maximized);
                _winBtnClose = MakeWinBtn("\uE8BB", Hide);
                Controls.AddRange([_winBtnKiosk, _winBtnMaxRestore, _winBtnClose]);
            }
            else
            {
                _winBtnKiosk!.Visible      = true;
                _winBtnMaxRestore!.Visible = true;
                _winBtnClose.Visible       = true;
            }
            UpdateWindowButtonAppearance();
        }

        private Label MakeWinBtn(string icon, Action onClick)
        {
            bool isClose = icon == "\uE8BB";
            var lbl = new Label
            {
                Text      = icon,
                Font      = new Font("Segoe MDL2 Assets", 8f, FontStyle.Regular, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false,
                BackColor = BackColor,
                Cursor    = Cursors.Default,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            lbl.MouseEnter += (_, _) =>
            {
                lbl.BackColor = isClose
                    ? Color.FromArgb(0xC4, 0x2B, 0x1C)
                    : IsColorDark(BackColor)
                        ? Color.FromArgb(Math.Min(BackColor.R + 40, 255),
                                         Math.Min(BackColor.G + 40, 255),
                                         Math.Min(BackColor.B + 40, 255))
                        : Color.FromArgb(Math.Max(BackColor.R - 40, 0),
                                         Math.Max(BackColor.G - 40, 0),
                                         Math.Max(BackColor.B - 40, 0));
            };
            lbl.MouseLeave += (_, _) => lbl.BackColor = BackColor;
            lbl.Click      += (_, _) => onClick();
            return lbl;
        }

        private static bool IsColorDark(Color c) =>
            0.299 * c.R + 0.587 * c.G + 0.114 * c.B < 128;

        private void PositionWindowButtons()
        {
            if (_winBtnClose is null) return;
            const int w    = 36;
            const int vPad = 1;
            int h     = DragBarHeight + ResizeBorder - vPad;
            int top   = vPad;
            int right = ClientSize.Width - ResizeBorder;
            _winBtnClose.SetBounds(right - w,           top, w, h);
            _winBtnMaxRestore!.SetBounds(right - 2 * w, top, w, h);
            _winBtnKiosk!.SetBounds(right - 3 * w,      top, w, h);
        }

        private void UpdateWindowButtonAppearance()
        {
            if (_winBtnClose is null) return;
            bool isDark = IsColorDark(BackColor);
            Color fg = isDark ? Color.White : Color.Black;
            Color bg = BackColor;
            _winBtnKiosk!.ForeColor      = fg;  _winBtnKiosk.BackColor       = bg;
            _winBtnMaxRestore!.ForeColor = fg;  _winBtnMaxRestore.BackColor  = bg;
            _winBtnClose.ForeColor       = fg;  _winBtnClose.BackColor       = bg;
            _winBtnMaxRestore.Text = WindowState == FormWindowState.Maximized
                ? "\uE923" : "\uE922";
        }

        // Rahmenfarbe je nach BorderlessBackColor-Einstellung anwenden.
        private void ApplyBorderColor()
        {
            if (FormBorderStyle != FormBorderStyle.None) return;
            BackColor = _settings.Window.BorderlessBackColor switch
            {
                "system" => GetSystemAccentColor(),
                "auto"   => _autoDetectedColor ?? Color.FromArgb(0x1e, 0x1e, 0x2e),
                var hex  => TryParseHtmlColor(hex)
            };
        }

        // Windows-Akzentfarbe via DWM auslesen.
        private static Color GetSystemAccentColor()
        {
            try
            {
                DwmGetColorizationColor(out uint c, out _);
                // c ist ARGB – wir ignorieren den Alpha-Kanal
                return Color.FromArgb((byte)(c >> 16), (byte)(c >> 8), (byte)c);
            }
            catch
            {
                return SystemColors.ActiveCaption;
            }
        }

        private static Color TryParseHtmlColor(string hex)
        {
            try   { return ColorTranslator.FromHtml(hex); }
            catch { return Color.FromArgb(0x1e, 0x1e, 0x2e); }
        }

        // Hintergrundfarbe der geladenen Seite per JavaScript ermitteln (Modus "auto").
        private async void DetectPageBackgroundColor()
        {
            if (_settings.Window.BorderlessBackColor != "auto") return;
            if (FormBorderStyle != FormBorderStyle.None) return;

            // SPA-Frameworks (Vue, React, …) setzen die Hintergrundfarbe erst nach dem
            // ersten Render-Durchlauf. Kurz warten, bevor wir getComputedStyle abfragen.
            await Task.Delay(500);

            if (IsDisposed || _settings.Window.BorderlessBackColor != "auto") return;
            if (FormBorderStyle != FormBorderStyle.None) return;

            try
            {
                string json = await webView.CoreWebView2.ExecuteScriptAsync("""
                    (function () {
                        let bg = getComputedStyle(document.documentElement).backgroundColor;
                        if (!bg || bg === 'rgba(0, 0, 0, 0)')
                            bg = getComputedStyle(document.body).backgroundColor;
                        return bg;
                    })()
                    """);
                string css = json.Trim('"');
                if (css is "transparent" or "rgba(0, 0, 0, 0)")
                {
                    _autoDetectedColor = Color.White;
                }
                else
                {
                    var m = Regex.Match(css, @"rgba?\((\d+),\s*(\d+),\s*(\d+)");
                    if (!m.Success) return;
                    _autoDetectedColor = Color.FromArgb(
                        int.Parse(m.Groups[1].Value),
                        int.Parse(m.Groups[2].Value),
                        int.Parse(m.Groups[3].Value));
                }
                BackColor = _autoDetectedColor.Value;
                _settings.Window.AutoDetectedColor =
                    $"#{_autoDetectedColor.Value.R:x2}{_autoDetectedColor.Value.G:x2}{_autoDetectedColor.Value.B:x2}";
                AppSettingsService.Save(_settings);
            }
            catch { }
        }

        // System-Menü oder Tray: Titelleiste ein-/ausblenden
        private void ToggleTitleBar()
        {
            bool hide = FormBorderStyle != FormBorderStyle.None;
            if (!hide && _settings.Window.IsKioskMode)
                ToggleKioskMode();
            ShowInTaskbar   = !hide;
            FormBorderStyle = hide ? FormBorderStyle.None : FormBorderStyle.Sizable;
            ApplyWebViewBounds();
            _settings.Window.HideTitleBar = hide;
            AppSettingsService.Save(_settings);
            _trayToggleItem.Text = TrayToggleLabel();
            _rahmenlosMenu.Enabled = hide;
        }

        private void ToggleKioskMode()
        {
            bool enter = !_settings.Window.IsKioskMode;
            if (enter)
            {
                // Bounds vor dem Kiosk-Modus sichern
                Rectangle save = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                _settings.Window.Left   = save.Left;
                _settings.Window.Top    = save.Top;
                _settings.Window.Width  = save.Width;
                _settings.Window.Height = save.Height;
                TopMost = true;
                Bounds  = Screen.PrimaryScreen!.Bounds;
            }
            else
            {
                TopMost = false;
                Bounds  = new Rectangle(
                    _settings.Window.Left, _settings.Window.Top,
                    _settings.Window.Width, _settings.Window.Height);
            }
            _settings.Window.IsKioskMode = enter;
            ApplyWebViewBounds();
            AppSettingsService.Save(_settings);
            _kioskMenuItem.Checked  = enter;
            _winBtnKiosk!.Text      = enter ? "\uE92E" : "\uE92D";
        }

        // Prüft ob die obere Kante des Fensters auf einem der aktuellen Monitore sichtbar ist
        private static bool IsVisibleOnAnyScreen(Rectangle bounds)
        {
            var titleBar = new Rectangle(bounds.Left, bounds.Top, bounds.Width, 30);
            return Screen.AllScreens.Any(screen =>
                Rectangle.Intersect(screen.WorkingArea, titleBar).Width >= 100);
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            string userDataFolder = Program.DataFolder;

            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder);

            await webView.EnsureCoreWebView2Async(env);
            webView.ZoomFactor = _settings.Window.ZoomFactor;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            if (Program.ActivateEventName is { } evtName)
            {
                _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, evtName);
                _activateCts = new CancellationTokenSource();
                _ = Task.Run(() =>
                {
                    WaitHandle[] handles = [_activateEvent, _activateCts.Token.WaitHandle];
                    while (WaitHandle.WaitAny(handles) == 0)
                        if (!IsDisposed) BeginInvoke(RestoreAndActivate);
                });
            }

            if (AppSettingsService.IsFirstRun)
                PromptForUrl();
            webView.CoreWebView2.Navigate(_settings.StartUrl);
            if (_settings.Window.IsKioskMode)
            {
                TopMost = true;
                Bounds  = Screen.PrimaryScreen!.Bounds;
                ApplyWebViewBounds();
            }
        }

        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // HttpStatusCode == 0 → lokale Navigation z.B. durch NavigateToString → ignorieren
                if (e.HttpStatusCode == 0 || e.HttpStatusCode < 400)
                {
                    if (e.HttpStatusCode is > 0 and < 400)
                        DetectPageBackgroundColor();
                    return;
                }

                // HTTP-Fehler (404, 500, …)
                string url = webView.Source?.ToString() ?? _settings.StartUrl;
                string message = e.HttpStatusCode switch
                {
                    401 => Strings.ErrHttp401,
                    403 => Strings.ErrHttp403,
                    404 => Strings.ErrHttp404,
                    500 => Strings.ErrHttp500,
                    502 => Strings.ErrHttp502,
                    503 => Strings.ErrHttp503,
                    _   => string.Format(Strings.ErrHttpGeneric, e.HttpStatusCode)
                };
                webView.NavigateToString(BuildErrorPage(url, $"HTTP {e.HttpStatusCode}", message));
            }
            else
            {
                // Netzwerkfehler (kein Server, DNS, Timeout, …)
                string url = webView.Source?.ToString() ?? _settings.StartUrl;
                string message = e.WebErrorStatus switch
                {
                    CoreWebView2WebErrorStatus.HostNameNotResolved =>
                        Strings.ErrDnsNotResolved,
                    CoreWebView2WebErrorStatus.CannotConnect =>
                        Strings.ErrCannotConnect,
                    CoreWebView2WebErrorStatus.ConnectionReset =>
                        Strings.ErrConnectionReset,
                    CoreWebView2WebErrorStatus.Disconnected =>
                        Strings.ErrDisconnected,
                    CoreWebView2WebErrorStatus.Timeout =>
                        Strings.ErrTimeout,
                    CoreWebView2WebErrorStatus.ServerUnreachable =>
                        Strings.ErrServerUnreachable,
                    CoreWebView2WebErrorStatus.OperationCanceled =>
                        Strings.ErrOperationCanceled,
                    _ => string.Format(Strings.ErrNetworkGeneric, e.WebErrorStatus)
                };
                webView.NavigateToString(BuildErrorPage(url, Strings.ErrNetworkTitle, message));
            }
        }

        private static string BuildErrorPage(string url, string title, string message) => $$"""
            <!DOCTYPE html>
            <html lang="de">
            <head>
                <meta charset="UTF-8">
                <title>{{title}}</title>
                <style>
                    * { box-sizing: border-box; margin: 0; padding: 0; }
                    body {
                        font-family: 'Segoe UI', sans-serif;
                        background: #1e1e2e;
                        color: #cdd6f4;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        height: 100vh;
                    }
                    .card {
                        background: #313244;
                        border-radius: 12px;
                        padding: 40px 50px;
                        max-width: 520px;
                        width: 90%;
                        text-align: center;
                        box-shadow: 0 8px 32px rgba(0,0,0,0.5);
                    }
                    .icon { font-size: 3rem; margin-bottom: 16px; }
                    h1 { font-size: 1.4rem; color: #f38ba8; margin-bottom: 12px; }
                    p  { font-size: 0.95rem; color: #a6adc8; margin-bottom: 8px; line-height: 1.5; }
                    .url {
                        font-size: 0.78rem; color: #6c7086;
                        word-break: break-all; margin-bottom: 28px;
                    }
                    button {
                        background: #89b4fa; color: #1e1e2e;
                        border: none; border-radius: 8px;
                        padding: 10px 30px; font-size: 0.95rem;
                        font-weight: 600; cursor: pointer;
                    }
                    button:hover { background: #b4d0fe; }
                </style>
            </head>
            <body>
                <div class="card">
                    <div class="icon">⚠️</div>
                    <h1>{{title}}</h1>
                    <p>{{message}}</p>
                    <p class="url">{{url}}</p>
                    <button onclick="window.location.href='{{url}}'">{{Strings.ErrPageRetry}}</button>
                </div>
            </body>
            </html>
            """;

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _activateCts?.Cancel();
            _activateEvent?.Dispose();

            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            _settings.Window.Maximized = WindowState == FormWindowState.Maximized;
            _settings.Window.HideTitleBar = FormBorderStyle == FormBorderStyle.None;
            if (webView.CoreWebView2 is not null)
                _settings.Window.ZoomFactor = webView.ZoomFactor;

            // Im Kiosk-Modus die vorab gesicherten Bounds beibehalten (keine Bildschirmgröße speichern)
            if (!_settings.Window.IsKioskMode)
            {
                Rectangle saveBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                _settings.Window.Left = saveBounds.Left;
                _settings.Window.Top = saveBounds.Top;
                _settings.Window.Width = saveBounds.Width;
                _settings.Window.Height = saveBounds.Height;
            }

            AppSettingsService.Save(_settings);
        }
    }
}
