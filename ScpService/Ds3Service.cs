using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using log4net;
using ScpControl;
using ScpControl.Bluetooth;
using ScpControl.Exceptions;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;
using ScpControl.Usb.Gamepads;

namespace ScpService
{
    public partial class Ds3Service : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IntPtr _bthNotify = IntPtr.Zero;
        private ScpDevice.ServiceControlHandlerEx _mControlHandler;
        private IntPtr _ds3Notify = IntPtr.Zero;
        private IntPtr _ds4Notify = IntPtr.Zero;
        private IntPtr _mServiceHandle = IntPtr.Zero;
        private IntPtr _genericNotify = IntPtr.Zero;
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
            Log.Info("Scarlet.Crush Productions DSx Service Started");

            Log.DebugFormat("++ {0} {1}", Assembly.GetExecutingAssembly().Location,
                    Assembly.GetExecutingAssembly().GetName().Version);

            Log.InfoFormat("Setting working directory to {0}", GlobalConfiguration.AppDirectory);
            Directory.SetCurrentDirectory(GlobalConfiguration.AppDirectory);

            _mControlHandler = ServiceControlHandler;
            _mServiceHandle = ScpDevice.RegisterServiceCtrlHandlerEx(ServiceName, _mControlHandler, IntPtr.Zero);

#if FIXME // TODO: this is currently broken, replace

            var installTask = Task.Factory.StartNew(() =>
            {
                if (GlobalConfiguration.Instance.ForceBluetoothDriverReinstallation)
                    DriverInstaller.InstallBluetoothDongles(
                        ScpDeviceCollection.Instance.Devices.Where(d => d.DeviceType == WdiUsbDeviceType.BluetoothHost));

                if (GlobalConfiguration.Instance.ForceDs3DriverReinstallation)
                    DriverInstaller.InstallDualShock3Controllers(
                        ScpDeviceCollection.Instance.Devices.Where(d => d.DeviceType == WdiUsbDeviceType.DualShock3));

                if (GlobalConfiguration.Instance.ForceDs4DriverReinstallation)
                    DriverInstaller.InstallDualShock4Controllers(
                        ScpDeviceCollection.Instance.Devices.Where(d => d.DeviceType == WdiUsbDeviceType.DualSHock4));
            });

            installTask.ContinueWith(task =>
            {
                Log.FatalFormat("Error during driver installation: {0}", task.Exception);
                Stop();
            }, TaskContinuationOptions.OnlyOnFaulted);

#endif

            try
            {
                rootHub.Open();
                rootHub.Start();
            }
            catch (RootHubAlreadyStartedException rhex)
            {
                Log.FatalFormat("Couldn't start the root hub: {0}", rhex.Message);
                Stop();
                return;
            }

            ScpDevice.RegisterNotify(_mServiceHandle, UsbDs3.DeviceClassGuid, ref _ds3Notify, false);
            ScpDevice.RegisterNotify(_mServiceHandle, UsbDs4.DeviceClassGuid, ref _ds4Notify, false);
            ScpDevice.RegisterNotify(_mServiceHandle, BthDongle.DeviceClassGuid, ref _bthNotify, false);
            ScpDevice.RegisterNotify(_mServiceHandle, UsbGenericGamepad.DeviceClassGuid, ref _genericNotify, false);
        }

        protected override void OnStop()
        {
            if (_ds3Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(_ds3Notify);
            if (_ds4Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(_ds4Notify);
            if (_bthNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(_bthNotify);
            if (_genericNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(_genericNotify);

            rootHub.Stop();
            rootHub.Close();

            Log.Info("Scarlet.Crush Productions DSx Service Stopped");
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
                                    Marshal.PtrToStructure(data, typeof(ScpDevice.DEV_BROADCAST_HDR));

                            if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                            {
                                ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                                deviceInterface =
                                    (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M)
                                        Marshal.PtrToStructure(data, typeof(ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                                var Class = "{" + new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper() + "}";

                                var path = new string(deviceInterface.dbcc_name);
                                path = path.Substring(0, path.IndexOf('\0')).ToUpper();

                                var pad = rootHub.Notify((ScpDevice.Notified)type, Class, path);

                                if (pad != DsPadId.None)
                                {
                                    if (rootHub.Pairable && !rootHub.BluetoothHostAddress.Equals(rootHub.Pads[(byte)pad].HostAddress))
                                    {
                                        if(rootHub.Pads[(byte)pad].Pair(rootHub.BluetoothHostAddress))
                                        {
                                            Log.InfoFormat("Paired DualShock Device {0} to Bluetooth host {1}",
                                                rootHub.Pads[(byte) pad].DeviceAddress, rootHub.BluetoothHostAddress);
                                        }
                                        else
                                        {
                                            Log.ErrorFormat("Couldn't pair DualShock Device {0} to Bluetooth host {1}",
                                                rootHub.Pads[(byte) pad].DeviceAddress, rootHub.BluetoothHostAddress);
                                        }
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
