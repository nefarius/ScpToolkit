using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using log4net;
using ScpControl;
using ScpControl.Bluetooth;
using ScpControl.Driver;
using ScpControl.Exceptions;
using ScpControl.Utilities;
using ScpService.Properties;

namespace ScpService
{
    public partial class Ds3Service : ServiceBase
    {
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IntPtr _mBthNotify = IntPtr.Zero;
        private ScpDevice.ServiceControlHandlerEx _mControlHandler;
        private IntPtr _mDs3Notify = IntPtr.Zero;
        private IntPtr _mDs4Notify = IntPtr.Zero;
        private IntPtr _mServiceHandle = IntPtr.Zero;
        private readonly Timer _mTimer;

        public Ds3Service()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.FatalFormat("An unhandled exception occured: {0}", args.ExceptionObject);
            };

            _mTimer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif

            Log.Info("Scarlet.Crush Productions DS3 Service Started");

            Log.DebugFormat("++ {0} {1}", Assembly.GetExecutingAssembly().Location,
                    Assembly.GetExecutingAssembly().GetName().Version);

            Log.InfoFormat("Settings working directory to {0}", WorkingDirectory);
            Directory.SetCurrentDirectory(WorkingDirectory);

            _mControlHandler = ServiceControlHandler;
            _mServiceHandle = ScpDevice.RegisterServiceCtrlHandlerEx(ServiceName, _mControlHandler, IntPtr.Zero);

            // install compatible bluetooth dongles
            if(Settings.Default.InstallBluetoothDongles)
            {
                var bthDrivers = IniConfig.Instance.BthDongleDriver;

                foreach (var hardwareId in bthDrivers.HardwareIds)
                {
                    Log.DebugFormat("DeviceGUID = {0}", bthDrivers.DeviceGuid);
                    Log.DebugFormat("HardwareId = {0}", hardwareId);
                    var result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, bthDrivers.DeviceGuid, "Driver",
                        "BthDongle.inf",
                        IntPtr.Zero);
                    Log.DebugFormat("result = {0}", result);
                }
            }

            // install compatible DS3 controllers
            if(Settings.Default.InstallDualShock3Controllers)
            {
                var ds3Drivers = IniConfig.Instance.Ds3Driver;

                foreach (var hardwareId in ds3Drivers.HardwareIds)
                {
                    Log.DebugFormat("DeviceGUID = {0}", ds3Drivers.DeviceGuid);
                    Log.DebugFormat("HardwareId = {0}", hardwareId);
                    var result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, ds3Drivers.DeviceGuid, "Driver",
                        "Ds3Controller.inf", IntPtr.Zero);
                    Log.DebugFormat("result = {0}", result);
                }
            }

            // install compatible DS4 controllers
            if(Settings.Default.InstallDualShock4Controllers)
            {
                var ds4Drivers = IniConfig.Instance.Ds4Driver;

                foreach (var hardwareId in ds4Drivers.HardwareIds)
                {
                    Log.DebugFormat("DeviceGUID = {0}", ds4Drivers.DeviceGuid);
                    Log.DebugFormat("HardwareId = {0}", hardwareId);
                    var result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, ds4Drivers.DeviceGuid, "Driver",
                        "Ds4Controller.inf", IntPtr.Zero);
                    Log.DebugFormat("result = {0}", result);
                }
            }

            try
            {
                rootHub.Open();
                rootHub.Start();
            }
            catch (RootHubAlreadyStartedException rhex)
            {
                Log.FatalFormat("Couldn't start the ScpService: {0}", rhex.Message);
                Stop();
                return;
            }

            ScpDevice.RegisterNotify(_mServiceHandle, new Guid(UsbDs3.USB_CLASS_GUID), ref _mDs3Notify, false);
            ScpDevice.RegisterNotify(_mServiceHandle, new Guid(UsbDs4.USB_CLASS_GUID), ref _mDs4Notify, false);
            ScpDevice.RegisterNotify(_mServiceHandle, new Guid(BthDongle.BTH_CLASS_GUID), ref _mBthNotify, false);
        }

        protected override void OnStop()
        {
            if (_mDs3Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(_mDs3Notify);
            if (_mDs4Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(_mDs4Notify);
            if (_mBthNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(_mBthNotify);

            rootHub.Stop();
            rootHub.Close();

            Log.Info("Scarlet.Crush Productions DS3 Service Stopped");
        }

        private int ServiceControlHandler(int control, int type, IntPtr data, IntPtr context)
        {
            switch (control)
            {
                case ScpDevice.SERVICE_CONTROL_STOP:
                case ScpDevice.SERVICE_CONTROL_SHUTDOWN:

                    Stop();
                    break;

                case ScpDevice.SERVICE_CONTROL_POWEREVENT:

                    switch (type)
                    {
                        case ScpDevice.PBT_APMSUSPEND:

                            Log.Info("Scp DS3 Service Suspending");

                            rootHub.Suspend();
                            break;

                        case ScpDevice.PBT_APMRESUMEAUTOMATIC:

                            Log.Info("Scp DS3 Service Resuming");

                            _mTimer.Change(10000, Timeout.Infinite);
                            break;
                    }
                    break;

                case ScpDevice.SERVICE_CONTROL_DEVICEEVENT:

                    switch (type)
                    {
                        case ScpDevice.DBT_DEVICEARRIVAL:
                        case ScpDevice.DBT_DEVICEREMOVECOMPLETE:

                            ScpDevice.DEV_BROADCAST_HDR hdr;

                            hdr =
                                (ScpDevice.DEV_BROADCAST_HDR)
                                    Marshal.PtrToStructure(data, typeof (ScpDevice.DEV_BROADCAST_HDR));

                            if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                            {
                                ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                                deviceInterface =
                                    (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M)
                                        Marshal.PtrToStructure(data, typeof (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                                var Class = "{" + new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper() + "}";

                                var path = new string(deviceInterface.dbcc_name);
                                path = path.Substring(0, path.IndexOf('\0')).ToUpper();

                                var pad = rootHub.Notify((ScpDevice.Notified) type, Class, path);

                                if (pad != DsPadId.None)
                                {
                                    if (rootHub.Pairable && (rootHub.Master != rootHub.Pad[(byte) pad].Remote))
                                    {
                                        var master = new byte[6];
                                        var parts = rootHub.Master.Split(new[] {":"},
                                            StringSplitOptions.RemoveEmptyEntries);

                                        for (var Part = 0; Part < master.Length; Part++)
                                        {
                                            master[Part] = byte.Parse(parts[Part], NumberStyles.HexNumber);
                                        }

                                        rootHub.Pad[(byte) pad].Pair(master);

                                        Log.InfoFormat("Paired DS3 [{0}] To BTH Dongle [{1}]",
                                            rootHub.Pad[(byte) pad].Local, rootHub.Master);
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }

            return 0; // NO_ERROR
        }

        private void OnTimer(object state)
        {
            lock (this)
            {
                rootHub.Resume();

                Log.Info("Scp DS3 Service Resumed");
            }
        }
    }
}