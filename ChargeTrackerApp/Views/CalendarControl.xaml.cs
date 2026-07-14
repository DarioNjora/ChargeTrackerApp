using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Shapes;
using ChargeTrackerApp.ViewModels;

namespace ChargeTrackerApp.Views
{
    public partial class CalendarControl : UserControl
    {
        private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        public CalendarControl()
        {
            InitializeComponent();
            Loaded += (s, e) => BuildCalendar();
            Unloaded += (s, e) => Unsubscribe(DataContext as MainViewModel);
            DataContextChanged += (s, e) =>
            {
                Unsubscribe(e.OldValue as MainViewModel);
                Subscribe(e.NewValue as MainViewModel);
                BuildCalendar();
            };
        }

        private void Subscribe(MainViewModel? vm)
        {
            if (vm != null)
                vm.DataChanged += ViewModel_DataChanged;
        }

        private void Unsubscribe(MainViewModel? vm)
        {
            if (vm != null)
                vm.DataChanged -= ViewModel_DataChanged;
        }

        private void ViewModel_DataChanged(object? sender, EventArgs e) => BuildCalendar();

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            BuildCalendar();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            BuildCalendar();
        }

        public void Refresh() => BuildCalendar();

        private void BuildCalendar()
        {
            if (DataContext is not MainViewModel vm) return;

            MonthLabel.Text = _currentMonth.ToString("MMMM yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("it-IT"));

            DaysGrid.Children.Clear();
            DaysGrid.RowDefinitions.Clear();
            DaysGrid.ColumnDefinitions.Clear();

            for (int c = 0; c < 7; c++)
                DaysGrid.ColumnDefinitions.Add(new ColumnDefinition());

            var firstDay = _currentMonth;
            int firstDayOffset = ((int)firstDay.DayOfWeek + 6) % 7; // lunedi = 0
            int daysInMonth = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
            int totalCells = firstDayOffset + daysInMonth;
            int rows = (int)Math.Ceiling(totalCells / 7.0);

            for (int r = 0; r < rows; r++)
                DaysGrid.RowDefinitions.Add(new RowDefinition());

            var dueDates = vm.Devices
                .Where(d => d.NextDueDate.HasValue)
                .GroupBy(d => d.NextDueDate!.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < totalCells; i++)
            {
                if (i < firstDayOffset) continue;
                int dayNumber = i - firstDayOffset + 1;
                var date = new DateTime(firstDay.Year, firstDay.Month, dayNumber);

                var cell = BuildDayCell(date, dueDates.TryGetValue(date, out var list) ? list : null);
                Grid.SetRow(cell, i / 7);
                Grid.SetColumn(cell, i % 7);
                DaysGrid.Children.Add(cell);
            }
        }

        private Border BuildDayCell(DateTime date, List<DeviceViewModel>? devices)
        {
            bool isToday = date.Date == DateTime.Today;

            var border = new Border
            {
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(10),
                Background = (Brush)Application.Current.Resources["CardBackgroundBrush"],
                BorderBrush = isToday ? (Brush)Application.Current.Resources["AccentSolidBrush"] : Brushes.Transparent,
                BorderThickness = new Thickness(isToday ? 2 : 0),
                Padding = new Thickness(6),
                MinHeight = 78
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = date.Day.ToString(),
                FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal,
                Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize = 14
            });

            if (devices != null && devices.Count > 0)
            {
                var wrap = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
                foreach (var d in devices.Take(6))
                {
                    wrap.Children.Add(new Ellipse
                    {
                        Width = 9,
                        Height = 9,
                        Margin = new Thickness(1.5),
                        Fill = d.StatusColor,
                        ToolTip = d.Name
                    });
                }
                stack.Children.Add(wrap);

                stack.Children.Add(new TextBlock
                {
                    Text = devices.Count == 1 ? devices[0].Name : $"{devices.Count} dispositivi",
                    FontSize = 10,
                    Margin = new Thickness(0, 4, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"]
                });
            }

            border.Child = stack;
            return border;
        }
    }
}
