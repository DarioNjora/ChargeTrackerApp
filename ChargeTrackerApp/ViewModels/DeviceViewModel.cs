using System;
using System.ComponentModel;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ChargeTrackerApp.Models;

namespace ChargeTrackerApp.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        public Device Model { get; }

        public DeviceViewModel(Device model)
        {
            Model = model;
        }

        public int Id => Model.Id;

        public string Name
        {
            get => Model.Name;
            set { Model.Name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Category
        {
            get => Model.Category;
            set { Model.Category = value; OnPropertyChanged(nameof(Category)); OnPropertyChanged(nameof(Icon)); }
        }

        public int ChargeIntervalDays
        {
            get => Model.ChargeIntervalDays;
            set { Model.ChargeIntervalDays = value; OnPropertyChanged(nameof(ChargeIntervalDays)); RefreshStatus(); }
        }

        public string Notes
        {
            get => Model.Notes;
            set { Model.Notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public string ChargerType
        {
            get => Model.ChargerType;
            set { Model.ChargerType = value; OnPropertyChanged(nameof(ChargerType)); }
        }

        public DateTime? LastCharged
        {
            get => Model.LastCharged;
            set { Model.LastCharged = value; OnPropertyChanged(nameof(LastCharged)); RefreshStatus(); }
        }

        public int TotalCharges => Model.ChargeHistory.Count;

        public string Location
        {
            get => Model.Location;
            set { Model.Location = value; OnPropertyChanged(nameof(Location)); }
        }

        public bool IsPortable
        {
            get => Model.IsPortable;
            set { Model.IsPortable = value; OnPropertyChanged(nameof(IsPortable)); }
        }

        public double CapacityMah
        {
            get => Model.CapacityMah;
            set { Model.CapacityMah = value; OnPropertyChanged(nameof(CapacityMah)); }
        }

        public DateTime? PurchaseDate
        {
            get => Model.PurchaseDate;
            set
            {
                Model.PurchaseDate = value;
                OnPropertyChanged(nameof(PurchaseDate));
                OnPropertyChanged(nameof(WarrantyEndDate));
                OnPropertyChanged(nameof(IsUnderWarranty));
            }
        }

        public int? WarrantyMonths
        {
            get => Model.WarrantyMonths;
            set
            {
                Model.WarrantyMonths = value;
                OnPropertyChanged(nameof(WarrantyMonths));
                OnPropertyChanged(nameof(WarrantyEndDate));
                OnPropertyChanged(nameof(IsUnderWarranty));
            }
        }

        public DateTime? WarrantyEndDate => (Model.PurchaseDate.HasValue && Model.WarrantyMonths.HasValue)
            ? Model.PurchaseDate.Value.AddMonths(Model.WarrantyMonths.Value)
            : (DateTime?)null;

        public bool IsUnderWarranty => WarrantyEndDate.HasValue && WarrantyEndDate.Value.Date >= DateTime.Today;

        /// <summary>
        /// Stima (indicativa) della carica residua in percentuale, basata sul tempo
        /// trascorso dall'ultima ricarica rispetto all'intervallo tipico del dispositivo.
        /// </summary>
        public int BatteryLevelPercent
        {
            get
            {
                if (Model.LastCharged == null) return 0;
                if (Model.ChargeIntervalDays <= 0) return 100;
                var daysSince = (DateTime.Today - Model.LastCharged.Value.Date).TotalDays;
                var percent = 100.0 - (daysSince / Model.ChargeIntervalDays * 100.0);
                return (int)Math.Max(0, Math.Min(100, Math.Round(percent)));
            }
        }

        /// <summary>
        /// Stima (indicativa) della salute della batteria, basata sul numero di cicli
        /// di ricarica registrati rispetto a una durata tipica di 500 cicli.
        /// </summary>
        public int EstimatedBatteryHealthPercent
        {
            get
            {
                const int estimatedLifespanCycles = 500;
                var percent = 100.0 - (TotalCharges / (double)estimatedLifespanCycles * 100.0);
                return (int)Math.Max(0, Math.Min(100, Math.Round(percent)));
            }
        }

        /// <summary>
        /// Stima (indicativa) del costo energetico annuo. La capacità inserita è in mAh
        /// (come sull'etichetta del dispositivo); si assume una ricarica a 5V, la tensione
        /// standard USB, per convertirla in Wh: Wh = mAh × 5V / 1000.
        /// Nota: dispositivi con ricarica rapida a tensioni più alte avranno un consumo
        /// reale leggermente diverso, ma la stima resta un'indicazione di massima utile.
        /// </summary>
        public double EstimatedAnnualCost(double costPerKwh)
        {
            if (Model.CapacityMah <= 0 || Model.ChargeIntervalDays <= 0) return 0;
            const double chargingVoltage = 5.0;
            var wh = Model.CapacityMah * chargingVoltage / 1000.0;
            var chargesPerYear = 365.0 / Model.ChargeIntervalDays;
            return (wh / 1000.0) * chargesPerYear * costPerKwh;
        }

        public string Icon => CategoryDefaults.GetIcon(Model.Category);

        public DateTime? NextDueDate => Model.LastCharged?.AddDays(Model.ChargeIntervalDays);

        public int? DaysRemaining
        {
            get
            {
                if (NextDueDate == null) return null;
                return (int)Math.Ceiling((NextDueDate.Value.Date - DateTime.Today).TotalDays);
            }
        }

        public ChargeStatus Status
        {
            get
            {
                if (Model.LastCharged == null) return ChargeStatus.MaiCaricato;
                var days = DaysRemaining ?? 0;
                if (days < 0) return ChargeStatus.Scaduto;
                if (days <= 1) return ChargeStatus.InScadenza;
                return ChargeStatus.Carico;
            }
        }

        public string StatusText => Status switch
        {
            ChargeStatus.MaiCaricato => "Mai caricato",
            ChargeStatus.Scaduto => $"Scaduto da {Math.Abs(DaysRemaining ?? 0)} giorni",
            ChargeStatus.InScadenza => (DaysRemaining == 0 ? "Da caricare oggi" : "Da caricare domani"),
            ChargeStatus.Carico => $"Carico ancora per {DaysRemaining} giorni",
            _ => ""
        };

        public Brush StatusColor => Status switch
        {
            ChargeStatus.MaiCaricato => new SolidColorBrush(Color.FromRgb(148, 148, 158)),
            ChargeStatus.Scaduto => new SolidColorBrush(Color.FromRgb(235, 87, 87)),
            ChargeStatus.InScadenza => new SolidColorBrush(Color.FromRgb(242, 153, 74)),
            ChargeStatus.Carico => new SolidColorBrush(Color.FromRgb(111, 207, 151)),
            _ => new SolidColorBrush(Colors.Gray)
        };

        public void RefreshStatus()
        {
            OnPropertyChanged(nameof(NextDueDate));
            OnPropertyChanged(nameof(DaysRemaining));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }

        public void MarkAsCharged()
        {
            Model.LastCharged = DateTime.Now;
            Model.ChargeHistory.Add(DateTime.Now);
            OnPropertyChanged(nameof(LastCharged));
            OnPropertyChanged(nameof(TotalCharges));
            RefreshStatus();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
