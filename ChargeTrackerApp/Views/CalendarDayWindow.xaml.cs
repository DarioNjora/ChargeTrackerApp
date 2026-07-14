using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.ViewModels;

namespace ChargeTrackerApp.Views
{
    public partial class CalendarDayWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly DateTime _date;

        public CalendarDayWindow(MainViewModel viewModel, DateTime date)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _date = date.Date;

            DateTitle.Text = _date.ToString("dddd d MMMM yyyy", CultureInfo.GetCultureInfo("it-IT"));
            DateTitle.Text = char.ToUpper(DateTitle.Text[0]) + DateTitle.Text.Substring(1);

            RefreshList();
        }

        private void RefreshList()
        {
            var devices = _viewModel.Devices
                .Where(d => d.NextDueDate.HasValue && d.NextDueDate.Value.Date == _date)
                .OrderBy(d => d.Name)
                .ToList();

            DevicesList.ItemsSource = devices;
            EmptyText.Visibility = devices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MarkCharged_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not DeviceViewModel dvm) return;

            if (_viewModel.MarkChargedCommand.CanExecute(dvm))
                _viewModel.MarkChargedCommand.Execute(dvm);

            RefreshList();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
