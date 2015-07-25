using System;
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using log4net;
using ScpControl;

namespace ScpService 
{
    public partial class Ds3Service : ServiceBase 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ScpDevice.ServiceControlHandlerEx m_ControlHandler;

        protected IntPtr m_ServiceHandle = IntPtr.Zero;
        protected IntPtr m_Ds3Notify     = IntPtr.Zero;
        protected IntPtr m_Ds4Notify     = IntPtr.Zero;
        protected IntPtr m_BthNotify     = IntPtr.Zero;
        protected Timer  m_Timer;

        public Ds3Service() 
        {
            InitializeComponent();

            m_Timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(String[] args) 
        {
            Log.Info("Scarlet.Crush Productions DS3 Service Started");

            OnDebug(this, new DebugEventArgs(String.Format("++ {0} {1}", Assembly.GetExecutingAssembly().Location, Assembly.GetExecutingAssembly().GetName().Version.ToString())));

            m_ControlHandler = new ScpDevice.ServiceControlHandlerEx(ServiceControlHandler);
            m_ServiceHandle = ScpDevice.RegisterServiceCtrlHandlerEx(ServiceName, m_ControlHandler, IntPtr.Zero);

            rootHub.Open();
            rootHub.Start();

            ScpDevice.RegisterNotify(m_ServiceHandle, new Guid(UsbDs3.USB_CLASS_GUID),    ref m_Ds3Notify, false);
            ScpDevice.RegisterNotify(m_ServiceHandle, new Guid(UsbDs4.USB_CLASS_GUID),    ref m_Ds4Notify, false);
            ScpDevice.RegisterNotify(m_ServiceHandle, new Guid(BthDongle.BTH_CLASS_GUID), ref m_BthNotify, false);
        }

        protected override void OnStop() 
        {
            if (m_Ds3Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Ds3Notify);
            if (m_Ds4Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Ds4Notify);
            if (m_BthNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_BthNotify);

            rootHub.Stop();
            rootHub.Close();

            Log.Info("Scarlet.Crush Productions DS3 Service Stopped");
        }

        protected Int32 ServiceControlHandler(Int32 Control, Int32 Type, IntPtr Data, IntPtr Context) 
        {
            switch (Control)
            {
                case ScpDevice.SERVICE_CONTROL_STOP:
                case ScpDevice.SERVICE_CONTROL_SHUTDOWN:

                    base.Stop();
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

                            m_Timer.Change(10000, Timeout.Infinite);
                            break;
                    }
                    break;

                case ScpDevice.SERVICE_CONTROL_DEVICEEVENT:

                    switch (Type)
                    {
                        case ScpDevice.DBT_DEVICEARRIVAL:
                        case ScpDevice.DBT_DEVICEREMOVECOMPLETE:

                            ScpDevice.DEV_BROADCAST_HDR hdr;

                            hdr = (ScpDevice.DEV_BROADCAST_HDR) Marshal.PtrToStructure(Data, typeof(ScpDevice.DEV_BROADCAST_HDR));

                            if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                            {
                                ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                                deviceInterface = (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M) Marshal.PtrToStructure(Data, typeof(ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                                String Class = "{" + new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper() + "}";

                                String Path = new String(deviceInterface.dbcc_name);
                                Path = Path.Substring(0, Path.IndexOf('\0')).ToUpper();

                                DsPadId Pad = rootHub.Notify((ScpDevice.Notified) Type, Class, Path);

                                if (Pad != DsPadId.None)
                                {
                                    if (rootHub.Pairable && (rootHub.Master != rootHub.Pad[(Byte) Pad].Remote))
                                    {
                                        Byte[]   Master = new Byte[6];
                                        String[] Parts  = rootHub.Master.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                                        for (Int32 Part = 0; Part < Master.Length; Part++)
                                        {
                                            Master[Part] = Byte.Parse(Parts[Part], System.Globalization.NumberStyles.HexNumber);
                                        }

                                        rootHub.Pad[(Byte) Pad].Pair(Master);

                                        Log.InfoFormat("Paired DS3 [{0}] To BTH Dongle [{1}]", rootHub.Pad[(Byte) Pad].Local, rootHub.Master);
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }

            return 0;   // NO_ERROR
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
