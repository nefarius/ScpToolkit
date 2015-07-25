using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using log4net;
using ScpControl;

namespace ScpService
{
    public partial class Ds3Service : ServiceBase
    {
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

            _mTimer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("Scarlet.Crush Productions DS3 Service Started");

            OnDebug(this,
                new DebugEventArgs(string.Format("++ {0} {1}", Assembly.GetExecutingAssembly().Location,
                    Assembly.GetExecutingAssembly().GetName().Version)));

            _mControlHandler = ServiceControlHandler;
            _mServiceHandle = ScpDevice.RegisterServiceCtrlHandlerEx(ServiceName, _mControlHandler, IntPtr.Zero);

            rootHub.Open();
            rootHub.Start();

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

        private int ServiceControlHandler(int Control, int Type, IntPtr Data, IntPtr Context)
        {
            switch (Control)
            {
                case ScpDevice.SERVICE_CONTROL_STOP:
                case ScpDevice.SERVICE_CONTROL_SHUTDOWN:

                    Stop();
                    break;

                case ScpDevice.SERVICE_CONTROL_POWEREVENT:

                    switch (Type)
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

                    switch (Type)
                    {
                        case ScpDevice.DBT_DEVICEARRIVAL:
                        case ScpDevice.DBT_DEVICEREMOVECOMPLETE:

                            ScpDevice.DEV_BROADCAST_HDR hdr;

                            hdr =
                                (ScpDevice.DEV_BROADCAST_HDR)
                                    Marshal.PtrToStructure(Data, typeof (ScpDevice.DEV_BROADCAST_HDR));

                            if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                            {
                                ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                                deviceInterface =
                                    (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M)
                                        Marshal.PtrToStructure(Data, typeof (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                                var Class = "{" + new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper() + "}";

                                var Path = new string(deviceInterface.dbcc_name);
                                Path = Path.Substring(0, Path.IndexOf('\0')).ToUpper();

                                var Pad = rootHub.Notify((ScpDevice.Notified) Type, Class, Path);

                                if (Pad != DsPadId.None)
                                {
                                    if (rootHub.Pairable && (rootHub.Master != rootHub.Pad[(byte) Pad].Remote))
                                    {
                                        var Master = new byte[6];
                                        var Parts = rootHub.Master.Split(new[] {":"},
                                            StringSplitOptions.RemoveEmptyEntries);

                                        for (var Part = 0; Part < Master.Length; Part++)
                                        {
                                            Master[Part] = byte.Parse(Parts[Part], NumberStyles.HexNumber);
                                        }

                                        rootHub.Pad[(byte) Pad].Pair(Master);

                                        Log.InfoFormat("Paired DS3 [{0}] To BTH Dongle [{1}]",
                                            rootHub.Pad[(byte) Pad].Local, rootHub.Master);
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }

            return 0; // NO_ERROR
        }

        private void OnTimer(object State)
        {
            lock (this)
            {
                rootHub.Resume();

                Log.Info("Scp DS3 Service Resumed");
            }
        }

        private void OnDebug(object sender, DebugEventArgs e)
        {
            Log.Debug(e.Data);
        }
    }
}