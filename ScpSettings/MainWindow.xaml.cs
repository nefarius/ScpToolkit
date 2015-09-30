using System;
using System.ComponentModel;
using System.Windows;
using ScpControl;
using ScpControl.ScpCore;

namespace ScpSettings
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalConfiguration _config;
        private readonly ScpProxy _proxy = new ScpProxy();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_proxy.IsActive)
                return;

            _config.IdleTimeout *= GlobalConfiguration.IdleTimeoutMultiplier;
            _config.Latency *= GlobalConfiguration.LatencyMultiplier;

            _proxy.WriteConfig(_config);
            _proxy.Stop();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (!_proxy.Start())
            {
                MessageBox.Show("Couldn't connect to server, please check if the service is running!",
                    "Fatal error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _config = _proxy.ReadConfig();

            _config.IdleTimeout /= GlobalConfiguration.IdleTimeoutMultiplier;
            _config.Latency /= GlobalConfiguration.LatencyMultiplier;

            MainAccordion.DataContext = _config;
        }

        private void IdleTimoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = e.NewValue;

            if (value == 0)
            {
                IdleTimoutGroupBox.Header = "Idle Timeout: Disabled";
            }
            else if (value == 1)
            {
                IdleTimoutGroupBox.Header = "Idle Timeout: 1 minute";
            }
            else
            {
                IdleTimoutGroupBox.Header = string.Format("Idle Timeout: {0} minutes", value);
            }
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = e.NewValue;

            BrightnessGroupBox.Header = value == 0
                ? string.Format("Light Bar Brightness: Disabled")
                : string.Format("Light Bar Brightness: {0}%", (int)((value * 100) / 255));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = ((int)e.NewValue) << 4;

            RumbleLatencyGroupBox.Header = string.Format("Rumble Latency: {0} ms", value);
        }

        private void Slider_LEDsPeriodChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = (int)e.NewValue;

            LEDsFlashingPeriodGroupBox.Header = string.Format("LEDs flashing period: {0} ms", value);
        }
    }
}
