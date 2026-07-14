using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.Models;

namespace ChargeTrackerApp.Views
{
    public partial class AddEditDeviceWindow : Window
    {
        public Device Device { get; }

        public AddEditDeviceWindow(Device device, IEnumerable<string> availableCategories)
        {
            InitializeComponent();
            Device = device;

            var categories = availableCategories?.ToList() ?? new List<string>();
            if (!categories.Contains(Device.Category))
                categories.Add(Device.Category);
            CategoryCombo.ItemsSource = categories;
            CategoryCombo.Text = Device.Category;

            NameBox.Text = Device.Name;
            IntervalBox.Text = Device.ChargeIntervalDays.ToString();
            ChargerBox.Text = Device.ChargerType;
            NotesBox.Text = Device.Notes;
            LocationBox.Text = Device.Location;
            PortableCheck.IsChecked = Device.IsPortable;
            CapacityBox.Text = Device.CapacityWh > 0 ? Device.CapacityWh.ToString(CultureInfo.InvariantCulture) : string.Empty;
            WarrantyBox.Text = Device.WarrantyMonths?.ToString() ?? string.Empty;

            if (Device.LastCharged.HasValue)
                LastChargedPicker.SelectedDate = Device.LastCharged.Value.Date;

            if (Device.PurchaseDate.HasValue)
                PurchaseDatePicker.SelectedDate = Device.PurchaseDate.Value.Date;

            // L'etichetta QR richiede un Id valido: disponibile solo dopo il primo salvataggio.
            QrButton.IsEnabled = Device.Id != 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Inserisci un nome per il dispositivo.", "Campo obbligatorio",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(IntervalBox.Text, out var interval) || interval <= 0)
            {
                MessageBox.Show("Inserisci un intervallo di ricarica valido (in giorni, maggiore di zero).",
                    "Valore non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double capacity = 0;
            if (!string.IsNullOrWhiteSpace(CapacityBox.Text) &&
                !double.TryParse(CapacityBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out capacity))
            {
                MessageBox.Show("La capacità della batteria deve essere un numero (es. 15.5).",
                    "Valore non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? warrantyMonths = null;
            if (!string.IsNullOrWhiteSpace(WarrantyBox.Text))
            {
                if (!int.TryParse(WarrantyBox.Text, out var wm) || wm <= 0)
                {
                    MessageBox.Show("La durata della garanzia deve essere un numero di mesi valido.",
                        "Valore non valido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                warrantyMonths = wm;
            }

            var category = CategoryCombo.Text?.Trim();
            if (string.IsNullOrWhiteSpace(category))
                category = CategoryDefaults.Default;

            Device.Name = NameBox.Text.Trim();
            Device.Category = category;
            Device.ChargeIntervalDays = interval;
            Device.ChargerType = ChargerBox.Text.Trim();
            Device.Notes = NotesBox.Text.Trim();
            Device.LastCharged = LastChargedPicker.SelectedDate;
            Device.Location = LocationBox.Text.Trim();
            Device.IsPortable = PortableCheck.IsChecked == true;
            Device.CapacityWh = capacity;
            Device.PurchaseDate = PurchaseDatePicker.SelectedDate;
            Device.WarrantyMonths = warrantyMonths;

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ChargedNow_Click(object sender, RoutedEventArgs e)
        {
            LastChargedPicker.SelectedDate = DateTime.Today;
        }

        private void GenerateQr_Click(object sender, RoutedEventArgs e)
        {
            if (Device.Id == 0)
            {
                MessageBox.Show("Salva prima il dispositivo, poi potrai generare l'etichetta QR.",
                    "Salvataggio richiesto", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var qrWindow = new QrLabelWindow(Device) { Owner = this };
            qrWindow.ShowDialog();
        }
    }
}
