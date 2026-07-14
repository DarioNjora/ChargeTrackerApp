using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.ViewModels;

namespace ChargeTrackerApp.Views
{
    public class LocationRow
    {
        public string Name { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
    }

    public partial class ManageLocationsWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public ManageLocationsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            RefreshList();
        }

        private void RefreshList()
        {
            var rows = _viewModel.Devices
                .Where(d => !string.IsNullOrWhiteSpace(d.Location))
                .GroupBy(d => d.Location)
                .Select(g => new LocationRow { Name = g.Key, DeviceCount = g.Count() })
                .OrderBy(r => r.Name)
                .ToList();

            LocationsList.ItemsSource = rows;
            EmptyText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DeleteLocation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not string location) return;

            var result = MessageBox.Show(
                $"Eliminare la posizione '{location}' da tutti i dispositivi che la usano?",
                "Conferma eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _viewModel.DeleteLocation(location);
            RefreshList();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
