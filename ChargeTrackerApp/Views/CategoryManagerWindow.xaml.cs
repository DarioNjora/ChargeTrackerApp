using System.Linq;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.Models;
using ChargeTrackerApp.ViewModels;

namespace ChargeTrackerApp.Views
{
    public class CategoryRow
    {
        public string Name { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
        public bool CanDelete { get; set; }
    }

    public partial class CategoryManagerWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public CategoryManagerWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            RefreshList();
        }

        private void RefreshList()
        {
            var rows = _viewModel.Categories
                .Select(name => new CategoryRow
                {
                    Name = name,
                    DeviceCount = _viewModel.Devices.Count(d => d.Category == name),
                    CanDelete = name != CategoryDefaults.Default
                })
                .ToList();

            CategoriesList.ItemsSource = rows;
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var name = NewCategoryBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Inserisci un nome per la nuova categoria.", "Campo obbligatorio",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _viewModel.AddCategory(name);
            NewCategoryBox.Text = string.Empty;
            RefreshList();
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not string category) return;

            var result = MessageBox.Show(
                $"Eliminare la categoria '{category}'? I dispositivi che la usano diventeranno 'Altro'.",
                "Conferma eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _viewModel.DeleteCategory(category);
            RefreshList();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
