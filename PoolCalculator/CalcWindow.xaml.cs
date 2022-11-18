using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using UniversalSerializerLib3;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace PoolCalculator {

    /// <summary>
    /// Interaction logic for CalcWindow.xaml
    /// </summary>
    public partial class CalcWindow : Window {
        private static double hourlyRateDefault = 3.00;
        private static double hourlyRate = hourlyRateDefault;
        private static double hourlyRatePrevious = hourlyRate;

        private ClockBase inClock => (ClockBase) FindName("In");
        private ClockBase outClock => (ClockBase) FindName("Out");
        private Label chargeLabel => (Label) FindName("Charge");
        private TextBox rateSettingBox => (TextBox) FindName("RateSetting");
        private Button rateSettingApply => (Button) FindName("RateApply");
        private Label timeLabel => (Label) FindName("TotalTimePlayed");
        
        public CalcWindow() {
            InitializeComponent();
            inClock.DisplayTimeChanged += UpdatePrice;
            outClock.DisplayTimeChanged += UpdatePrice;
            rateSettingApply.Click += UpdateRate;
            try {
                using (var s = new UniversalSerializer("settings.pool"))
                {
                    hourlyRate = s.Deserialize<double>();
                }
            } catch {}
            rateSettingBox.Text = hourlyRate.ToString("N2");
        }

        /// <summary>
        /// Update rental cost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateRate(object sender, RoutedEventArgs e) {
            double d;
            if (double.TryParse(rateSettingBox.Text, 
                    NumberStyles.Currency, CultureInfo.CurrentCulture, out d))
            {
                rateSettingBox.Text = d.ToString("N2");
                hourlyRate = d;
                try {
                    using (var s = new UniversalSerializer("settings.pool"))
                    {
                        s.Serialize(hourlyRate);
                    }
                } catch { }
                hourlyRatePrevious = hourlyRate;
                UpdatePrice();
            } else {
                rateSettingBox.Text = hourlyRateDefault.ToString("N2");
                hourlyRate = hourlyRatePrevious;
                UpdatePrice();
            }
        }

        /// <summary>
        /// Event method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="functionEventArgs"></param>
        private void UpdatePrice(object? sender, FunctionEventArgs<DateTime> functionEventArgs) {
            UpdatePrice();
        }
        
        /// <summary>
        /// Update the displayed price
        /// </summary>
        private void UpdatePrice() {
            outClock.DisplayTimeChanged -= UpdatePrice;
            TimeSpan inTime = inClock.DisplayTime.TimeOfDay;
            TimeSpan outTime = outClock.DisplayTime.TimeOfDay;
            TimeSpan result = outTime - inTime;
            if (result.TotalSeconds < 0) {
                outClock.DisplayTime = inClock.DisplayTime;
                timeLabel.Content = "0h 0m";
                chargeLabel.Content = "$0.00";
                return;
            }
            timeLabel.Content = result.Hours + "h " + result.Minutes + "m";
            chargeLabel.Content = (hourlyRate / 60.00 * result.TotalMinutes).ToString("C");
            outClock.DisplayTimeChanged += UpdatePrice;
        }
    }
}