using System.Collections.Generic;

namespace ChargeTrackerApp.Models
{
    public class AppSettings
    {
        public int Id { get; set; } = 1;
        public int DefaultIntervalDays { get; set; } = 3;
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool NotificationsEnabled { get; set; } = true;
        public int NotifyDaysBefore { get; set; } = 1;
        public int NotifyCheckIntervalMinutes { get; set; } = 30;
        public ThemeMode ThemeMode { get; set; } = ThemeMode.Scuro;
        public double CostPerKwh { get; set; } = 0.25;
        public bool ShowWidgetOnStartup { get; set; } = false;

        // Se true, alla chiusura viene sempre chiesto se uscire del tutto o rimanere in background.
        public bool AskOnClose { get; set; } = true;

        // Categorie aggiunte manualmente dall'utente (oltre a quelle suggerite)
        public List<string> CustomCategories { get; set; } = new();

        // Categorie suggerite che l'utente ha scelto di eliminare
        public List<string> HiddenDefaultCategories { get; set; } = new();
    }
}
