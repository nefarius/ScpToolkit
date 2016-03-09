using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Mantin.Controls.Wpf.Notification;
using Ookii.Dialogs.Wpf;
using ScpControl.Bluetooth;
using ScpControl.Driver;
using ScpControl.ScpCore;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;
using ScpControl.Usb.PnP;
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

        #region Device notification events

        private void OnUsbDeviceAddedOrRemoved()
        {
            var usbDevices = WdiWrapper.Instance.UsbDeviceList.ToList();
            var supportedBluetoothDevices = IniConfig.Instance.BthDongleDriver.HardwareIds;
            var regex = new Regex("VID_([0-9A-Z]{4})&PID_([0-9A-Z]{4})", RegexOptions.IgnoreCase);

            // HidUsb devices
            {
                DualShockStackPanelHidUsb.Children.Clear();
                _viewModel.InstallDs3ButtonEnabled = false;

                foreach (
                    var usbDevice in
                        usbDevices.Where(
                            d => d.VendorId == _hidUsbDs3.VendorId
                                 && (d.ProductId == _hidUsbDs3.ProductId || d.ProductId == _hidUsbDs4.ProductId)
                                 && d.CurrentDriver.Equals("HidUsb"))
                    )
                {
                    DualShockStackPanelHidUsb.Children.Add(new TextBlock
                    {
                        Text = string.Format("Device #{0}: {1}", DualShockStackPanelHidUsb.Children.Count, usbDevice),
                        Tag = usbDevice,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 10)
                    });

                    _viewModel.InstallDs3ButtonEnabled = true;
                }
            }

            // WinUsb devices
            {
                DualShockStackPanelWinUsb.Children.Clear();

                foreach (
                    var usbDevice in
                        usbDevices.Where(
                            d => d.VendorId == _hidUsbDs3.VendorId
                                 && (d.ProductId == _hidUsbDs3.ProductId || d.ProductId == _hidUsbDs4.ProductId)
                                 && d.CurrentDriver.Equals("WinUSB"))
                    )
                {
                    DualShockStackPanelWinUsb.Children.Add(new TextBlock
                    {
                        Text = string.Format("Device #{0}: {1}", DualShockStackPanelWinUsb.Children.Count, usbDevice),
                        Tag = usbDevice,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 10)
                    });
                }
            }

            // refresh devices filtering on supported Bluetooth hardware IDs and BTHUSB driver (uninitialized)
            {
                var uninitialized =
                    usbDevices.Where(
                        d =>
                            d.CurrentDriver.Equals("BTHUSB") &&
                            supportedBluetoothDevices.Any(s => s.Contains(regex.Match(d.HardwareId).Value)));

                BluetoothStackPanelDefault.Children.Clear();
                _viewModel.InstallBthButtonEnabled = false;

                foreach (var usbDevice in uninitialized)
                {
                    BluetoothStackPanelDefault.Children.Add(new TextBlock
                    {
                        Text = string.Format("Device #{0}: {1}", BluetoothStackPanelDefault.Children.Count, usbDevice),
                        Tag = usbDevice,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 10)
                    });

                    _viewModel.InstallBthButtonEnabled = true;
                }
            }

            // refresh devices filtering on supported Bluetooth hardware IDs and WinUSB driver (initialized)
            {
                var initialized =
                    usbDevices.Where(
                        d =>
                            d.CurrentDriver.Equals("WinUSB") &&
                            supportedBluetoothDevices.Any(s => s.Contains(regex.Match(d.HardwareId).Value)));

                BluetoothStackPanelWinUsb.Children.Clear();

                foreach (var usbDevice in initialized)
                {
                    BluetoothStackPanelWinUsb.Children.Add(new TextBlock
                    {
                        Text = string.Format("Device #{0}: {1}", BluetoothStackPanelWinUsb.Children.Count, usbDevice),
                        Tag = usbDevice,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 10)
                    });
                }
            }
        }

        #endregion

        #region Private static fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        #endregion

        #region Misc. Helpers

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
        private readonly UsbNotifier _hidUsbDs3 = new UsbNotifier(0x054C, 0x0268);
        private readonly UsbNotifier _winUsbDs3 = new UsbNotifier(0x054C, 0x0268, UsbDs3.DeviceClassGuid);
        private readonly UsbNotifier _hidUsbDs4 = new UsbNotifier(0x054C, 0x05C4);
        private readonly UsbNotifier _winUsbDs4 = new UsbNotifier(0x054C, 0x05C4, UsbDs4.DeviceClassGuid);

        /// <summary>
        ///     The GUID_BTHPORT_DEVICE_INTERFACE device interface class is defined for Bluetooth radios.
        /// </summary>
        /// <remarks>https://msdn.microsoft.com/en-us/library/windows/hardware/ff545033(v=vs.85).aspx</remarks>
        private readonly UsbNotifier _genericBluetoothHost =
            new UsbNotifier(Guid.Parse("{0850302A-B344-4fda-9BE9-90576B8D46F0}"));

        private readonly UsbNotifier _winUsbBluetoothHost = new UsbNotifier(BthDongle.DeviceClassGuid);

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
                            Log.InfoFormat("{0} stopped", Settings.Default.ScpServiceName);
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
                            Log.Info("Virtual Bus Removed");
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
                            Log.Error("Virtual Bus Removal Failure");
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
            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.Wait;
            InstallGrid.IsEnabled = !InstallGrid.IsEnabled;
            MainProgressBar.IsIndeterminate = !MainProgressBar.IsIndeterminate;

            #endregion

            #region Driver Installation

            await Task.Run(() =>
            {
                try
                {
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
                        {
                            Log.InfoFormat("{0} started", Settings.Default.ScpServiceName);
                        }
                        else
                        {
                            _reboot = true;
                        }

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

        #region Button events

        private void InstallDsOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            WdiErrorCode ds3Result = WdiErrorCode.WDI_SUCCESS, ds4Result = WdiErrorCode.WDI_SUCCESS;

            var ds3SToInstall =
                DualShockStackPanelHidUsb.Children.Cast<TextBlock>()
                    .Select(c => c.Tag)
                    .Cast<WdiDeviceInfo>()
                    .Where(d => d.VendorId == _hidUsbDs3.VendorId && d.ProductId == _hidUsbDs3.ProductId)
                    .ToList();

            if (ds3SToInstall.Any())
            {
                ds3Result = DriverInstaller.InstallDualShock3Controller(ds3SToInstall.First(), _hWnd);
            }

            var ds4SToInstall =
                DualShockStackPanelHidUsb.Children.Cast<TextBlock>()
                    .Select(c => c.Tag)
                    .Cast<WdiDeviceInfo>()
                    .Where(d => d.VendorId == _hidUsbDs4.VendorId && d.ProductId == _hidUsbDs4.ProductId)
                    .ToList();

            if (ds4SToInstall.Any())
            {
                ds4Result = DriverInstaller.InstallDualShock4Controller(ds4SToInstall.First(), _hWnd);
            }

            // display success or failure message
            if (ds3Result == WdiErrorCode.WDI_SUCCESS && ds4Result == WdiErrorCode.WDI_SUCCESS)
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.DsInstOk_Title,
                    Properties.Resources.DsInstOk_Instruction,
                    Properties.Resources.DsInstOk_Content,
                    Properties.Resources.DsInstOk_Verbose,
                    Properties.Resources.DsInstOk_Footer,
                    TaskDialogIcon.Information);
            }
            else
            {
                if (ds3Result != WdiErrorCode.WDI_SUCCESS)
                {
                    ExtendedMessageBox.Show(this,
                        Properties.Resources.DsInstError_Title,
                        Properties.Resources.DsInstError_Instruction,
                        Properties.Resources.DsInstError_Content,
                        string.Format(Properties.Resources.DsInstError_Verbose,
                            WdiWrapper.Instance.GetErrorMessage(ds3Result), ds3Result),
                        Properties.Resources.DsInstError_Footer,
                        TaskDialogIcon.Error);
                    return;
                }

                if (ds4Result != WdiErrorCode.WDI_SUCCESS)
                {
                    ExtendedMessageBox.Show(this,
                        Properties.Resources.DsInstError_Title,
                        Properties.Resources.DsInstError_Instruction,
                        Properties.Resources.DsInstError_Content,
                        string.Format(Properties.Resources.DsInstError_Verbose,
                            WdiWrapper.Instance.GetErrorMessage(ds4Result), ds4Result),
                        Properties.Resources.DsInstError_Footer,
                        TaskDialogIcon.Error);
                }
            }
        }

        private void InstallBthHostOnClick(object sender, RoutedEventArgs e)
        {
            var bthResult = WdiErrorCode.WDI_SUCCESS;

            var bthToInstall =
                BluetoothStackPanelDefault.Children.Cast<TextBlock>()
                    .Select(c => c.Tag)
                    .Cast<WdiDeviceInfo>()
                    .ToList();

            if (bthToInstall.Any())
            {
                bthResult = DriverInstaller.InstallBluetoothHost(bthToInstall.First(), _hWnd);
            }

            // display success or failure message
            if (bthResult == WdiErrorCode.WDI_SUCCESS)
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.BthInstOk_Title,
                    Properties.Resources.BthInstOk_Instruction,
                    Properties.Resources.BthInstOk_Content,
                    Properties.Resources.BthInstOk_Verbose,
                    Properties.Resources.BthInstOk_Footer,
                    TaskDialogIcon.Information);
            }
            else
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.DsInstError_Title,
                    Properties.Resources.DsInstError_Instruction,
                    Properties.Resources.DsInstError_Content,
                    string.Format(Properties.Resources.DsInstError_Verbose,
                        WdiWrapper.Instance.GetErrorMessage(bthResult), bthResult),
                    Properties.Resources.DsInstError_Footer,
                    TaskDialogIcon.Error);
            }
        }

        private async void InstallVBusOnClick(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;
                var forceInstall = _viewModel.ForceDriverInstallation;

                try
                {
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

                            // create pseudo-device so the bus driver can attach to it later
                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Log.Info("Virtual Bus Created");
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
                                (uint) Marshal.GetLastWin32Error());
                            return;
                        }
                    }

                    // install Virtual Bus driver
                    var result = _installer.Install(busInfPath, flags,
                        out rebootRequired);

                    _reboot |= rebootRequired;
                    if (result == 0) _busDriverConfigured = true;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error during installation: {0}", ex);
                }
            });
        }

        #endregion

        #region Window events

        private void Window_Initialized(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [Built: {1}]", Assembly.GetExecutingAssembly().GetName().Version,
                AssemblyHelper.LinkerTimestamp);

            _installer = Difx.Instance;

            Log.InfoFormat("{0} detected", OsInfoHelper.OsInfo);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // add popup-appender to all loggers
            foreach (var currentLogger in LogManager.GetCurrentLoggers())
            {
                ((Logger) currentLogger.Logger).AddAppender(this);
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
                ((Logger) currentLogger.Logger).RemoveAppender(this);
            }

            // unregister notifications
            {
                _hidUsbDs3.UnregisterHandle();
                _winUsbDs3.UnregisterHandle();
                _hidUsbDs4.UnregisterHandle();
                _winUsbDs4.UnregisterHandle();
                _genericBluetoothHost.UnregisterHandle();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // get native window handle
            _hWnd = new WindowInteropHelper(this).Handle;

            // listen for DualShock 3 plug-in events (HidUsb)
            {
                _hidUsbDs3.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _hidUsbDs3.OnSpecifiedDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _hidUsbDs3.RegisterHandle(_hWnd);
                _hidUsbDs3.CheckDevicePresent();
            }

            // listen for DualShock 3 plug-in events (WinUSB)
            {
                _winUsbDs3.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbDs3.OnSpecifiedDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbDs3.RegisterHandle(_hWnd);
                _winUsbDs3.CheckDevicePresent();
            }

            // listen for DualShock 4 plug-in events (HidUsb)
            {
                _hidUsbDs4.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _hidUsbDs4.OnSpecifiedDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _hidUsbDs4.RegisterHandle(_hWnd);
                _hidUsbDs4.CheckDevicePresent();
            }

            // listen for DualShock 4 plug-in events (HidUsb)
            {
                _winUsbDs4.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbDs4.OnSpecifiedDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbDs4.RegisterHandle(_hWnd);
                _winUsbDs4.CheckDevicePresent();
            }

            // listen for Bluetooth devices (BTHUSB or WinUSB)
            {
                _genericBluetoothHost.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _genericBluetoothHost.OnDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _genericBluetoothHost.RegisterHandle(_hWnd);
            }

            // listen for Bluetooth devices (WinUSB, initialized)
            {
                _winUsbBluetoothHost.OnDeviceRemoved += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbBluetoothHost.OnDeviceArrived += (sender, args) => OnUsbDeviceAddedOrRemoved();
                _winUsbBluetoothHost.RegisterHandle(_hWnd);
            }

            // refresh all lists
            OnUsbDeviceAddedOrRemoved();

            // hook into WndProc
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null) source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hidUsbDs3.ParseMessages(msg, wParam);
            _winUsbDs3.ParseMessages(msg, wParam);
            _hidUsbDs4.ParseMessages(msg, wParam);
            _winUsbDs4.ParseMessages(msg, wParam);
            _genericBluetoothHost.ParseMessages(msg, wParam);
            _winUsbBluetoothHost.ParseMessages(msg, wParam);

            return IntPtr.Zero;
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
                    sc.WaitForStatus(ServiceControllerStatus.Running);
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
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
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

        #region Wizard events

        private void Wizard_OnHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/nefarius/ScpToolkit/wiki/Welcome-to-the-ScpToolkit-documentation!");
        }

        #endregion
    }
}
