using System.Windows;
using Application = System.Windows.Application;
using ChargeTrackerApp.ViewModels;

namespace ChargeTrackerApp.Views
{
    public partial class WidgetWindow : Window
    {
        public WidgetWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 16;
            Top = workArea.Bottom - Height - 16;
        }

        private void OpenApp_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }
        }

        private void CloseWidget_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
