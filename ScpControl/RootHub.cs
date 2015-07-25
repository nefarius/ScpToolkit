using System;
using System.ComponentModel;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Management;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ScpControl 
{
    public partial class RootHub : ScpHub 
    {
        protected class Cache 
        {
            protected Byte[] m_Report = new Byte[BusDevice.ReportSize];
            protected Byte[] m_Rumble = new Byte[BusDevice.RumbleSize];
            protected Byte[] m_Mapped = new Byte[ReportEventArgs.Length];

            public Byte[] Report 
            {
                get { return m_Report; }
            }

            public Byte[] Rumble 
            {
                get { return m_Rumble; }
            }

            public Byte[] Mapped 
            {
                get { return m_Mapped; }
            }
        }
        protected Cache[] m_Cache = { new Cache(), new Cache(), new Cache(), new Cache() };

        protected volatile Boolean m_Suspended = false;

        protected BthHub    bthHub = new BthHub();
        protected UsbHub    usbHub = new UsbHub();
        protected BusDevice scpBus = new BusDevice();

        protected Byte[][] m_XInput = { new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }};
        protected Byte[][] m_Native = { new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }, new Byte[2] { 0, 0 }};

        protected IDsDevice[] m_Pad = { new DsNull(DsPadId.One), new DsNull(DsPadId.Two), new DsNull(DsPadId.Three), new DsNull(DsPadId.Four) };
        protected String[] m_Reserved = new String[] { String.Empty, String.Empty, String.Empty, String.Empty };

        protected IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        protected UdpClient  m_Server   = new UdpClient();

        protected IPEndPoint m_ClientEp = new IPEndPoint(IPAddress.Loopback, 26761);
        protected UdpClient  m_Client   = new UdpClient();

        public IDsDevice[] Pad 
        {
            get { return m_Pad; }
        }

        public String  Dongle   
        {
            get { return bthHub.Dongle; }
        }
        public String  Master   
        {
            get { return bthHub.Master; }
        }
        public Boolean Pairable 
        {
            get { return m_Started && bthHub.Pairable; }
        }


        public RootHub() 
        {
            InitializeComponent();

            scpMap.Debug += new EventHandler<DebugEventArgs>(On_Debug);

            bthHub.Debug += new EventHandler<DebugEventArgs>(On_Debug);
            usbHub.Debug += new EventHandler<DebugEventArgs>(On_Debug);
            scpBus.Debug += new EventHandler<DebugEventArgs>(On_Debug);

            bthHub.Arrival += new EventHandler<ArrivalEventArgs>(On_Arrival);
            usbHub.Arrival += new EventHandler<ArrivalEventArgs>(On_Arrival);

            bthHub.Report += new EventHandler<ReportEventArgs>(On_Report);
            usbHub.Report += new EventHandler<ReportEventArgs>(On_Report);
        }

        public RootHub(IContainer container) 
        {
            container.Add(this);
            InitializeComponent();

            scpMap.Debug += new EventHandler<DebugEventArgs>(On_Debug);

            bthHub.Debug += new EventHandler<DebugEventArgs>(On_Debug);
            usbHub.Debug += new EventHandler<DebugEventArgs>(On_Debug);
            scpBus.Debug += new EventHandler<DebugEventArgs>(On_Debug);

            bthHub.Arrival += new EventHandler<ArrivalEventArgs>(On_Arrival);
            usbHub.Arrival += new EventHandler<ArrivalEventArgs>(On_Arrival);

            bthHub.Report += new EventHandler<ReportEventArgs>(On_Report);
            usbHub.Report += new EventHandler<ReportEventArgs>(On_Report);
        }


        public override Boolean Open()  
        {
            bool Opened = false;

            LogDebug(String.Format("++ {0} {1}", Assembly.GetExecutingAssembly().Location, Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            LogDebug(String.Format("++ {0}", OSInfo()));

            scpMap.Open();

            Opened |= scpBus.Open(Global.Bus);
            Opened |= usbHub.Open();
            Opened |= bthHub.Open();

            Global.Load();
            return Opened;
        }

        public override Boolean Start() 
        {
            if (!m_Started)
            {
                scpMap.Start();

                m_Started |= scpBus.Start();
                m_Started |= usbHub.Start();
                m_Started |= bthHub.Start();

                if (m_Started) UDP_Worker.RunWorkerAsync();
            }

            return m_Started;
        }

        public override Boolean Stop()  
        {
            if (m_Started)
            {
                m_Started = false;
                m_Server.Close();

                scpMap.Stop();

                scpBus.Stop();
                usbHub.Stop();
                bthHub.Stop();
            }

            return !m_Started;
        }

        public override Boolean Close() 
        {
            if (m_Started)
            {
                m_Started = false;
                m_Server.Close();

                scpMap.Close();

                scpBus.Close();
                usbHub.Close();
                bthHub.Close();
            }

            Global.Save();

            return !m_Started;
        }


        public override Boolean Suspend() 
        {
            m_Suspended = true;

            for (Int32 Index = 0; Index < m_Pad.Length; Index++) m_Pad[Index].Disconnect();

            scpBus.Suspend();
            usbHub.Suspend();
            bthHub.Suspend();

            LogDebug("++ Suspended");
            return true;
        }

        public override Boolean Resume()  
        {
            LogDebug("++ Resumed");

            scpBus.Resume();
            for (Int32 Index = 0; Index < m_Pad.Length; Index++)
            {
                if (m_Pad[Index].State != DsState.Disconnected)
                {
                    scpBus.Plugin(Index + 1);
                }
            }

            usbHub.Resume();
            bthHub.Resume();

            m_Suspended = false;
            return true;
        }


        public override DsPadId Notify(ScpDevice.Notified Notification, String Class, String Path) 
        {
            if (!m_Suspended)
            {
                if (Class == UsbDs4.USB_CLASS_GUID)
                {
                    return usbHub.Notify(Notification, Class, Path);
                }

                if (Class == UsbDs3.USB_CLASS_GUID)
                {
                    return usbHub.Notify(Notification, Class, Path);
                }

                if (Class == BthDongle.BTH_CLASS_GUID)
                {
                    bthHub.Notify(Notification, Class, Path);
                }
            }

            return DsPadId.None;
        }

        protected virtual void UDP_Worker_Thread(object sender, DoWorkEventArgs e)  
        {
            Byte Serial;
            StringBuilder sb = new StringBuilder();

            Thread.Sleep(1);

            IPEndPoint Remote = new IPEndPoint(IPAddress.Loopback, 0);

            m_Server = new UdpClient(m_ServerEp);

            LogDebug("-- Controller : UDP_Worker_Thread Starting");

            while (m_Started)
            {
                try
                {
                    Byte[] Buffer = m_Server.Receive(ref Remote);

                    switch (Buffer[1])
                    {
                        case 0x00:      // Status Request

                            if (!Global.DisableNative)
                            {
                                Buffer[2] = (Byte)Pad[0].State;
                                Buffer[3] = (Byte)Pad[1].State;
                                Buffer[4] = (Byte)Pad[2].State;
                                Buffer[5] = (Byte)Pad[3].State;
                            }
                            else
                            {
                                Buffer[2] = 0;
                                Buffer[3] = 0;
                                Buffer[4] = 0;
                                Buffer[5] = 0;
                            }

                            m_Server.Send(Buffer, Buffer.Length, Remote);
                            break;

                        case 0x01:      // Rumble Request

                            Serial = Buffer[0];

                            if (Pad[Serial].State == DsState.Connected)
                            {
                                if (Buffer[2] != m_Native[Serial][0] || Buffer[3] != m_Native[Serial][1])
                                {
                                    m_Native[Serial][0] = Buffer[2];
                                    m_Native[Serial][1] = Buffer[3];

                                    Pad[Buffer[0]].Rumble(Buffer[2], Buffer[3]);
                                }
                            }
                            break;

                        case 0x02:      // Status Data Request
                            {
                                sb.Clear();
                                sb.Append(Dongle); sb.Append('^');

                                sb.Append(Pad[0].ToString()); sb.Append('^');
                                sb.Append(Pad[1].ToString()); sb.Append('^');
                                sb.Append(Pad[2].ToString()); sb.Append('^');
                                sb.Append(Pad[3].ToString()); sb.Append('^');

                                Byte[] Data = Encoding.Unicode.GetBytes(sb.ToString());

                                m_Server.Send(Data, Data.Length, Remote);
                            }
                            break;

                        case 0x03:      // Config Read Request
                            {
                                Byte[] Data = Global.Packed;

                                m_Server.Send(Data, Data.Length, Remote);
                            }
                            break;

                        case 0x04:      // Config Write Request
                            {
                                Global.Packed = Buffer;
                            }
                            break;

                        case 0x05:      // Pad Promote Request
                            {
                                Int32 Target = Buffer[2];

                                lock (this)
                                {
                                    if (Pad[Target].State != DsState.Disconnected)
                                    {
                                        IDsDevice Swap = Pad[Target];
                                        Pad[Target] = Pad[Target - 1];
                                        Pad[Target - 1] = Swap;

                                        Pad[Target].PadId = (DsPadId)(Target);
                                        Pad[Target - 1].PadId = (DsPadId)(Target - 1);

                                        m_Reserved[Target] = Pad[Target].Local;
                                        m_Reserved[Target - 1] = Pad[Target - 1].Local;
                                    }
                                }
                            }
                            break;

                        case 0x06:      // Profile List
                            {
                                sb.Clear();
                                sb.Append(scpMap.Active); sb.Append('^');

                                foreach (String Profile in scpMap.Profiles)
                                {
                                    sb.Append(Profile); sb.Append('^');
                                }

                                Byte[] Data = Encoding.Unicode.GetBytes(sb.ToString());

                                m_Server.Send(Data, Data.Length, Remote);
                            }
                            break;

                        case 0x07:      // Set Active Profile
                            {
                                Byte[] Data = new Byte[Buffer.Length - 2];

                                Array.Copy(Buffer, 2, Data, 0, Data.Length);

                                scpMap.Active = Encoding.Unicode.GetString(Data);
                            }
                            break;

                        case 0x08:      // Get XML
                            {
                                Byte[] Data = Encoding.UTF8.GetBytes(scpMap.Xml);

                                m_Server.Send(Data, Data.Length, Remote);
                            }
                            break;

                        case 0x09:      // Set XML
                            {
                                Byte[] Data = new Byte[Buffer.Length - 2];

                                Array.Copy(Buffer, 2, Data, 0, Data.Length);

                                scpMap.Xml = Encoding.UTF8.GetString(Data);
                            }
                            break;

                        case 0x0A:      // Pad Detail
                            {

                                Serial = Buffer[0];

                                Byte[] Data = new Byte[11];
                                Byte[] Temp = Encoding.Unicode.GetBytes(m_Pad[Serial].Local);

                                Data[0] = Serial;
                                Data[1] = (Byte) m_Pad[Serial].State;
                                Data[2] = (Byte) m_Pad[Serial].Model;
                                Data[3] = (Byte) m_Pad[Serial].Connection;
                                Data[4] = (Byte) m_Pad[Serial].Battery;
                                Array.Copy(m_Pad[Serial].BD_Address, 0, Data, 5, m_Pad[Serial].BD_Address.Length);

                                m_Server.Send(Data, Data.Length, Remote);
                            }
                            break;
                    }
                }
                catch { }
           }

            LogDebug("-- Controller : UDP_Worker_Thread Exiting");
        }


        protected override void On_Arrival(object sender, ArrivalEventArgs e) 
        {
            lock (this)
            {
                Boolean bFound = false;
                IDsDevice Arrived = e.Device;

                for (Int32 Index = 0; Index < m_Pad.Length && !bFound; Index++)
                {
                    if (Arrived.Local == m_Reserved[Index])
                    {
                        if (m_Pad[Index].State == DsState.Connected)
                        {
                            if (m_Pad[Index].Connection == DsConnection.BTH)
                            {
                                m_Pad[Index].Disconnect();
                            }

                            if (m_Pad[Index].Connection == DsConnection.USB)
                            {
                                Arrived.Disconnect();

                                e.Handled = false;
                                return;
                            }
                        }

                        bFound = true;

                        Arrived.PadId = (DsPadId) Index;
                        m_Pad[Index] = Arrived;
                    }
                }

                for (Int32 Index = 0; Index < m_Pad.Length && !bFound; Index++)
                {
                    if (m_Pad[Index].State == DsState.Disconnected)
                    {
                        bFound = true;
                        m_Reserved[Index] = Arrived.Local;

                        Arrived.PadId = (DsPadId) Index;
                        m_Pad[Index] = Arrived;
                    }
                }

                if (bFound)
                {
                    scpBus.Plugin((int) Arrived.PadId + 1);

                    LogDebug(String.Format("++ Plugin Port #{0} for [{1}]", (int) Arrived.PadId + 1, Arrived.Local));
                }
                e.Handled = bFound;
            }
        }

        protected override void On_Report(object sender, ReportEventArgs e)   
        {
            Int32   Serial = e.Report[(Int32) DsOffset.Pad];
            DsModel Model  = (DsModel) e.Report[(Int32) DsOffset.Model];

            Byte[] Report = m_Cache[Serial].Report;
            Byte[] Rumble = m_Cache[Serial].Rumble;
            Byte[] Mapped = m_Cache[Serial].Mapped;
            
            if (scpMap.Remap(Model, Serial, m_Pad[Serial].Local, e.Report, Mapped))
            {
                scpBus.Parse(Mapped, Report, Model);
            }
            else
            {
                scpBus.Parse(e.Report, Report, Model);
            }

            if (scpBus.Report(Report, Rumble) && (DsState) e.Report[1] == DsState.Connected)
            {
                Byte Large = (Byte)(Rumble[3]);
                Byte Small = (Byte)(Rumble[4]);

                if (Rumble[1] == 0x08 && (Large != m_XInput[Serial][0] || Small != m_XInput[Serial][1]))
                {
                    m_XInput[Serial][0] = Large;
                    m_XInput[Serial][1] = Small;

                    Pad[Serial].Rumble(Large, Small);
                }
            }

            if ((DsState) e.Report[1] != DsState.Connected)
            {
                m_XInput[Serial][0] = m_XInput[Serial][1] = 0;
                m_Native[Serial][0] = m_Native[Serial][1] = 0;
            }

            if (!Global.DisableNative) m_Client.Send(e.Report, e.Report.Length, m_ClientEp);
        }


        protected String OSInfo() 
        {
            String Info = String.Empty;

            try
            {
                using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
                {
                    foreach (ManagementObject mo in mos.Get())
                    {
                        try
                        {
                            Info = Regex.Replace(mo.GetPropertyValue("Caption").ToString(), "[^A-Za-z0-9 ]", "").Trim();

                            try
                            {
                                Object spv = mo.GetPropertyValue("ServicePackMajorVersion");

                                if (spv != null && spv.ToString() != "0")
                                {
                                    Info += " Service Pack " + spv.ToString();
                                }
                            }
                            catch { }

                            Info = String.Format("{0} ({1} {2})", Info, System.Environment.OSVersion.Version.ToString(), System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));

                        }
                        catch { }

                        mo.Dispose();
                    }
                }
            }
            catch { }

            return Info;
        }
    }
}
