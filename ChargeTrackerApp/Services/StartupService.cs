using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace ChargeTrackerApp.Services
{
    public static class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ChargeTrackerApp";

        public static void SetStartup(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName
                    ?? Assembly.GetExecutingAssembly().Location;
                key.SetValue(AppName, $"\"{exePath}\" --minimized");
            }
            else
            {
                if (key.GetValue(AppName) != null)
                    key.DeleteValue(AppName, false);
            }
        }

        public static bool IsStartupEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            return key?.GetValue(AppName) != null;
        }
    }
}
