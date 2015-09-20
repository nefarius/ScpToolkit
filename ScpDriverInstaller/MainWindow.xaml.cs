using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
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
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        private bool _bthDriverConfigured;
        private bool _busDeviceConfigured;
        private bool _busDriverConfigured;
        private bool _ds3DriverConfigured;
        private bool _ds4DriverConfigured;
        private Difx _installer;
        private bool _reboot;
        private Cursor _saved;
        private bool _scpServiceConfigured;
        private OsType _valid = OsType.Invalid;
        private IntPtr _hWnd;

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

        private void ViewModelOnExitButtonClicked(object sender, EventArgs eventArgs)
        {
            Close();
        }

        private async void ViewModelOnUninstallButtonClicked(object sender, EventArgs eventArgs)
        {

        }

        private async void ViewModelOnInstallButtonClicked(object sender, EventArgs eventArgs)
        {
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

            /*

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


                    if (!Devcon.Find(Settings.Default.VirtualBusClassGuid, ref devPath, ref instanceId))
                    {
                        if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                            "root\\ScpVBus\0\0"))
                        {
                            Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Created");
                            _busDeviceConfigured = true;
                        }
                    }

                    bool rebootRequired;

                    // install Virtual Bus driver
                    result = _installer.Install(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"), flags,
                        out rebootRequired);

                    _reboot |= rebootRequired;
                    if (result == 0) _busDriverConfigured = true;

                    if (_viewModel.InstallBluetoothDriver)
                    {
                        Dispatcher.Invoke(() => result = DriverInstaller.InstallBluetoothDongles(_hWnd, forceInstall));

                        if (result > 0) _bthDriverConfigured = true;
                    }

                    if (_viewModel.InstallDualShock3Driver)
                    {
                        Dispatcher.Invoke(() => result = DriverInstaller.InstallDualShock3Controllers(_hWnd, forceInstall));

                        if (result > 0) _ds3DriverConfigured = true;
                    }

                    if (_viewModel.InstallDualShock4Driver)
                    {
                        Dispatcher.Invoke(() => result = DriverInstaller.InstallDualShock4Controllers(_hWnd, forceInstall));

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

            */

            #region Post-Installation

            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
            Cursor = _saved;

            if (_reboot)
                Log.InfoFormat("[Reboot Required]");

            Log.Info("-- Install Summary --");
            if (_scpServiceConfigured)
                Log.Info("SCP DS3 Service installed");

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

        private void Window_Initialized(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [{1}]", Assembly.GetExecutingAssembly().GetName().Version, DateTime.Now);

            _installer = Difx.Instance;
            _installer.OnLogEvent += Logger;

            var info = OsInfoHelper.OsInfo;
            _valid = OsInfoHelper.OsParse(info);

            Log.InfoFormat("{0} detected", info);

            // is MSVC already installed?
            _viewModel.InstallMsvc2010Redist = !OsInfoHelper.IsVc2010Installed;
            _viewModel.InstallMsvc2013Redist = !OsInfoHelper.IsVc2013Installed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _hWnd = new WindowInteropHelper(this).Handle;

            RedistPackageInstaller.Instance.ProgressChanged += (o, args) =>
            {
                Dispatcher.Invoke(() => MainProgressBar.Value = args.CurrentProgressPercentage);
            };
        }

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


    }
}
