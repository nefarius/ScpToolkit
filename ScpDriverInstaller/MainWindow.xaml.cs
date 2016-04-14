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
using System.Windows.Interop;
using System.Windows.Navigation;

namespace ScpDriverInstaller
{
    /// <summary>
    ///     Where the wizard does its black magic
    /// </summary>
    public partial class MainWindow : Window, IAppender
    {
        #region Ctor

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => { Log.FatalFormat("An unexpected exception occurred: {0}", args.ExceptionObject); };

            InstallGrid.DataContext = _viewModel;
        }

        #endregion Ctor

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

        #endregion Misc. Helpers

        #region Private methods

        private void ShowPopup(string title, string message, NotificationType type)
        {
            var popup = new ToastPopUp(title, message, type)
            {
                Background = Background,
                FontFamily = FontFamily
            };

            popup.Show();
        }

        #endregion Private methods

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
                                 && !string.IsNullOrEmpty(d.CurrentDriver)
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
                                 && !string.IsNullOrEmpty(d.CurrentDriver)
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
                            !string.IsNullOrEmpty(d.CurrentDriver) &&
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
                            !string.IsNullOrEmpty(d.CurrentDriver) &&
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

        #endregion Device notification events

        #region Wizard events

        private void Wizard_OnHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/nefarius/ScpToolkit/wiki");
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        #endregion Wizard events

        #region Private static fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        #endregion Private static fields

        #region Private fields

        private IntPtr _hWnd;
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

        #endregion Private fields

        #region Button events

        private async void InstallDsOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            var rebootRequired = false;
            var failed = false;
            uint result = 0;
            var ds3InfPath = Path.Combine(GlobalConfiguration.AppDirectory, "WinUSB", "Ds3Controller.inf");
            var ds4InfPath = Path.Combine(GlobalConfiguration.AppDirectory, "WinUSB", "Ds4Controller.inf");

            MainBusyIndicator.SetContentThreadSafe(Properties.Resources.DualShockSetupInstalling3);

            await Task.Run(() => result = Difx.Instance.Install(ds3InfPath,
                DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT | DifxFlags.DRIVER_PACKAGE_FORCE, out rebootRequired));

            // ERROR_NO_SUCH_DEVINST = 0xE000020B
            if (result != 0 && result != 0xE000020B)
            {
                failed = true;

                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupFailedTitle,
                    Properties.Resources.SetupFailedInstructions,
                    Properties.Resources.SetupFailedContent,
                    string.Format(Properties.Resources.SetupFailedVerbose,
                        new Win32Exception(Marshal.GetLastWin32Error()), Marshal.GetLastWin32Error()),
                    Properties.Resources.SetupFailedFooter,
                    TaskDialogIcon.Error);
            }

            MainBusyIndicator.SetContentThreadSafe(Properties.Resources.DualShockSetupInstalling4);

            await Task.Run(() => result = Difx.Instance.Install(ds4InfPath,
                DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT | DifxFlags.DRIVER_PACKAGE_FORCE, out rebootRequired));

            // ERROR_NO_SUCH_DEVINST = 0xE000020B
            if (result != 0 && result != 0xE000020B)
            {
                failed = true;

                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupFailedTitle,
                    Properties.Resources.SetupFailedInstructions,
                    Properties.Resources.SetupFailedContent,
                    string.Format(Properties.Resources.SetupFailedVerbose,
                        new Win32Exception(Marshal.GetLastWin32Error()), Marshal.GetLastWin32Error()),
                    Properties.Resources.SetupFailedFooter,
                    TaskDialogIcon.Error);
            }

            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            if (!failed)
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupSuccessTitle,
                    Properties.Resources.DualShockSetupSuccessInstruction,
                    Properties.Resources.SetupSuccessContent,
                    string.Empty,
                    string.Empty,
                    TaskDialogIcon.Information);
            }

            if (rebootRequired)
            {
                MessageBox.Show(this,
                    Properties.Resources.RebootRequiredContent,
                    Properties.Resources.RebootRequiredTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void InstallBthHostOnClick(object sender, RoutedEventArgs e)
        {
            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            var rebootRequired = false;
            uint result = 0;
            var bhInfPath = Path.Combine(GlobalConfiguration.AppDirectory, "WinUSB", "BluetoothHost.inf");

            MainBusyIndicator.SetContentThreadSafe(Properties.Resources.BluetoothSetupInstalling);

            await Task.Run(() => result = Difx.Instance.Install(bhInfPath,
                DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT | DifxFlags.DRIVER_PACKAGE_FORCE, out rebootRequired));

            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            // ERROR_NO_SUCH_DEVINST = 0xE000020B
            if (result != 0 && result != 0xE000020B)
            {
                // display error message
                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupFailedTitle,
                    Properties.Resources.SetupFailedInstructions,
                    Properties.Resources.SetupFailedContent,
                    string.Format(Properties.Resources.SetupFailedVerbose,
                        new Win32Exception(Marshal.GetLastWin32Error()), Marshal.GetLastWin32Error()),
                    Properties.Resources.SetupFailedFooter,
                    TaskDialogIcon.Error);
                return;
            }

            // display success message
            ExtendedMessageBox.Show(this,
                Properties.Resources.SetupSuccessTitle,
                Properties.Resources.BluetoothSetupSuccessInstruction,
                Properties.Resources.SetupSuccessContent,
                string.Empty,
                string.Empty,
                TaskDialogIcon.Information);

            // display reboot required message
            if (rebootRequired)
            {
                MessageBox.Show(this,
                    Properties.Resources.RebootRequiredContent,
                    Properties.Resources.RebootRequiredTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void InstallVBusOnClick(object sender, RoutedEventArgs e)
        {
            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;
            var failed = false;
            var rebootRequired = false;

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;

                try
                {
                    var busInfPath = Path.Combine(
                        GlobalConfiguration.AppDirectory,
                        "ScpVBus",
                        Environment.Is64BitOperatingSystem ? "amd64" : "x86",
                        "ScpVBus.inf");
                    Log.DebugFormat("ScpVBus.inf path: {0}", busInfPath);

                    // check for existence of Scp VBus
                    if (!Devcon.Find(Settings.Default.VirtualBusClassGuid, ref devPath, ref instanceId))
                    {
                        MainBusyIndicator.SetContentThreadSafe(Properties.Resources.VirtualBusSetupAddingDriverStore);

                        // if not detected, install Inf-file in Windows Driver Store
                        if (Devcon.Install(busInfPath, ref rebootRequired))
                        {
                            Log.Info("Virtual Bus Driver pre-installed in Windows Driver Store successfully");

                            MainBusyIndicator.SetContentThreadSafe(Properties.Resources.VirtualBusSetupCreating);

                            // create pseudo-device so the bus driver can attach to it later
                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Log.Info("Virtual Bus Created");
                            }
                            else
                            {
                                Log.Fatal("Virtual Bus Device creation failed");
                                failed = true;
                            }
                        }
                        else
                        {
                            Log.FatalFormat("Virtual Bus Driver pre-installation failed with Win32 error {0}",
                                (uint)Marshal.GetLastWin32Error());
                            failed = true;
                        }
                    }

                    MainBusyIndicator.SetContentThreadSafe(Properties.Resources.VirtualBusSetupInstalling);

                    // install Virtual Bus driver
                    var result = Difx.Instance.Install(busInfPath,
                        DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT | DifxFlags.DRIVER_PACKAGE_FORCE,
                        out rebootRequired);

                    if (result != 0)
                    {
                        failed = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error during installation: {0}", ex);
                }
            });

            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            // display error message
            if (failed)
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupFailedTitle,
                    Properties.Resources.SetupFailedInstructions,
                    Properties.Resources.SetupFailedContent,
                    string.Format(Properties.Resources.SetupFailedVerbose,
                        new Win32Exception(Marshal.GetLastWin32Error()), Marshal.GetLastWin32Error()),
                    Properties.Resources.SetupFailedFooter,
                    TaskDialogIcon.Error);
                return;
            }

            // display success message
            ExtendedMessageBox.Show(this,
                Properties.Resources.SetupSuccessTitle,
                Properties.Resources.VirtualBusSetupSuccessInstruction,
                Properties.Resources.SetupSuccessContent,
                string.Empty,
                string.Empty,
                TaskDialogIcon.Information);

            // display reboot required message
            if (rebootRequired)
            {
                MessageBox.Show(this,
                    Properties.Resources.RebootRequiredContent,
                    Properties.Resources.RebootRequiredTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void InstallWindowsServiceOnClick(object sender, RoutedEventArgs e)
        {
            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;
            var failed = false;
            var rebootRequired = false;

            await Task.Run(() =>
            {
                try
                {
                    MainBusyIndicator.SetContentThreadSafe(Properties.Resources.ServiceSetupInstalling);

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
                        rebootRequired = true;
                    }
                }
                catch (Win32Exception w32Ex)
                {
                    switch (w32Ex.NativeErrorCode)
                    {
                        case 1073: // ERROR_SERVICE_EXISTS
                            Log.Info("Service already exists");
                            break;

                        default:
                            Log.ErrorFormat("Win32-Error during installation: {0}", w32Ex);
                            failed = true;
                            break;
                    }
                }
                catch (InvalidOperationException iopex)
                {
                    Log.ErrorFormat("Error during installation: {0}", iopex.Message);
                    failed = true;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error during installation: {0}", ex);
                    failed = true;
                }
            });

            MainBusyIndicator.IsBusy = !MainBusyIndicator.IsBusy;

            // display error message
            if (failed)
            {
                ExtendedMessageBox.Show(this,
                    Properties.Resources.SetupFailedTitle,
                    Properties.Resources.SetupFailedInstructions,
                    Properties.Resources.SetupFailedContent,
                    string.Format(Properties.Resources.SetupFailedVerbose,
                        new Win32Exception(Marshal.GetLastWin32Error()), Marshal.GetLastWin32Error()),
                    Properties.Resources.SetupFailedFooter,
                    TaskDialogIcon.Error);
                return;
            }

            // display success message
            ExtendedMessageBox.Show(this,
                Properties.Resources.SetupSuccessTitle,
                Properties.Resources.ServiceSetupSuccessInstruction,
                Properties.Resources.ServiceSetupSuccessContent,
                string.Empty,
                string.Empty,
                TaskDialogIcon.Information);

            // display reboot required message
            if (rebootRequired)
            {
                MessageBox.Show(this,
                    Properties.Resources.RebootRequiredContent,
                    Properties.Resources.RebootRequiredTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        #endregion Button events

        #region Window events

        private void Window_Initialized(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [Built: {1}]", Assembly.GetExecutingAssembly().GetName().Version,
                AssemblyHelper.LinkerTimestamp);

            Log.InfoFormat("{0} detected", OsInfoHelper.OsInfo);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // add popup-appender to all loggers
            foreach (var currentLogger in LogManager.GetCurrentLoggers())
            {
                ((Logger)currentLogger.Logger).AddAppender(this);
            }

            // stop service if exists so no device is occupied
            if (StopService(Settings.Default.ScpServiceName))
            {
                Log.InfoFormat("{0} stopped", Settings.Default.ScpServiceName);
            }

#if NOPE
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
#endif
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // remove popup-appender from all loggers
            foreach (var currentLogger in LogManager.GetCurrentLoggers())
            {
                ((Logger)currentLogger.Logger).RemoveAppender(this);
            }

            // unregister notifications
            {
                _hidUsbDs3.UnregisterHandle();
                _winUsbDs3.UnregisterHandle();
                _hidUsbDs4.UnregisterHandle();
                _winUsbDs4.UnregisterHandle();
                _genericBluetoothHost.UnregisterHandle();
            }

            if (StartService(Settings.Default.ScpServiceName))
            {
                Log.InfoFormat("{0} started", Settings.Default.ScpServiceName);
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

        #endregion Window events

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

        #endregion Windows Service Helpers
    }
}