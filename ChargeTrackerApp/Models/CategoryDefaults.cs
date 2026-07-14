using System.Collections.Generic;

namespace ChargeTrackerApp.Models
{
    /// <summary>
    /// Categorie suggerite di partenza e relative icone. L'utente può aggiungerne
    /// di personalizzate o eliminare quelle predefinite dalle Impostazioni.
    /// </summary>
    public static class CategoryDefaults
    {
        public const string Default = "Altro";

        public static readonly List<string> Suggested = new()
        {
            "Smartphone",
            "Tablet",
            "Laptop / PC",
            "Smartwatch",
            "Cuffie / Auricolari",
            "Speaker Bluetooth",
            "Fotocamera",
            "Drone",
            "Controller / Joypad",
            "Spazzolino elettrico",
            "Rasoio elettrico",
            "Power Bank",
            Default
        };

        private static readonly Dictionary<string, string> Icons = new()
        {
            { "Smartphone", "📱" },
            { "Tablet", "📟" },
            { "Laptop / PC", "💻" },
            { "Smartwatch", "⌚" },
            { "Cuffie / Auricolari", "🎧" },
            { "Speaker Bluetooth", "🔊" },
            { "Fotocamera", "📷" },
            { "Drone", "🚁" },
            { "Controller / Joypad", "🎮" },
            { "Spazzolino elettrico", "🪥" },
            { "Rasoio elettrico", "🪒" },
            { "Power Bank", "🔋" }
        };

        public static string GetIcon(string category)
            => !string.IsNullOrWhiteSpace(category) && Icons.TryGetValue(category, out var icon) ? icon : "🔌";
    }
}
