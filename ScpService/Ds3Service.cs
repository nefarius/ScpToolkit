using System;
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

using ScpControl;

namespace ScpService 
{
    public partial class Ds3Service : ServiceBase 
    {
        protected ScpDevice.ServiceControlHandlerEx m_ControlHandler;

        protected String m_Log           = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".log";
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
            EventLog.WriteEntry("Scarlet.Crush Productions DS3 Service Started", System.Diagnostics.EventLogEntryType.Information, 1);

            try { if (File.Exists(m_Log)) File.Delete(m_Log); }
            catch { }

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

            EventLog.WriteEntry("Scarlet.Crush Productions DS3 Service Stopped", System.Diagnostics.EventLogEntryType.Information, 2);
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

                            EventLog.WriteEntry("Scp DS3 Service Suspending", System.Diagnostics.EventLogEntryType.Information, 3);

                            rootHub.Suspend();
                            break;

                        case ScpDevice.PBT_APMRESUMEAUTOMATIC:

                            EventLog.WriteEntry("Scp DS3 Service Resuming", System.Diagnostics.EventLogEntryType.Information, 4);

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

                                        EventLog.WriteEntry(String.Format("Paired DS3 [{0}] To BTH Dongle [{1}]", rootHub.Pad[(Byte) Pad].Local, rootHub.Master), System.Diagnostics.EventLogEntryType.Information, 6);
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }

            return 0;   // NO_ERROR
        }

        protected void OnTimer(object State) 
        {
            lock (this)
            {
                rootHub.Resume();

                EventLog.WriteEntry("Scp DS3 Service Resumed", System.Diagnostics.EventLogEntryType.Information, 5);
            }
        }

        protected void OnDebug(object sender, DebugEventArgs e) 
        {
            lock (rootHub)
            {
                try
                {
                    using (StreamWriter fs = new StreamWriter(m_Log, true))
                    {
                        fs.Write(String.Format("{0} {1}\r\n", e.Time.ToString("yyyy-MM-dd HH:mm:ss.fff"), e.Data));
                        fs.Flush();
                   }
                }
                catch { }
            }
        }
    }
}
