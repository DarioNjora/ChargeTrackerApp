using System;
using System.IO;

namespace ChargeTrackerApp.Services
{
    public static class DataLocationService
    {
        private static readonly string BaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChargeTrackerApp");

        private static readonly string ConfigFile = Path.Combine(BaseFolder, "location.cfg");

        public static string DefaultFolderPath => BaseFolder;

        public static string GetDataFolder()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var custom = File.ReadAllText(ConfigFile).Trim();
                    if (!string.IsNullOrWhiteSpace(custom) && Directory.Exists(custom))
                        return custom;
                }
            }
            catch
            {
                // In caso di problemi di lettura, si torna alla cartella predefinita.
            }

            Directory.CreateDirectory(BaseFolder);
            return BaseFolder;
        }

        public static void SetDataFolder(string folderPath)
        {
            Directory.CreateDirectory(BaseFolder);
            File.WriteAllText(ConfigFile, folderPath);
        }
    }
}
