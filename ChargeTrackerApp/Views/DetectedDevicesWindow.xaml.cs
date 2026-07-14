using System.Collections.Generic;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Controls;
using CheckBox = System.Windows.Controls.CheckBox;
using ChargeTrackerApp.Models;
using ChargeTrackerApp.Services;

namespace ChargeTrackerApp.Views
{
    public partial class DetectedDevicesWindow : Window
    {
        private readonly List<CheckBox> _checkBoxes = new();

        public DetectedDevicesWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => ScanDevices();
        }

        private void ScanDevices()
        {
            DevicesPanel.Children.Clear();
            _checkBoxes.Clear();

            var names = BluetoothImportService.GetPairedBluetoothDeviceNames();

            if (names.Count == 0)
            {
                DevicesPanel.Children.Add(new TextBlock
                {
                    Text = "Nessun dispositivo Bluetooth associato trovato. Puoi comunque aggiungere i dispositivi manualmente.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
                    Margin = new Thickness(6)
                });
                return;
            }

            foreach (var name in names)
            {
                var cb = new CheckBox
                {
                    Content = name,
                    Margin = new Thickness(6),
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextPrimaryBrush"]
                };
                _checkBoxes.Add(cb);
                DevicesPanel.Children.Add(cb);
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            int imported = 0;
            foreach (var cb in _checkBoxes)
            {
                if (cb.IsChecked == true)
                {
                    var device = new Device
                    {
                        Name = cb.Content?.ToString() ?? "Nuovo dispositivo",
                        Category = CategoryDefaults.Default,
                        ChargeIntervalDays = 3
                    };
                    App.DataService.AddDevice(device);
                    imported++;
                }
            }

            if (imported == 0)
            {
                MessageBox.Show("Nessun dispositivo selezionato.", "Importazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
