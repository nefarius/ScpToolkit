using System;
using System.Windows;
using System.Windows.Controls;
using ScpControl;
using ScpControl.Profiler;
using ScpControl.ScpCore;

namespace ScpProfiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ScpProxy _proxy = new ScpProxy();
        private DsPadId _currentPad;
        private DualShockProfile _currentProfile = new DualShockProfile();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _proxy.NativeFeedReceived += ProxyOnNativeFeedReceived;
            _proxy.Start();
        }

        private void ProxyOnNativeFeedReceived(object sender, ScpHidReport report)
        {
            if(report.PadId != _currentPad) return;


        }

        private void CurrentPad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentPad = (DsPadId)((ComboBox)sender).SelectedItem;

            var t = CurrentDualShockProfile;
        }
    }
}
