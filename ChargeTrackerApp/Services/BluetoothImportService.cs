using System.Collections.Generic;
using System.Management;

namespace ChargeTrackerApp.Services
{
    public static class BluetoothImportService
    {
        /// <summary>
        /// Restituisce i nomi dei dispositivi Bluetooth risultanti associati a Windows,
        /// interrogando la classe WMI Win32_PnPEntity. Se WMI non è disponibile
        /// (ambienti particolari, permessi limitati) restituisce una lista vuota.
        /// </summary>
        public static List<string> GetPairedBluetoothDeviceNames()
        {
            var names = new List<string>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, PNPClass FROM Win32_PnPEntity WHERE PNPClass = 'Bluetooth'");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) &&
                        !name.Contains("Bluetooth Enumerator") &&
                        !name.Contains("Bluetooth Device (") &&
                        !names.Contains(name))
                    {
                        names.Add(name);
                    }
                }
            }
            catch
            {
                // WMI non disponibile: si ignora silenziosamente, l'utente potrà
                // comunque aggiungere i dispositivi manualmente.
            }

            return names;
        }
    }
}
