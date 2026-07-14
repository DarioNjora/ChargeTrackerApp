using System;
using System.Collections.Generic;

namespace ChargeTrackerApp.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = CategoryDefaults.Default;
        public int ChargeIntervalDays { get; set; } = 3;
        public DateTime? LastCharged { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string ChargerType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<DateTime> ChargeHistory { get; set; } = new List<DateTime>();
        public bool IsArchived { get; set; } = false;

        // Posizione / stanza in cui si trova abitualmente il dispositivo
        public string Location { get; set; } = string.Empty;

        // Se true, il dispositivo esce spesso di casa (utile per il promemoria "porta il caricatore")
        public bool IsPortable { get; set; } = false;

        // Capacità batteria in mAh (come indicato di solito sull'etichetta del dispositivo),
        // usata per la stima del costo energetico
        public double CapacityMah { get; set; } = 0;

        // Data di acquisto e durata della garanzia in mesi
        public DateTime? PurchaseDate { get; set; }
        public int? WarrantyMonths { get; set; }
    }
}
