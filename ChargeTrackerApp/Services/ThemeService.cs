using System;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using ChargeTrackerApp.Models;

namespace ChargeTrackerApp.Services
{
    public static class ThemeService
    {
        public static void Apply(ThemeMode mode)
        {
            bool useDark = mode switch
            {
                ThemeMode.Scuro => true,
                ThemeMode.Chiaro => false,
                ThemeMode.Automatico => IsNightTime(),
                _ => true
            };

            var uri = new Uri(
                useDark ? "/ChargeTrackerApp;component/Themes/DarkTheme.xaml" : "/ChargeTrackerApp;component/Themes/LightTheme.xaml",
                UriKind.Relative);
            var newDict = new ResourceDictionary { Source = uri };

            var app = Application.Current;
            var existing = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

            if (existing != null)
                app.Resources.MergedDictionaries.Remove(existing);

            app.Resources.MergedDictionaries.Insert(0, newDict);
        }

        private static bool IsNightTime()
        {
            var hour = DateTime.Now.Hour;
            return hour >= 20 || hour < 7;
        }
    }
}
