using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using ScpControl.Driver;
using ScpControl.Utilities;
using ScpDriver.Properties;

namespace ScpDriver
{
    public partial class ScpForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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

        public ScpForm()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => { Log.FatalFormat("An unhandled exception occured: {0}", args.ExceptionObject); };

            try
            {
                // get absolute path to XML file
                var cfgFile =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                        Assembly.GetExecutingAssembly().GetName().Name + ".xml");
                // deserialize file content
                var cfg = ScpDriver.Deserialize(cfgFile);

                // set display options
                cbService.Checked = cbService.Visible = cfg.InstallService;
                cbBluetooth.Checked = cbBluetooth.Visible = cfg.InstallBluetooth;
                cbDS3.Checked = cbDS3.Visible = cfg.InstallDualShock3;
                cbBus.Checked = cbBus.Visible = cfg.InstallVirtualBus;

                // Don't install Service if Bus not Enabled
                if (!cfg.InstallVirtualBus)
                    cbService.Checked = cbService.Visible = cfg.InstallVirtualBus;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Couldn't load configuration: {0}", ex);
            }
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

        private void ScpForm_Load(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [{1}]", Application.ProductVersion, DateTime.Now);

            _installer = Difx.Instance;
            _installer.OnLogEvent += Logger;

            var info = OsInfoHelper.OsInfo;
            _valid = OsInfoHelper.OsParse(info);

            Log.InfoFormat("{0} detected", info);

            if (_valid == OsType.Invalid)
            {
                btnInstall.Enabled = false;
                btnUninstall.Enabled = false;

                Log.Error("Could not find a valid configuration");
            }
            else
            {
                btnInstall.Enabled = true;
                btnUninstall.Enabled = true;

                Log.InfoFormat("Selected {0} configuration", _valid);
            }

            Icon = Resources.Scp_All;

            if (!OsInfoHelper.IsVc2013Installed)
            {
                MessageBox.Show(Resources.ScpForm_VcppMissingText, Resources.ScpForm_VcppMissingHead,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private async void btnInstall_Click(object sender, EventArgs e)
        {
            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.WaitCursor;

            btnInstall.Enabled = false;
            btnUninstall.Enabled = false;
            btnExit.Enabled = false;

            _busDeviceConfigured = false;
            _busDriverConfigured = false;
            _ds3DriverConfigured = false;
            _bthDriverConfigured = false;
            _scpServiceConfigured = false;

            pbRunning.Style = ProgressBarStyle.Marquee;

            var forceInstall = cbForce.Checked;
            var installBus = cbBus.Checked;
            var installBth = cbBluetooth.Checked;
            var installDs3 = cbDS3.Checked;
            var installDs4 = cbDs4.Checked;
            var installService = cbService.Checked;

            #endregion

            #region Installation

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;

                try
                {
                    uint result = 0;

                    var flags = DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT;

                    if (forceInstall)
                        flags |= DifxFlags.DRIVER_PACKAGE_FORCE;

                    if (installBus)
                    {
                        if (!Devcon.Find(Settings.Default.Ds3BusClassGuid, ref devPath, ref instanceId))
                        {
                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Created");
                                _busDeviceConfigured = true;
                            }
                        }

                        bool rebootRequired;
                        result = _installer.Install(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"), flags,
                            out rebootRequired);
                        _reboot |= rebootRequired;
                        if (result == 0) _busDriverConfigured = true;
                    }

                    if (installBth)
                    {
                        Invoke(
                            (MethodInvoker) delegate { result = DriverInstaller.InstallBluetoothDongles(Handle, forceInstall); });
                        if (result > 0) _bthDriverConfigured = true;
                    }

                    if (installDs3)
                    {
                        Invoke(
                            (MethodInvoker)
                                delegate { result = DriverInstaller.InstallDualShock3Controllers(Handle, forceInstall); });
                        if (result > 0) _ds3DriverConfigured = true;
                    }

                    if (installDs4)
                    {
                        Invoke(
                            (MethodInvoker)
                                delegate { result = DriverInstaller.InstallDualShock4Controllers(Handle, forceInstall); });
                        if (result > 0) _ds4DriverConfigured = true;
                    }

                    if (installService)
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

            pbRunning.Style = ProgressBarStyle.Continuous;

            btnInstall.Enabled = true;
            btnUninstall.Enabled = true;
            btnExit.Enabled = true;

            Cursor = _saved;

            Log.Info("Install Succeeded.");
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
                Log.Info("DS3 USB Driver installed");

            if (_bthDriverConfigured)
                Log.Info("Bluetooth Driver installed");

            if (_ds4DriverConfigured)
                Log.Info("DS4 USB Driver installed");

            #endregion
        }

        private async void btnUninstall_Click(object sender, EventArgs e)
        {
            #region Pre-Uninstallation

            _saved = Cursor;
            Cursor = Cursors.WaitCursor;

            btnInstall.Enabled = false;
            btnUninstall.Enabled = false;
            btnExit.Enabled = false;

            _busDeviceConfigured = false;
            _busDriverConfigured = false;
            _ds3DriverConfigured = false;
            _bthDriverConfigured = false;
            _scpServiceConfigured = false;

            pbRunning.Style = ProgressBarStyle.Marquee;

            #endregion

            #region Uninstallation

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;

                try
                {
                    uint result = 0;
                    var rebootRequired = false;

                    if (cbService.Checked)
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

                    if (cbBluetooth.Checked)
                    {
                        DriverInstaller.UninstallBluetoothDongles(ref rebootRequired);
                        _reboot |= rebootRequired;
                    }

                    if (cbDS3.Checked)
                    {
                        DriverInstaller.UninstallDualShock3Controllers(ref rebootRequired);
                        _reboot |= rebootRequired;
                    }

                    if (cbDs4.Checked)
                    {
                        DriverInstaller.UninstallDualShock4Controllers(ref rebootRequired);
                        _reboot |= rebootRequired;
                    }

                    if (cbBus.Checked && Devcon.Find(Settings.Default.Ds3BusClassGuid, ref devPath, ref instanceId))
                    {
                        if (Devcon.Remove(Settings.Default.Ds3BusClassGuid, devPath, instanceId))
                        {
                            Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Removed");
                            _busDeviceConfigured = true;

                            _installer.Uninstall(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"),
                                DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                                out rebootRequired);
                            _reboot |= rebootRequired;
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

            pbRunning.Style = ProgressBarStyle.Continuous;

            btnInstall.Enabled = true;
            btnUninstall.Enabled = true;
            btnExit.Enabled = true;

            Cursor = _saved;

            Log.Info("Uninstall Succeeded.");
            if (_reboot)
                Log.Info(" [Reboot Required]");

            Log.Info("-- Uninstall Summary --");

            if (_scpServiceConfigured)
                Log.Info("SCP DS3 Service uninstalled");

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

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}