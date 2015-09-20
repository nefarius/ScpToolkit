using System;
using System.Windows;
using ScpControl.Utilities;
using ScpDriverInstaller.Utilities;

namespace ScpDriverInstaller
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileDownloader _dldr;

        public MainWindow()
        {
            InitializeComponent();

            _dldr = new FileDownloader("http://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe");

            _dldr.ProgressChanged += (sender, args) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    TestProgressBar.Value = args.CurrentProgressPercentage;
                }));
            };
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //await _dldr.DownloadAsync(@"D:\Temp\lulz.exe");

            await RedistPackageInstaller.DownloadAndInstallDirectXRedist();
        }
    }
}
