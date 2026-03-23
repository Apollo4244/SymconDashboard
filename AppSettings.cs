namespace SymconDashboard
{
    public class AppSettings
    {
        public string StartUrl { get; set; } = "http://localhost:3777/";
        public WindowSettings Window { get; set; } = new();
    }

    public class WindowSettings
    {
        public int Left { get; set; } = 100;
        public int Top { get; set; } = 100;
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 800;
        public bool Maximized { get; set; } = false;
        public bool HideTitleBar { get; set; } = true;

        // Rahmenfarbe im Titelleisten-losen Modus:
        // "system"   → Windows Akzentfarbe (DWM)
        // "auto"     → Hintergrundfarbe der geladenen Webseite (via JS)
        // "#RRGGBB"  → beliebige Hex-Farbe
        public string BorderlessBackColor { get; set; } = "auto";

        // Breite des unsichtbaren Randes (Resize-Griffe + Drag-Streifen oben) in Pixeln
        public int BorderSize { get; set; } = 10;

        // Zuletzt erkannte Auto-Farbe als "#rrggbb" – sofortiger Fallback beim nächsten Start,
        // bis die Seite geladen und die Farbe neu erkannt wird. null = noch nie erkannt.
        public string? AutoDetectedColor { get; set; }

        // Zoom-Faktor der WebView2-Ansicht. 1.0 = 100 %, 1.5 = 150 % usw.
        public double ZoomFactor { get; set; } = 1.0;

        // Kiosk-Modus: Fenster füllt den gesamten Primärmonitor (TopMost + Screen.PrimaryScreen.Bounds).
        public bool IsKioskMode { get; set; } = false;
    }
}
