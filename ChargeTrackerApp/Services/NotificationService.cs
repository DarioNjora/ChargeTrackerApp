using System;
using System.Windows.Forms;

namespace ChargeTrackerApp.Services
{
    public class NotificationService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        /// <summary>Singolo clic sull'icona (tasto sinistro): apre il widget.</summary>
        public event EventHandler? TrayIconClicked;

        /// <summary>Doppio clic sull'icona: apre la finestra principale.</summary>
        public event EventHandler? TrayIconDoubleClicked;

        public event EventHandler? ExitRequested;
        public event EventHandler? ShowRequested;
        public event EventHandler? WidgetRequested;

        public NotificationService()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = false,
                Text = "Charge Tracker"
            };

            var menu = new ContextMenuStrip();
            var showItem = menu.Items.Add("Apri ChargeTracker");
            showItem.Click += (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty);
            var widgetItem = menu.Items.Add("Apri widget");
            widgetItem.Click += (s, e) => WidgetRequested?.Invoke(this, EventArgs.Empty);
            menu.Items.Add(new ToolStripSeparator());
            var exitItem = menu.Items.Add("Esci");
            exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            _notifyIcon.ContextMenuStrip = menu;

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    TrayIconClicked?.Invoke(this, EventArgs.Empty);
            };
            _notifyIcon.DoubleClick += (s, e) => TrayIconDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        public void ShowTray() => _notifyIcon.Visible = true;

        public void HideTray() => _notifyIcon.Visible = false;

        public void ShowBalloon(string title, string message, int timeoutMs = 6000)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(timeoutMs);
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
