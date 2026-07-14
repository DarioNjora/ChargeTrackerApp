using System.Windows;

namespace ChargeTrackerApp.Views
{
    public enum ExitChoice
    {
        Cancel,
        CloseCompletely,
        Background
    }

    public partial class ExitConfirmationWindow : Window
    {
        public ExitChoice Choice { get; private set; } = ExitChoice.Cancel;
        public bool RememberChoice => RememberCheck.IsChecked == true;

        public ExitConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Background_Click(object sender, RoutedEventArgs e)
        {
            Choice = ExitChoice.Background;
            DialogResult = true;
        }

        private void CloseCompletely_Click(object sender, RoutedEventArgs e)
        {
            Choice = ExitChoice.CloseCompletely;
            DialogResult = true;
        }
    }
}
