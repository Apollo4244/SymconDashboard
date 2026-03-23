using System.Runtime.InteropServices;

namespace SymconDashboard
{
    internal static class Program
    {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        internal static string? ActivateEventName;
        internal static string  DataFolder = Path.Combine(AppContext.BaseDirectory, "WebView2Data");
        internal static string? Profile;

        [STAThread]
        static void Main(string[] args)
        {
            int pi = Array.IndexOf(args, "--profile");
            string? profile = pi >= 0 && pi + 1 < args.Length ? args[pi + 1] : null;
            bool noSingleInstance = args.Contains("--no-single-instance", StringComparer.OrdinalIgnoreCase);

            if (profile is not null)
            {
                Profile = profile;
                AppSettingsService.FilePath = Path.Combine(AppContext.BaseDirectory, $"{profile}.json");
                DataFolder = Path.Combine(AppContext.BaseDirectory, $"WebView2Data-{profile}");
            }

            string suffix = profile is not null ? $"41E2C7F3-{profile}" : "41E2C7F3";

            if (!noSingleInstance)
            {
                ActivateEventName = $@"Local\Symcon-Dashboard-for-Windows-Activate-{suffix}";
                using var mutex = new Mutex(true, $@"Local\Symcon-Dashboard-for-Windows-{suffix}", out bool createdNew);
                if (!createdNew)
                {
                    try
                    {
                        AllowSetForegroundWindow(0xFFFFFFFF); // ASFW_ANY
                        using var evt = EventWaitHandle.OpenExisting(ActivateEventName);
                        evt.Set();
                    }
                    catch { }
                    return;
                }
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            else
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
        }
    }
}