using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using System.Threading.Tasks;
using log4net;
using ScpDriver.Utilities;

namespace ScpDriver
{
    public partial class ScpForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected String DS3_BUS_CLASS_GUID = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";

        protected Cursor Saved;
        protected Difx Installer;

        protected Boolean Bus_Device_Configured = false;
        protected Boolean Bus_Driver_Configured = false;
        protected Boolean DS3_Driver_Configured = false;
        protected Boolean BTH_Driver_Configured = false;
        protected Boolean Scp_Service_Configured = false;

        protected Boolean Reboot = false;
        protected OsType Valid = OsType.Invalid;
        protected String InfPath = @".\System\";
        protected String ScpService = "SCP DS3 Service";

        protected String[] Desc = new String[] { "SUCCESS", "INFO   ", "WARNING", "ERROR  " };

        private void Logger(DifxLog Event, Int32 error, String description)
        {
            Log.InfoFormat("{0} - {1}", Desc[(Int32)Event], description);
        }

        protected Boolean Start(String Service)
        {
            try
            {
                ServiceController sc = new ServiceController("SCP DS3 Service");

                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start(); Thread.Sleep(1000);
                    return true;
                }
            }
            catch { }

            return false;
        }

        protected Boolean Stop(String Service)
        {
            try
            {
                ServiceController sc = new ServiceController("SCP DS3 Service");

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop(); Thread.Sleep(1000);
                    return true;
                }
            }
            catch { }

            return false;
        }

        protected Boolean Configuration()
        {
            Boolean Loaded = true, Enabled = true;

            try
            {
                String m_File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".xml";
                XmlDocument m_Xdoc = new XmlDocument();
                XmlNode Item;

                m_Xdoc.Load(m_File);

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/ScpDriver/Service"); Boolean.TryParse(Item.InnerText, out Enabled);
                    cbService.Checked = cbService.Visible = Enabled;
                }
                catch { }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/ScpDriver/Bluetooth"); Boolean.TryParse(Item.InnerText, out Enabled);
                    cbBluetooth.Checked = cbBluetooth.Visible = Enabled;
                }
                catch { }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/ScpDriver/DualShock3"); Boolean.TryParse(Item.InnerText, out Enabled);
                    cbDS3.Checked = cbDS3.Visible = Enabled;
                }
                catch { }

                try
                {
                    Item = m_Xdoc.SelectSingleNode("/ScpDriver/VirtualBus"); Boolean.TryParse(Item.InnerText, out Enabled);
                    cbBus.Checked = cbBus.Visible = Enabled;

                    // Don't install Service if Bus not Enabled
                    if (!Enabled) cbService.Checked = cbService.Visible = Enabled;
                }
                catch { }
            }
            catch { Loaded = false; }

            return Loaded;
        }

        public ScpForm()
        {
            InitializeComponent();

            Configuration();
        }

        private void ScpForm_Load(object sender, EventArgs e)
        {
            Log.InfoFormat("SCP Driver Installer {0} [{1}]", Application.ProductVersion, DateTime.Now);

            Installer = Difx.Factory();
            Installer.onLogEvent += Logger;

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

            Icon = Properties.Resources.Scp_All;
        }

        private async void btnInstall_Click(object sender, EventArgs e)
        {
            #region Pre-Installation

            Saved = Cursor;
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
                String DevPath = String.Empty, InstanceId = String.Empty;

                try
                {
                    UInt32 Result = 0;
                    Boolean RebootRequired = false;

                    DifxFlags Flags = DifxFlags.DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT;

                    if (cbForce.Checked) Flags |= DifxFlags.DRIVER_PACKAGE_FORCE;

                    if (cbBus.Checked)
                    {
                        if (!Devcon.Find(new Guid(DS3_BUS_CLASS_GUID), ref DevPath, ref InstanceId))
                        {
                            if (Devcon.Create("System", new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),
                                "root\\ScpVBus\0\0"))
                            {
                                Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Created");
                                Bus_Device_Configured = true;
                            }
                        }

                        Result = Installer.Install(InfPath + @"ScpVBus.inf", Flags, out RebootRequired);
                        Reboot |= RebootRequired;
                        if (Result == 0) Bus_Driver_Configured = true;
                    }

                    if (cbBluetooth.Checked)
                    {
                        Result = Installer.Install(InfPath + @"BthWinUsb.inf", Flags, out RebootRequired);
                        Reboot |= RebootRequired;
                        if (Result == 0) BTH_Driver_Configured = true;
                    }

                    if (cbDS3.Checked)
                    {
                        Result = Installer.Install(InfPath + @"Ds3WinUsb.inf", Flags, out RebootRequired);
                        Reboot |= RebootRequired;
                        if (Result == 0) DS3_Driver_Configured = true;
                    }

                    if (cbService.Checked)
                    {
                        IDictionary State = new Hashtable();
                        AssemblyInstaller Service =
                            new AssemblyInstaller(Directory.GetCurrentDirectory() + @"\ScpService.exe", null);

                        State.Clear();
                        Service.UseNewContext = true;

                        Service.Install(State);
                        Service.Commit(State);

                        if (Start(ScpService)) Logger(DifxLog.DIFXAPI_INFO, 0, ScpService + " Started.");
                        else Reboot = true;

                        Scp_Service_Configured = true;
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

            Cursor = Saved;

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

            Saved = Cursor;
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
                String DevPath = String.Empty, InstanceId = String.Empty;

                try
                {
                    UInt32 Result = 0;
                    Boolean RebootRequired = false;

                    if (cbService.Checked)
                    {
                        IDictionary State = new Hashtable();
                        AssemblyInstaller Service =
                            new AssemblyInstaller(Directory.GetCurrentDirectory() + @"\ScpService.exe", null);

                        State.Clear();
                        Service.UseNewContext = true;

                        if (Stop(ScpService))
                        {
                            Logger(DifxLog.DIFXAPI_INFO, 0, ScpService + " Stopped.");
                        }

                        Service.Uninstall(State);
                        Scp_Service_Configured = true;
                    }

                    if (cbBluetooth.Checked)
                    {
                        Result = Installer.Uninstall(InfPath + @"BthWinUsb.inf", DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                            out RebootRequired);
                        Reboot |= RebootRequired;
                        if (Result == 0) BTH_Driver_Configured = true;
                    }

                    if (cbDS3.Checked)
                    {
                        Result = Installer.Uninstall(InfPath + @"Ds3WinUsb.inf", DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                            out RebootRequired);
                        Reboot |= RebootRequired;
                        if (Result == 0) DS3_Driver_Configured = true;
                    }

                    if (cbBus.Checked && Devcon.Find(new Guid(DS3_BUS_CLASS_GUID), ref DevPath, ref InstanceId))
                    {
                        if (Devcon.Remove(new Guid(DS3_BUS_CLASS_GUID), DevPath, InstanceId))
                        {
                            Logger(DifxLog.DIFXAPI_SUCCESS, 0, "Virtual Bus Removed");
                            Bus_Device_Configured = true;

                            Installer.Uninstall(InfPath + @"ScpVBus.inf", DifxFlags.DRIVER_PACKAGE_DELETE_FILES,
                                out RebootRequired);
                            Reboot |= RebootRequired;
                        }
                        else
                        {
                            Logger(DifxLog.DIFXAPI_ERROR, 0, "Virtual Bus Removal Failure");
                        }
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

            Cursor = Saved;

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
