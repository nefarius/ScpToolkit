using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScpLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DriverInstallerButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(WorkingDirectory, "ScpDriver.exe"));
        }

        private void ServerMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(WorkingDirectory, "ScpMonitor.exe"));
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(WorkingDirectory, "ScpServer.exe"));
        }
    }
}
