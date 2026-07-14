using System;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using ChargeTrackerApp.Services;

namespace ChargeTrackerApp
{
    public partial class App : Application
    {
        public static DataService DataService { get; private set; } = null!;
        public static NotificationService NotificationService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Intercetta qualsiasi errore imprevisto durante l'esecuzione e lo mostra
            // invece di far chiudere l'app senza spiegazioni.
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    $"Si è verificato un errore imprevisto:\n\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show(
                    $"Si è verificato un errore critico all'avvio:\n\n{ex?.Message}\n\n{ex?.StackTrace}",
                    "Errore critico", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            try
            {
                DataService = new DataService();
                NotificationService = new NotificationService();

                var settings = DataService.GetSettings();
                ThemeService.Apply(settings.ThemeMode);

                var window = new MainWindow();
                MainWindow = window;
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Impossibile avviare l'applicazione:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Errore di avvio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NotificationService?.Dispose();
            DataService?.Dispose();
            base.OnExit(e);
        }
    }
}
