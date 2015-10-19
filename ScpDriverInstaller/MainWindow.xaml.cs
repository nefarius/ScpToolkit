using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using log4net;
using ScpControl.Driver;
using ScpControl.Utilities;
using ScpDriverInstaller.Properties;
using ScpDriverInstaller.Utilities;
using ScpDriverInstaller.View_Models;

namespace ScpDriverInstaller
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        #region Ctor

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => { Log.FatalFormat("An unhandled exception occured: {0}", args.ExceptionObject); };

            _viewModel.InstallButtonClicked += ViewModelOnInstallButtonClicked;
            _viewModel.UninstallButtonClicked += ViewModelOnUninstallButtonClicked;
            _viewModel.ExitButtonClicked += ViewModelOnExitButtonClicked;

            InstallGrid.DataContext = _viewModel;
        }

        #endregion

        #region Misc. Helpers

        private static void Logger(DifxLog Event, int error, string description)
        {
            switch (Event)
            {
                case DifxLog.DIFXAPI_ERROR:
                    Log.Error(description);
                    break;
                case DifxLog.DIFXAPI_INFO:
                case DifxLog.DIFXAPI_SUCCESS:
                    Log.Info(description);
                    break;
                case DifxLog.DIFXAPI_WARNING:
                    Log.Warn(description);
                    break;
            }
        }

        #endregion

        #region Private fields

        private bool _bthDriverConfigured;
        private bool _busDeviceConfigured;
        private bool _busDriverConfigured;
        private bool _ds3DriverConfigured;
        private bool _ds4DriverConfigured;
        private IntPtr _hWnd;
        private Difx _installer;
        private bool _reboot;
        private Cursor _saved;
        private bool _scpServiceConfigured;
        private OsType _valid = OsType.Invalid;

        #endregion

        #region View Model events

        private void ViewModelOnExitButtonClicked(object sender, EventArgs eventArgs)
        {
            Log.Info("Closing installer");

            Close();
        }

        private async void ViewModelOnUninstallButtonClicked(object sender, EventArgs eventArgs)
        {
            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.Wait;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;

            #endregion

            #region Uninstallation

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;

                try
                {
                    var rebootRequired = false;
                    _bthDriverConfigured = false;
                    _ds3DriverConfigured = false;
                    _ds4DriverConfigured = false;
                    _busDriverConfigured = false;
                    _busDeviceConfigured = false;

                    if (_viewModel.InstallWindowsService)
                    {
                        IDictionary state = new Hashtable();
                        var service =
                            new AssemblyInstaller(Directory.GetCurrentDirectory() + @"\ScpService.exe", null);

                        state.Clear();
                        service.UseNewContext = true;

                        if (Stop(Settings.Default.ScpServiceName))
                        {
                            Logger(DifxLog.DIFXAPI_INFO, 0, Settings.Default.ScpServiceName + " Stopped.");
                        }

                        service.Uninstall(state);
                        _scpServiceConfigured = true;
                    }

                    uint result = 0;

                    if (_viewModel.InstallBluetoothDriver)
                    {
                        result = DriverInstaller.UninstallBluetoothDongles(ref rebootRequired);

                        if (result > 0) _bthDriverConfigured = true;
                        _reboot |= rebootRequired;
                    }

                    if (_viewModel.InstallDualShock3Driver)
                    {
                        result = DriverInstaller.UninstallDualShock3Controllers(ref rebootRequired);

                        if (result > 0) _ds3DriverConfigured = true;
                        _reboot |= rebootRequired;
                    }

                    if (_viewModel.InstallDualShock4Driver)
                    {
                        result = DriverInstaller.UninstallDualShock4Controllers(ref rebootRequired);

                        if (result > 0) _ds4DriverConfigured = true;
                        _reboot |= rebootRequired;
                    }

                    if (Devcon.Find(Settings.Default.VirtualBusClassGuid, ref devPath, ref instanceId))
                    {
                        if (Devcon.Remove(Settings.Default.VirtualBusClassGuid, devPath, instanceId))
                        {
                            Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Removed");
                            _busDeviceConfigured = true;

                            _installer.Uninstall(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"),
                                DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                                out rebootRequired);
                            _reboot |= rebootRequired;

                            _busDriverConfigured = true;
                            _busDeviceConfigured = true;
                        }
                        else
                        {
                            Logger(DifxLog.DIFXAPI_ERROR, 0, "Virtual Bus Removal Failure");
                        }
                    }
                }
                catch (InstallException instex)
                {
                    if (!(instex.InnerException is Win32Exception))
                    {
                        Log.ErrorFormat("Error during uninstallation: {0}", instex);
                        return;
                    }

                    switch (((Win32Exception) instex.InnerException).NativeErrorCode)
                    {
                        case 1060: // ERROR_SERVICE_DOES_NOT_EXIST
                            Log.Warn("Service doesn't exist, maybe it was uninstalled before");
                            break;
                        default:
                            Log.ErrorFormat("Win32-Error during uninstallation: {0}",
                                (Win32Exception) instex.InnerException);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error during uninstallation: {0}", ex);
                }
            });

            #endregion

            #region Post-Uninstallation

            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
            Cursor = _saved;


            if (_reboot)
                Log.Info("[Reboot Required]");

            Log.Info("-- Uninstall Summary --");

            if (_scpServiceConfigured)
                Log.Info("SCP DSx Service uninstalled");

            if (_busDeviceConfigured)
                Log.Info("Bus Device uninstalled");

            if (_busDriverConfigured)
                Log.Info("Bus Driver uninstalled");

            if (_ds3DriverConfigured)
                Log.Info("DS3 USB Driver uninstalled");

            if (_bthDriverConfigured)
                Log.Info("Bluetooth Driver uninstalled");

            #endregion
        }

        private async void ViewModelOnInstallButtonClicked(object sender, EventArgs eventArgs)
        {
            #region Preparation

            // get selected Bluetooth devices
            var donglesToInstall =
                BluetoothStackPanel.Children.Cast<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => c.Content)
                    .Cast<WdiUsbDevice>()
                    .ToList();

            // get selected DualShock 3 devices
            var ds3SToInstall =
                DualShock3StackPanel.Children.Cast<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => c.Content)
                    .Cast<WdiUsbDevice>()
                    .ToList();

            // get selected DualShock 4 devices
            var ds4SToInstall =
                DualShock4StackPanel.Children.Cast<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => c.Content)
                    .Cast<WdiUsbDevice>()
                    .ToList();

            #endregion

            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.Wait;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;

            #endregion

            #region 3rd Party Package Installation

            if (_viewModel.InstallMsvc2010Redist)
            {
                Log.Info("Installing Microsoft Visual C++ 2010 Redistributable Package");
                await RedistPackageInstaller.Instance.DownloadAndInstallMsvc2010Async();
            }

            if (_viewModel.InstallMsvc2013Redist)
            {
                Log.Info("Installing Visual C++ Redistributable Packages für Visual Studio 2013");
                await RedistPackageInstaller.Instance.DownloadAndInstallMsvc2013Async();
            }

            if (_viewModel.InstallDirectXRuntime)
            {
                Log.Info("Installing DirectX Runtime");
                await RedistPackageInstaller.Instance.DownloadAndInstallDirectXRedistAsync();
            }

            if (_viewModel.InstallXbox360Driver)
            {
                Log.Info("Installing Xbox 360 Controller Driver for Windows");
                await RedistPackageInstaller.Instance.DownloadAndInstallXbox360DriverAsync();
            }

            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;

            #endregion

            #region Driver Installation

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;
                var forceInstall = _viewModel.ForceDriverInstallation;

                try
                {
                    uint result = 0;

                    var flags = DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT;

                    if (forceInstall)
                        flags |= DifxFlags.DRIVER_PACKAGE_FORCE;

                    var rebootRequired = false;
                    var busInfPath = Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf");

                    if (!Devcon.Find(Settings.Default.VirtualBusClassGuid, ref devPath, ref instanceId))
                    {
                        if (Devcon.Install(busInfPath, ref rebootRequired))
                        {
                            Log.Info("Virtual Bus Driver pre-installed in Windows Driver Store successfully");

                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Created");
                                _busDeviceConfigured = true;
                            }
                            else
                            {
                                Log.Fatal("Virtual Bus Device creation failed");
                                return;
                            }
                        }
                        else
                        {
                            Log.FatalFormat("Virtual Bus Driver pre-installation failed with error {0}",
                                Marshal.GetLastWin32Error());
                            return;
                        }
                    }

                    // install Virtual Bus driver
                    result = _installer.Install(busInfPath, flags,
                        out rebootRequired);

                    _reboot |= rebootRequired;
                    if (result == 0) _busDriverConfigured = true;

                    if (_viewModel.InstallBluetoothDriver)
                    {
                        result = DriverInstaller.InstallBluetoothDongles(donglesToInstall, force: forceInstall);

                        if (result > 0) _bthDriverConfigured = true;
                    }

                    if (_viewModel.InstallDualShock3Driver)
                    {
                        result = DriverInstaller.InstallDualShock3Controllers(ds3SToInstall, force: forceInstall);

                        if (result > 0) _ds3DriverConfigured = true;
                    }

                    if (_viewModel.InstallDualShock4Driver)
                    {
                        result = DriverInstaller.InstallDualShock4Controllers(ds4SToInstall, force: forceInstall);

                        if (result > 0) _ds4DriverConfigured = true;
                    }

                    if (_viewModel.InstallWindowsService)
                    {
                        IDictionary state = new Hashtable();
                        var service =
                            new AssemblyInstaller(Directory.GetCurrentDirectory() + @"\ScpService.exe", null);

                        state.Clear();
                        service.UseNewContext = true;

                        service.Install(state);
                        service.Commit(state);

                        if (Start(Settings.Default.ScpServiceName))
                            Logger(DifxLog.DIFXAPI_INFO, 0, Settings.Default.ScpServiceName + " Started.");
                        else _reboot = true;

                        _scpServiceConfigured = true;
                    }
                }
                catch (Win32Exception w32Ex)
                {
                    switch (w32Ex.NativeErrorCode)
                    {
                        case 1073: // ERROR_SERVICE_EXISTS
                            Log.WarnFormat("Service already exists, skipping installation...");
                            break;
                        default:
                            Log.ErrorFormat("Win32-Error during installation: {0}", w32Ex);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error during installation: {0}", ex);
                }
            });

            #endregion

            #region Post-Installation

            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
            Cursor = _saved;

            if (_reboot)
                Log.InfoFormat("[Reboot Required]");

            Log.Info("-- Install Summary --");
            if (_scpServiceConfigured)
                Log.Info("SCP DSx Service installed");

            if (_busDeviceConfigured)
                Log.Info("Bus Device installed");

            if (_busDriverConfigured)
                Log.Info("Bus Driver installed");

            if (_ds3DriverConfigured)
                Log.Info("DualShock 3 USB Driver installed");

            if (_bthDriverConfigured)
                Log.Info("Bluetooth Driver installed");

            if (_ds4DriverConfigured)
                Log.Info("DualShock 4 USB Driver installed");

            #endregion
        }

        #endregion

        #region Window events

        private void Window_Initialized(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [Built: {1}]", Assembly.GetExecutingAssembly().GetName().Version,
                AssemblyHelper.LinkerTimestamp);

            _installer = Difx.Instance;
            _installer.OnLogEvent += Logger;

            var info = OsInfoHelper.OsInfo;
            _valid = OsInfoHelper.OsParse(info);

            Log.InfoFormat("{0} detected", info);

            // is MSVC already installed?
            _viewModel.InstallMsvc2010Redist = !OsInfoHelper.IsVc2010Installed;
            _viewModel.InstallMsvc2013Redist = !OsInfoHelper.IsVc2013Installed;

            // unblock system files
            foreach (var fInfo in Directory.GetFiles(WorkingDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".dll") || s.EndsWith(".inf") || s.EndsWith(".sys") || s.EndsWith(".cat"))
                .Select(file => new FileInfo(file))
                .Where(fInfo => fInfo.Unblock()))
            {
                Log.InfoFormat("Unblocked file {0}", fInfo.Name);
            }

            // get all local USB devices
            foreach (var usbDevice in WdiWrapper.Instance.UsbDeviceList)
            {
                BluetoothStackPanel.Children.Add(new CheckBox
                {
                    Content = usbDevice
                });

                DualShock3StackPanel.Children.Add(new CheckBox
                {
                    Content = usbDevice
                });

                DualShock4StackPanel.Children.Add(new CheckBox
                {
                    Content = usbDevice
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // link download progress to progress bar
            RedistPackageInstaller.Instance.ProgressChanged +=
                (o, args) => { Dispatcher.Invoke(() => MainProgressBar.Value = args.CurrentProgressPercentage); };

            // link NotifyAppender to TextBlock
            foreach (
                var appender in
                    LogManager.GetCurrentLoggers()
                        .SelectMany(log => log.Logger.Repository.GetAppenders().OfType<NotifyAppender>()))
            {
                LogTextBlock.DataContext = appender;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hWnd = new WindowInteropHelper(this).Handle;
        }

        #endregion

        #region Windows Service Helpers

        private static bool Start(string service)
        {
            try
            {
                var sc = new ServiceController(service);

                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    Thread.Sleep(1000);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Couldn't start service: {0}", ex);
            }

            return false;
        }

        private static bool Stop(string service)
        {
            try
            {
                var sc = new ServiceController(service);

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    Thread.Sleep(1000);
                    return true;
                }
            }
            catch (InvalidOperationException iopex)
            {
                if (!(iopex.InnerException is Win32Exception))
                {
                    Log.ErrorFormat("Win32-Exception occured: {0}", iopex);
                    return false;
                }

                switch (((Win32Exception) iopex.InnerException).NativeErrorCode)
                {
                    case 1060: // ERROR_SERVICE_DOES_NOT_EXIST
                        Log.Warn("Service doesn't exist, maybe it was uninstalled before");
                        break;
                    default:
                        Log.ErrorFormat("Win32-Error: {0}", (Win32Exception) iopex.InnerException);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Couldn't stop service: {0}", ex);
            }

            return false;
        }

        #endregion
    }
}