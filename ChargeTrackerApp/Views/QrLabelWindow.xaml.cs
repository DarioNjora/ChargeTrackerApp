using System;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media.Imaging;
using ChargeTrackerApp.Models;
using Microsoft.Win32;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using QRCoder;

namespace ChargeTrackerApp.Views
{
    public partial class QrLabelWindow : Window
    {
        private readonly System.Drawing.Bitmap _bitmap;
        private readonly string _deviceName;

        public QrLabelWindow(Device device)
        {
            InitializeComponent();
            _deviceName = device.Name;
            DeviceNameText.Text = device.Name;

            var generator = new QRCodeGenerator();
            var payload = $"ChargeTracker|{device.Id}|{device.Name}";
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(data);
            _bitmap = qrCode.GetGraphic(10);

            QrImage.Source = BitmapToImageSource(_bitmap);
        }

        private static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Immagine PNG (*.png)|*.png",
                FileName = $"Etichetta_{_deviceName}.png"
            };
            if (dlg.ShowDialog() == true)
            {
                _bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Etichetta salvata. Ora puoi stamparla dal visualizzatore immagini di Windows.",
                    "Etichetta salvata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
