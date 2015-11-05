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
using System.Windows.Media;
using log4net;
using log4net.Appender;
using log4net.Core;
using Mantin.Controls.Wpf.Notification;
using ScpControl.Driver;
using ScpControl.ScpCore;
using ScpControl.Utilities;
using ScpDriverInstaller.Properties;
using ScpDriverInstaller.Utilities;
using ScpDriverInstaller.View_Models;

namespace ScpDriverInstaller
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAppender
    {
        #region Private static fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        #endregion

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
                    Log.Debug(description);
                    break;
                case DifxLog.DIFXAPI_SUCCESS:
                    Log.Info(description);
                    break;
                case DifxLog.DIFXAPI_WARNING:
                    Log.Warn(description);
                    break;
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            if (!IsInitialized)
                return;

            var level = loggingEvent.Level;

            if (level == Level.Info)
            {
                Dispatcher.Invoke(
                    () => ShowPopup("Information", loggingEvent.RenderedMessage, NotificationType.Information));
            }

            if (level == Level.Warn)
            {
                Dispatcher.Invoke(
                    () => ShowPopup("Warning", loggingEvent.RenderedMessage, NotificationType.Warning));
            }

            if (level == Level.Error)
            {
                Dispatcher.Invoke(
                    () => ShowPopup("Error", loggingEvent.RenderedMessage, NotificationType.Error));
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

                        if (StopService(Settings.Default.ScpServiceName))
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

                    switch (((Win32Exception)instex.InnerException).NativeErrorCode)
                    {
                        case 1060: // ERROR_SERVICE_DOES_NOT_EXIST
                            Log.Warn("Service doesn't exist, maybe it was uninstalled before");
                            break;
                        default:
                            Log.ErrorFormat("Win32-Error during uninstallation: {0}",
                                (Win32Exception)instex.InnerException);
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

            if (_viewModel.InstallBluetoothDriver && !donglesToInstall.Any())
            {
                ShowPopup(Properties.Resources.BthListEmpty_Title,
                     Properties.Resources.BthListEmpty_Text,
                     NotificationType.Warning);
            }

            // get selected DualShock 3 devices
            var ds3SToInstall =
                DualShock3StackPanel.Children.Cast<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => c.Content)
                    .Cast<WdiUsbDevice>()
                    .ToList();

            if (_viewModel.InstallDualShock3Driver && !ds3SToInstall.Any())
            {
                ShowPopup(Properties.Resources.Ds3ListEmpty_Title,
                     Properties.Resources.Ds3ListEmpty_Text,
                     NotificationType.Warning);
            }

            // get selected DualShock 4 devices
            var ds4SToInstall =
                DualShock4StackPanel.Children.Cast<CheckBox>()
                    .Where(c => c.IsChecked == true)
                    .Select(c => c.Content)
                    .Cast<WdiUsbDevice>()
                    .ToList();

            if (_viewModel.InstallDualShock4Driver && !ds4SToInstall.Any())
            {
                ShowPopup(Properties.Resources.Ds4ListEmpty_Title,
                     Properties.Resources.Ds4ListEmpty_Text,
                     NotificationType.Warning);
            }

            #endregion

            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.Wait;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
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
                    var busInfPath = Path.Combine(GlobalConfiguration.AppDirectory,
                        "System", "ScpVBus.inf");
                    Log.DebugFormat("ScpVBus.inf path: {0}", busInfPath);

                    // check for existance of Scp VBus
                    if (!Devcon.Find(Settings.Default.VirtualBusClassGuid, ref devPath, ref instanceId))
                    {
                        // if not detected, install Inf-file in Windows Driver Store
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
                            Log.FatalFormat("Virtual Bus Driver pre-installation failed with Win32 error {0}",
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
                            new AssemblyInstaller(Path.Combine(GlobalConfiguration.AppDirectory, "ScpService.exe"), null);

                        state.Clear();
                        service.UseNewContext = true;

                        service.Install(state);
                        service.Commit(state);

                        if (StartService(Settings.Default.ScpServiceName))
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
                            Log.Info("Service already exists, attempting to restart...");
                            
                            StopService(Settings.Default.ScpServiceName);
                            Log.Info("Service stopped successfully");

                            StartService(Settings.Default.ScpServiceName);
                            Log.Info("Service started successfully");
                            break;
                        default:
                            Log.ErrorFormat("Win32-Error during installation: {0}", w32Ex);
                            break;
                    }
                }
                catch (InvalidOperationException iopex)
                {
                    Log.ErrorFormat("Error during installation: {0}", iopex.Message);
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
            // add popup-appender to all loggers
            foreach (var currentLogger in LogManager.GetCurrentLoggers())
            {
                ((log4net.Repository.Hierarchy.Logger)currentLogger.Logger).AddAppender(this);
            }

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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // remove popup-appender from all loggers
            foreach (var currentLogger in LogManager.GetCurrentLoggers())
            {
                ((log4net.Repository.Hierarchy.Logger)currentLogger.Logger).RemoveAppender(this);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hWnd = new WindowInteropHelper(this).Handle;
        }

        #endregion

        #region Windows Service Helpers

        private static bool StartService(string service)
        {
            try
            {
                var sc = new ServiceController(service);

                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    // TODO: improve this!
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

        private static bool StopService(string service)
        {
            try
            {
                var sc = new ServiceController(service);

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    // TODO: improve this!
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

                switch (((Win32Exception)iopex.InnerException).NativeErrorCode)
                {
                    case 1060: // ERROR_SERVICE_DOES_NOT_EXIST
                        Log.Warn("Service doesn't exist, maybe it was uninstalled before");
                        break;
                    default:
                        Log.ErrorFormat("Win32-Error: {0}", (Win32Exception)iopex.InnerException);
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

        #region Private methods

        private void ShowPopup(string title, string message, NotificationType type)
        {
            var popup = new ToastPopUp(title, message, type)
            {
                Background = Background,
                FontColor = Brushes.Bisque,
                FontFamily = FontFamily
            };

            popup.Show();
        }

        #endregion
    }
}
