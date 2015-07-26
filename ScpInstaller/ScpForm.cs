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
using ScpControl.Utilities;
using ScpDriver.Driver;
using ScpDriver.Properties;

namespace ScpDriver
{
    public partial class ScpForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string[] _desc = { "SUCCESS", "INFO   ", "WARNING", "ERROR  " };
        private Difx _installer;
        private Cursor _saved;
        private bool BTH_Driver_Configured;
        private bool Bus_Device_Configured;
        private bool Bus_Driver_Configured;
        private bool DS3_Driver_Configured;
        private bool Reboot;
        private bool Scp_Service_Configured;
        private OsType Valid = OsType.Invalid;

        public ScpForm()
        {
            InitializeComponent();

            try
            {
                // get absolute path to XML file
                var cfgFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Assembly.GetExecutingAssembly().GetName().Name + ".xml");
                // deserialize file content
                var cfg = ScpDriver.Deserialize(cfgFile);

                // set display options
                cbService.Checked = cbService.Visible = bool.Parse(cfg.Service);
                cbBluetooth.Checked = cbBluetooth.Visible = bool.Parse(cfg.Bluetooth);
                cbDS3.Checked = cbDS3.Visible = bool.Parse(cfg.DualShock3);
                cbBus.Checked = cbBus.Visible = bool.Parse(cfg.VirtualBus);

                // Don't install Service if Bus not Enabled
                if (!bool.Parse(cfg.VirtualBus))
                    cbService.Checked = cbService.Visible = bool.Parse(cfg.VirtualBus);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Couldn't load configuration: {0}", ex);
            }
        }

        private void Logger(DifxLog Event, int error, string description)
        {
            Log.InfoFormat("{0} - {1}", _desc[(int)Event], description);
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

        private void ScpForm_Load(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [{1}]", Application.ProductVersion, DateTime.Now);

            _installer = Difx.Factory();
            _installer.onLogEvent += Logger;

            var info = OsInfoHelper.OsInfo();
            Valid = OsInfoHelper.OsParse(info);

            Log.InfoFormat("{0} detected", info);

            if (Valid == OsType.Invalid)
            {
                btnInstall.Enabled = false;
                btnUninstall.Enabled = false;

                Log.Error("Could not find a valid configuration");
            }
            else
            {
                btnInstall.Enabled = true;
                btnUninstall.Enabled = true;

                Log.InfoFormat("Selected {0} configuration", Valid);
            }

            Icon = Resources.Scp_All;
        }

        private async void btnInstall_Click(object sender, EventArgs e)
        {
            #region Pre-Installation

            _saved = Cursor;
            Cursor = Cursors.WaitCursor;

            btnInstall.Enabled = false;
            btnUninstall.Enabled = false;
            btnExit.Enabled = false;

            Bus_Device_Configured = false;
            Bus_Driver_Configured = false;
            DS3_Driver_Configured = false;
            BTH_Driver_Configured = false;
            Scp_Service_Configured = false;

            pbRunning.Style = ProgressBarStyle.Marquee;

            #endregion

            #region Installation

            await Task.Run(() =>
            {
                string devPath = string.Empty, InstanceId = string.Empty;

                try
                {
                    uint result = 0;
                    bool rebootRequired;

                    var flags = DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT;

                    if (cbForce.Checked)
                        flags |= DifxFlags.DRIVER_PACKAGE_FORCE;

                    if (cbBus.Checked)
                    {
                        if (!Devcon.Find(Settings.Default.Ds3BusClassGuid, ref devPath, ref InstanceId))
                        {
                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Created");
                                Bus_Device_Configured = true;
                            }
                        }

                        result = _installer.Install(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"), flags,
                            out rebootRequired);
                        Reboot |= rebootRequired;
                        if (result == 0) Bus_Driver_Configured = true;
                    }

                    if (cbBluetooth.Checked)
                    {
                        result = _installer.Install(Path.Combine(Settings.Default.InfFilePath, @"BthWinUsb.inf"), flags,
                            out rebootRequired);
                        Reboot |= rebootRequired;
                        if (result == 0) BTH_Driver_Configured = true;
                    }

                    if (cbDS3.Checked)
                    {
                        result = _installer.Install(Path.Combine(Settings.Default.InfFilePath, @"Ds3WinUsb.inf"), flags,
                            out rebootRequired);
                        Reboot |= rebootRequired;
                        if (result == 0) DS3_Driver_Configured = true;
                    }

                    if (cbService.Checked)
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
                        else Reboot = true;

                        Scp_Service_Configured = true;
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
            if (Reboot)
                Log.InfoFormat("[Reboot Required]");

            Log.Info("-- Install Summary --");
            if (Scp_Service_Configured)
                Log.Info("SCP DS3 Service installed");

            if (Bus_Device_Configured)
                Log.Info("Bus Device installed");

            if (Bus_Driver_Configured)
                Log.Info("Bus Driver installed");

            if (DS3_Driver_Configured)
                Log.Info("DS3 USB Driver installed");

            if (BTH_Driver_Configured)
                Log.Info("Bluetooth Driver installed");

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

            Bus_Device_Configured = false;
            Bus_Driver_Configured = false;
            DS3_Driver_Configured = false;
            BTH_Driver_Configured = false;
            Scp_Service_Configured = false;

            pbRunning.Style = ProgressBarStyle.Marquee;

            #endregion

            #region Uninstallation

            await Task.Run(() =>
            {
                string devPath = string.Empty, instanceId = string.Empty;

                try
                {
                    uint result = 0;
                    bool rebootRequired;

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
                        Scp_Service_Configured = true;
                    }

                    if (cbBluetooth.Checked)
                    {
                        result = _installer.Uninstall(Path.Combine(Settings.Default.InfFilePath, @"BthWinUsb.inf"),
                            DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                            out rebootRequired);
                        Reboot |= rebootRequired;
                        if (result == 0) BTH_Driver_Configured = true;
                    }

                    if (cbDS3.Checked)
                    {
                        result = _installer.Uninstall(Path.Combine(Settings.Default.InfFilePath, @"Ds3WinUsb.inf"),
                            DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                            out rebootRequired);
                        Reboot |= rebootRequired;
                        if (result == 0) DS3_Driver_Configured = true;
                    }

                    if (cbBus.Checked && Devcon.Find(Settings.Default.Ds3BusClassGuid, ref devPath, ref instanceId))
                    {
                        if (Devcon.Remove(Settings.Default.Ds3BusClassGuid, devPath, instanceId))
                        {
                            Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Removed");
                            Bus_Device_Configured = true;

                            _installer.Uninstall(Path.Combine(Settings.Default.InfFilePath, @"ScpVBus.inf"),
                                DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                                out rebootRequired);
                            Reboot |= rebootRequired;
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
                            Log.ErrorFormat("Win32-Error during uninstallation: {0}", (Win32Exception)instex.InnerException);
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
            if (Reboot)
                Log.Info(" [Reboot Required]");

            Log.Info("-- Uninstall Summary --");

            if (Scp_Service_Configured)
                Log.Info("SCP DS3 Service uninstalled");

            if (Bus_Device_Configured)
                Log.Info("Bus Device uninstalled");

            if (Bus_Driver_Configured)
                Log.Info("Bus Driver uninstalled");

            if (DS3_Driver_Configured)
                Log.Info("DS3 USB Driver uninstalled");

            if (BTH_Driver_Configured)
                Log.Info("Bluetooth Driver uninstalled");

            #endregion
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}