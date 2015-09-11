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

            MainAccordion.DataContext = _config;
        }
    }
}