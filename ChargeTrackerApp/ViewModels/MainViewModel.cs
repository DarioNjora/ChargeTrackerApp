using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Input;
using System.Windows.Threading;
using ChargeTrackerApp.Helpers;
using ChargeTrackerApp.Models;
using ChargeTrackerApp.Services;
using ChargeTrackerApp.Views;

namespace ChargeTrackerApp.ViewModels
{
    public enum AppPage { Dashboard, Devices, Calendar, Statistiche, Impostazioni }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly NotificationService _notificationService;
        private DispatcherTimer? _checkTimer;

        public ObservableCollection<DeviceViewModel> Devices { get; } = new();
        public ObservableCollection<DeviceViewModel> FilteredDevices { get; } = new();
        public ObservableCollection<DeviceViewModel> UrgentDevices { get; } = new();

        public AppSettings Settings { get; private set; }

        private AppPage _currentPage = AppPage.Dashboard;
        public AppPage CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(IsDashboard));
                OnPropertyChanged(nameof(IsDevices));
                OnPropertyChanged(nameof(IsCalendar));
                OnPropertyChanged(nameof(IsStatistiche));
                OnPropertyChanged(nameof(IsImpostazioni));
            }
        }

        public bool IsDashboard => CurrentPage == AppPage.Dashboard;
        public bool IsDevices => CurrentPage == AppPage.Devices;
        public bool IsCalendar => CurrentPage == AppPage.Calendar;
        public bool IsStatistiche => CurrentPage == AppPage.Statistiche;
        public bool IsImpostazioni => CurrentPage == AppPage.Impostazioni;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); RefreshCollections(); }
        }

        public const string AllLocationsLabel = "Tutte le posizioni";

        public ObservableCollection<string> Locations { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();

        private string _locationFilter = AllLocationsLabel;
        public string LocationFilter
        {
            get => _locationFilter;
            set { _locationFilter = value; OnPropertyChanged(nameof(LocationFilter)); RefreshCollections(); }
        }

        public int TotalDevices => Devices.Count;
        public int OverdueCount => Devices.Count(d => d.Status == ChargeStatus.Scaduto);
        public int DueSoonCount => Devices.Count(d => d.Status == ChargeStatus.InScadenza);
        public int OkCount => Devices.Count(d => d.Status == ChargeStatus.Carico);
        public int NeverCount => Devices.Count(d => d.Status == ChargeStatus.MaiCaricato);

        public int TotalChargeEvents => Devices.Sum(d => d.TotalCharges);
        public int ChargeEventsThisMonth => Devices.Sum(d =>
            d.Model.ChargeHistory.Count(dt => dt.Month == DateTime.Today.Month && dt.Year == DateTime.Today.Year));
        public string MostChargedDeviceName =>
            Devices.OrderByDescending(d => d.TotalCharges).FirstOrDefault(d => d.TotalCharges > 0)?.Name ?? "—";

        public double AverageBatteryHealth
        {
            get
            {
                var withCharges = Devices.Where(d => d.TotalCharges > 0).ToList();
                return withCharges.Count == 0 ? 100 : withCharges.Average(d => d.EstimatedBatteryHealthPercent);
            }
        }

        public double TotalAnnualEnergyCost => Devices.Sum(d => d.EstimatedAnnualCost(Settings.CostPerKwh));

        public List<DeviceViewModel> WarrantyExpiringSoon => Devices.Where(d =>
            d.IsUnderWarranty && d.WarrantyEndDate.HasValue &&
            (d.WarrantyEndDate.Value.Date - DateTime.Today).TotalDays <= 60).ToList();

        public ICommand NavigateCommand { get; }
        public ICommand AddDeviceCommand { get; }
        public ICommand EditDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand MarkChargedCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand OpenWidgetCommand { get; }

        public event EventHandler? OpenWidgetRequested;
        public event EventHandler? ThemeChangeApplied;

        /// <summary>
        /// Notificato ogni volta che i dati dei dispositivi cambiano (aggiunta, modifica,
        /// eliminazione, segnato come caricato). Usato ad esempio dal calendario per
        /// aggiornarsi subito senza dover cambiare mese.
        /// </summary>
        public event EventHandler? DataChanged;

        public MainViewModel(DataService dataService, NotificationService notificationService)
        {
            _dataService = dataService;
            _notificationService = notificationService;
            Settings = _dataService.GetSettings();

            NavigateCommand = new RelayCommand(p =>
            {
                if (p is string s && Enum.TryParse<AppPage>(s, out var page))
                    CurrentPage = page;
            });

            AddDeviceCommand = new RelayCommand(_ => OpenDeviceEditor(null));
            EditDeviceCommand = new RelayCommand(p => { if (p is DeviceViewModel dvm) OpenDeviceEditor(dvm); });
            DeleteDeviceCommand = new RelayCommand(p => { if (p is DeviceViewModel dvm) DeleteDevice(dvm); });
            MarkChargedCommand = new RelayCommand(p => { if (p is DeviceViewModel dvm) MarkCharged(dvm); });
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            OpenWidgetCommand = new RelayCommand(_ => OpenWidgetRequested?.Invoke(this, EventArgs.Empty));

            LoadDevices();
            StartReminderTimer();
        }

        public void LoadDevices()
        {
            Devices.Clear();
            foreach (var d in _dataService.GetAllDevices().Where(d => !d.IsArchived))
                Devices.Add(new DeviceViewModel(d));
            RebuildLocations();
            RebuildCategories();
            RefreshCollections();
        }

        private void RebuildCategories()
        {
            var defaults = CategoryDefaults.Suggested.Where(c => !Settings.HiddenDefaultCategories.Contains(c));
            var used = Devices.Select(d => d.Category).Where(c => !string.IsNullOrWhiteSpace(c));
            var all = defaults
                .Concat(Settings.CustomCategories)
                .Concat(used)
                .Distinct()
                .OrderBy(c => c == CategoryDefaults.Default ? 1 : 0)
                .ThenBy(c => c);

            Categories.Clear();
            foreach (var c in all)
                Categories.Add(c);
        }

        /// <summary>
        /// Aggiunge (o ripristina, se era stata nascosta) una categoria personalizzata,
        /// anche senza doverla assegnare subito a un dispositivo.
        /// </summary>
        public void AddCategory(string name)
        {
            name = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name)) return;

            Settings.HiddenDefaultCategories.Remove(name);
            if (!CategoryDefaults.Suggested.Contains(name) && !Settings.CustomCategories.Contains(name))
                Settings.CustomCategories.Add(name);

            _dataService.SaveSettings(Settings);
            RebuildCategories();
        }

        /// <summary>
        /// Elimina una categoria: i dispositivi che la usano vengono riassegnati ad "Altro".
        /// La categoria "Altro" stessa non può essere eliminata (è il valore di riserva).
        /// </summary>
        public void DeleteCategory(string name)
        {
            if (name == CategoryDefaults.Default) return;

            foreach (var d in Devices.Where(d => d.Category == name))
            {
                d.Category = CategoryDefaults.Default;
                _dataService.UpdateDevice(d.Model);
            }

            Settings.CustomCategories.Remove(name);
            if (CategoryDefaults.Suggested.Contains(name) && !Settings.HiddenDefaultCategories.Contains(name))
                Settings.HiddenDefaultCategories.Add(name);

            _dataService.SaveSettings(Settings);
            RebuildCategories();
            RefreshCollections();
        }

        private void RebuildLocations()
        {
            Locations.Clear();
            Locations.Add(AllLocationsLabel);
            foreach (var loc in Devices
                         .Select(d => d.Location)
                         .Where(l => !string.IsNullOrWhiteSpace(l))
                         .Distinct()
                         .OrderBy(l => l))
                Locations.Add(loc);

            if (string.IsNullOrEmpty(_locationFilter))
                _locationFilter = AllLocationsLabel;
        }

        public void ReloadSettings()
        {
            Settings = _dataService.GetSettings();
            OnPropertyChanged(nameof(Settings));
        }

        public void RefreshCollections()
        {
            FilteredDevices.Clear();
            var query = Devices.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(d => d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(LocationFilter) && LocationFilter != AllLocationsLabel)
                query = query.Where(d => d.Location == LocationFilter);

            foreach (var d in query.OrderBy(StatusSortOrder).ThenBy(d => d.DaysRemaining ?? int.MinValue))
                FilteredDevices.Add(d);

            UrgentDevices.Clear();
            foreach (var d in Devices
                         .Where(d => d.Status == ChargeStatus.Scaduto || d.Status == ChargeStatus.InScadenza)
                         .OrderBy(StatusSortOrder))
                UrgentDevices.Add(d);

            OnPropertyChanged(nameof(TotalDevices));
            OnPropertyChanged(nameof(OverdueCount));
            OnPropertyChanged(nameof(DueSoonCount));
            OnPropertyChanged(nameof(OkCount));
            OnPropertyChanged(nameof(NeverCount));
            OnPropertyChanged(nameof(TotalChargeEvents));
            OnPropertyChanged(nameof(ChargeEventsThisMonth));
            OnPropertyChanged(nameof(MostChargedDeviceName));
            OnPropertyChanged(nameof(AverageBatteryHealth));
            OnPropertyChanged(nameof(TotalAnnualEnergyCost));
            OnPropertyChanged(nameof(WarrantyExpiringSoon));

            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int StatusSortOrder(DeviceViewModel d) => d.Status switch
        {
            ChargeStatus.Scaduto => 0,
            ChargeStatus.InScadenza => 1,
            ChargeStatus.MaiCaricato => 2,
            ChargeStatus.Carico => 3,
            _ => 4
        };

        private void OpenDeviceEditor(DeviceViewModel? existing)
        {
            var device = existing?.Model ?? new Device { ChargeIntervalDays = Settings.DefaultIntervalDays };
            var window = new AddEditDeviceWindow(device, Categories) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                if (existing == null)
                {
                    _dataService.AddDevice(device);
                    Devices.Add(new DeviceViewModel(device));
                }
                else
                {
                    _dataService.UpdateDevice(device);
                    existing.RefreshStatus();
                }
                RebuildLocations();
                RebuildCategories();
                RefreshCollections();
            }
        }

        private void DeleteDevice(DeviceViewModel dvm)
        {
            var result = MessageBox.Show($"Eliminare '{dvm.Name}'?", "Conferma eliminazione",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _dataService.DeleteDevice(dvm.Id);
                Devices.Remove(dvm);
                RefreshCollections();
            }
        }

        /// <summary>
        /// Rimuove una posizione da tutti i dispositivi che la usano (li imposta come "senza posizione"),
        /// senza toccare nessun'altra proprietà del dispositivo.
        /// </summary>
        public void DeleteLocation(string location)
        {
            foreach (var d in Devices.Where(d => d.Location == location))
            {
                d.Location = string.Empty;
                _dataService.UpdateDevice(d.Model);
            }

            if (LocationFilter == location)
                _locationFilter = AllLocationsLabel;

            RebuildLocations();
            RefreshCollections();
        }

        private void MarkCharged(DeviceViewModel dvm)
        {
            dvm.MarkAsCharged();
            _dataService.UpdateDevice(dvm.Model);
            RefreshCollections();
        }

        private void SaveSettings()
        {
            _dataService.SaveSettings(Settings);
            StartupService.SetStartup(Settings.StartWithWindows);
            RestartReminderTimer();
            ThemeService.Apply(Settings.ThemeMode);
            ThemeChangeApplied?.Invoke(this, EventArgs.Empty);
            MessageBox.Show("Impostazioni salvate.", "Impostazioni", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Salva le impostazioni correnti senza mostrare alcun messaggio di conferma.
        /// Usato ad esempio quando l'utente sceglie "ricorda la mia scelta" alla chiusura.
        /// </summary>
        public void PersistSettingsSilently() => _dataService.SaveSettings(Settings);

        private void StartReminderTimer()
        {
            _checkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(Math.Max(5, Settings.NotifyCheckIntervalMinutes))
            };
            _checkTimer.Tick += (s, e) => CheckReminders();
            _checkTimer.Start();
            CheckReminders();
        }

        private void RestartReminderTimer()
        {
            _checkTimer?.Stop();
            StartReminderTimer();
        }

        public void CheckReminders()
        {
            if (!Settings.NotificationsEnabled) return;

            var due = Devices.Where(d => d.Status == ChargeStatus.Scaduto || d.Status == ChargeStatus.InScadenza).ToList();
            if (due.Count == 0) return;

            var names = string.Join(", ", due.Take(5).Select(d => d.Name));
            var extra = due.Count > 5 ? $" e altri {due.Count - 5}" : "";
            _notificationService.ShowBalloon("Promemoria ricarica", $"Dispositivi da caricare: {names}{extra}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
