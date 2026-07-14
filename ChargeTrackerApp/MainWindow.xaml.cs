using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.Models;
using ChargeTrackerApp.Services;
using ChargeTrackerApp.ViewModels;
using ChargeTrackerApp.Views;
using Microsoft.Win32;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ChargeTrackerApp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        // true solo quando questa istanza di finestra viene sostituita da una nuova
        // (es. cambio tema): in quel caso NON va spento tutto il programma.
        private bool _isRestartingForTheme = false;

        // true quando l'utente ha scelto esplicitamente "Esci" dal menu del tray.
        private bool _isExplicitExit = false;

        private WidgetWindow? _widgetWindow;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(App.DataService, App.NotificationService);
            DataContext = _viewModel;

            ThemeModeCombo.ItemsSource = Enum.GetValues(typeof(ThemeMode));

            App.NotificationService.ShowRequested += NotificationService_ShowRequested;
            App.NotificationService.TrayIconDoubleClicked += NotificationService_ShowRequested;
            App.NotificationService.TrayIconClicked += NotificationService_TrayIconClicked;
            App.NotificationService.WidgetRequested += NotificationService_TrayIconClicked;
            App.NotificationService.ExitRequested += NotificationService_ExitRequested;

            _viewModel.OpenWidgetRequested += ViewModel_OpenWidgetRequested;
            _viewModel.ThemeChangeApplied += ViewModel_ThemeChangeApplied;

            App.NotificationService.ShowTray();

            var args = Environment.GetCommandLineArgs();
            bool startMinimized = Array.Exists(args, a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));

            if (startMinimized && _viewModel.Settings.MinimizeToTray)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }

            if (_viewModel.Settings.ShowWidgetOnStartup)
                OpenWidget();
        }

        private void NotificationService_ShowRequested(object? sender, EventArgs e) => RestoreWindow();

        private void NotificationService_TrayIconClicked(object? sender, EventArgs e) => OpenWidget();

        private void NotificationService_ExitRequested(object? sender, EventArgs e)
        {
            _isExplicitExit = true;
            _widgetWindow?.Close();
            Close();
        }

        private void ViewModel_OpenWidgetRequested(object? sender, EventArgs e) => OpenWidget();

        private void ViewModel_ThemeChangeApplied(object? sender, EventArgs e) => RestartWindowForTheme();

        private void OpenWidget()
        {
            if (_widgetWindow == null || !_widgetWindow.IsVisible)
                _widgetWindow = new WidgetWindow(_viewModel);

            _widgetWindow.Show();
            _widgetWindow.Activate();
        }

        private void RestoreWindow()
        {
            // Questa istanza potrebbe essere rimasta agganciata alla tray icon anche
            // dopo essere stata chiusa per davvero (es. cambio tema, o chiusura reale
            // a MinimizeToTray disattivato): in tal caso non proviamo a richiamare
            // Show/Activate su una finestra ormai chiusa.
            if (!IsLoaded)
                return;

            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void RestartWindowForTheme()
        {
            _isRestartingForTheme = true;

            var newWindow = new MainWindow();
            Application.Current.MainWindow = newWindow;
            newWindow.Show();

            _widgetWindow?.Close();
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _viewModel.Settings.MinimizeToTray)
                Hide();
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Chiusure "tecniche" (cambio tema, uscita già confermata dal menu del tray)
            // non devono mostrare alcun dialogo.
            if (_isRestartingForTheme || _isExplicitExit)
            {
                base.OnClosing(e);
                return;
            }

            if (!_viewModel.Settings.AskOnClose)
            {
                if (_viewModel.Settings.MinimizeToTray)
                {
                    e.Cancel = true;
                    Hide();
                    WindowState = WindowState.Minimized;
                }
                else
                {
                    base.OnClosing(e);
                }
                return;
            }

            var dlg = new ExitConfirmationWindow { Owner = this };
            var confirmed = dlg.ShowDialog();

            if (confirmed != true || dlg.Choice == ExitChoice.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (dlg.RememberChoice)
            {
                _viewModel.Settings.MinimizeToTray = dlg.Choice == ExitChoice.Background;
                _viewModel.Settings.AskOnClose = false;
                _viewModel.PersistSettingsSilently();
            }

            if (dlg.Choice == ExitChoice.Background)
            {
                e.Cancel = true;
                Hide();
                WindowState = WindowState.Minimized;
                return;
            }

            _isExplicitExit = true;
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Slega sempre questa istanza dagli eventi condivisi (servizio di notifica
            // e ViewModel), altrimenti una finestra ormai chiusa continuerebbe a
            // "rispondere" ai clic sull'icona del tray, causando un crash.
            App.NotificationService.ShowRequested -= NotificationService_ShowRequested;
            App.NotificationService.TrayIconDoubleClicked -= NotificationService_ShowRequested;
            App.NotificationService.TrayIconClicked -= NotificationService_TrayIconClicked;
            App.NotificationService.WidgetRequested -= NotificationService_TrayIconClicked;
            App.NotificationService.ExitRequested -= NotificationService_ExitRequested;
            _viewModel.OpenWidgetRequested -= ViewModel_OpenWidgetRequested;
            _viewModel.ThemeChangeApplied -= ViewModel_ThemeChangeApplied;

            base.OnClosed(e);

            // Se questa non era una semplice sostituzione per il cambio tema, la
            // chiusura è definitiva: bisogna spegnere del tutto il programma,
            // nascondere l'icona del tray e non lasciare nulla "in ascolto".
            if (!_isRestartingForTheme)
            {
                App.NotificationService.HideTray();
                Application.Current.Shutdown();
            }
        }

        private void ExportBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "File JSON (*.json)|*.json",
                FileName = $"ChargeTracker_Backup_{DateTime.Now:yyyyMMdd}.json"
            };
            if (dlg.ShowDialog() == true)
            {
                var json = App.DataService.ExportData();
                File.WriteAllText(dlg.FileName, json);
                MessageBox.Show("Backup esportato con successo.", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportBackup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "File JSON (*.json)|*.json"
            };
            if (dlg.ShowDialog() == true)
            {
                var result = MessageBox.Show(
                    "Importando il backup, tutti i dispositivi attuali verranno sostituiti. Continuare?",
                    "Conferma importazione", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                var json = File.ReadAllText(dlg.FileName);
                App.DataService.ImportData(json);
                _viewModel.LoadDevices();
                _viewModel.ReloadSettings();
                MessageBox.Show("Backup importato con successo.", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DetectBluetooth_Click(object sender, RoutedEventArgs e)
        {
            var window = new DetectedDevicesWindow { Owner = this };
            if (window.ShowDialog() == true)
            {
                _viewModel.LoadDevices();
                MessageBox.Show("Dispositivi importati. Ricordati di modificarli per impostare categoria e intervallo di ricarica corretti.",
                    "Importazione completata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ManageLocations_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageLocationsWindow(_viewModel) { Owner = this };
            window.ShowDialog();
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            var window = new CategoryManagerWindow(_viewModel) { Owner = this };
            window.ShowDialog();
        }

        private void ChangeDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog
            {
                Title = "Scegli una cartella per salvare i dati (es. una cartella OneDrive o Dropbox)"
            };
            if (dlg.ShowDialog() != true) return;

            var result = MessageBox.Show(
                "L'app verrà riavviata per usare la nuova cartella dati. I dati attuali verranno copiati nella nuova posizione. Continuare?",
                "Cambia cartella dati", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var oldDbPath = Path.Combine(DataLocationService.GetDataFolder(), "data.db");
                var newFolder = dlg.FolderName;
                Directory.CreateDirectory(newFolder);
                var newDbPath = Path.Combine(newFolder, "data.db");

                App.DataService.Dispose();

                if (File.Exists(oldDbPath) && !File.Exists(newDbPath))
                    File.Copy(oldDbPath, newDbPath);

                DataLocationService.SetDataFolder(newFolder);

                MessageBox.Show("Cartella dati aggiornata. L'app si riavvierà ora.", "Riavvio",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                    Process.Start(exePath);

                _isExplicitExit = true;
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il cambio cartella: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
