using System.Runtime.InteropServices;

namespace SymconDashboard
{
    internal static class Program
    {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, @"Local\Symcon-Dashboard-for-Windows-41E2C7F3", out bool createdNew);
            if (!createdNew)
            {
                try
                {
                    AllowSetForegroundWindow(0xFFFFFFFF); // ASFW_ANY
                    using var evt = EventWaitHandle.OpenExisting(
                        @"Local\Symcon-Dashboard-for-Windows-Activate-41E2C7F3");
                    evt.Set();
                }
                catch { }
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}