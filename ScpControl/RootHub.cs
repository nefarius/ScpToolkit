using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using ScpControl.Utilities;

namespace ScpControl
{
    public partial class RootHub : ScpHub
    {
        protected BthHub bthHub = new BthHub();
        protected Cache[] m_Cache = {new Cache(), new Cache(), new Cache(), new Cache()};
        protected UdpClient m_Client = new UdpClient();
        protected IPEndPoint m_ClientEp = new IPEndPoint(IPAddress.Loopback, 26761);
        protected byte[][] m_Native = {new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0}};

        protected IDsDevice[] m_Pad =
        {
            new DsNull(DsPadId.One), new DsNull(DsPadId.Two), new DsNull(DsPadId.Three),
            new DsNull(DsPadId.Four)
        };

        protected string[] m_Reserved = {string.Empty, string.Empty, string.Empty, string.Empty};
        protected UdpClient m_Server = new UdpClient();
        protected IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        protected volatile bool m_Suspended;
        protected byte[][] m_XInput = {new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0}};
        protected BusDevice scpBus = new BusDevice();
        protected UsbHub usbHub = new UsbHub();

        public RootHub()
        {
            InitializeComponent();

            bthHub.Arrival += On_Arrival;
            usbHub.Arrival += On_Arrival;

            bthHub.Report += On_Report;
            usbHub.Report += On_Report;
        }

        public RootHub(IContainer container)
        {
            container.Add(this);
            InitializeComponent();

            bthHub.Arrival += On_Arrival;
            usbHub.Arrival += On_Arrival;

            bthHub.Report += On_Report;
            usbHub.Report += On_Report;
        }

        public IDsDevice[] Pad
        {
            get { return m_Pad; }
        }

        public string Dongle
        {
            get { return bthHub.Dongle; }
        }

        public string Master
        {
            get { return bthHub.Master; }
        }

        public bool Pairable
        {
            get { return m_Started && bthHub.Pairable; }
        }

        public override bool Open()
        {
            var opened = false;

            Log.DebugFormat("++ {0} {1}", Assembly.GetExecutingAssembly().Location,
                Assembly.GetExecutingAssembly().GetName().Version);
            Log.DebugFormat("++ {0}", OsInfoHelper.OsInfo());

            scpMap.Open();

            opened |= scpBus.Open(Global.Bus);
            opened |= usbHub.Open();
            opened |= bthHub.Open();

            Global.Load();
            return opened;
        }

        public override bool Start()
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

        public override bool Stop()
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

        public override bool Close()
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

        public override bool Suspend()
        {
            m_Suspended = true;

            for (var index = 0; index < m_Pad.Length; index++) m_Pad[index].Disconnect();

            scpBus.Suspend();
            usbHub.Suspend();
            bthHub.Suspend();

            Log.Debug("++ Suspended");
            return true;
        }

        public override bool Resume()
        {
            Log.Debug("++ Resumed");

            scpBus.Resume();
            for (var index = 0; index < m_Pad.Length; index++)
            {
                if (m_Pad[index].State != DsState.Disconnected)
                {
                    scpBus.Plugin(index + 1);
                }
            }

            usbHub.Resume();
            bthHub.Resume();

            m_Suspended = false;
            return true;
        }

        public override DsPadId Notify(ScpDevice.Notified Notification, string Class, string Path)
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
            var sb = new StringBuilder();

            Thread.Sleep(1);

            var remote = new IPEndPoint(IPAddress.Loopback, 0);

            m_Server = new UdpClient(m_ServerEp);

            Log.Debug("-- Controller : UDP_Worker_Thread Starting");

            while (m_Started)
            {
                try
                {
                    var buffer = m_Server.Receive(ref remote);

                    byte serial;
                    switch (buffer[1])
                    {
                        case 0x00: // Status Request

                            if (!Global.DisableNative)
                            {
                                buffer[2] = (byte) Pad[0].State;
                                buffer[3] = (byte) Pad[1].State;
                                buffer[4] = (byte) Pad[2].State;
                                buffer[5] = (byte) Pad[3].State;
                            }
                            else
                            {
                                buffer[2] = 0;
                                buffer[3] = 0;
                                buffer[4] = 0;
                                buffer[5] = 0;
                            }

                            m_Server.Send(buffer, buffer.Length, remote);
                            break;

                        case 0x01: // Rumble Request

                            serial = buffer[0];

                            if (Pad[serial].State == DsState.Connected)
                            {
                                if (buffer[2] != m_Native[serial][0] || buffer[3] != m_Native[serial][1])
                                {
                                    m_Native[serial][0] = buffer[2];
                                    m_Native[serial][1] = buffer[3];

                                    Pad[buffer[0]].Rumble(buffer[2], buffer[3]);
                                }
                            }
                            break;

                        case 0x02: // Status Data Request
                        {
                            sb.Clear();
                            sb.Append(Dongle);
                            sb.Append('^');

                            sb.Append(Pad[0]);
                            sb.Append('^');
                            sb.Append(Pad[1]);
                            sb.Append('^');
                            sb.Append(Pad[2]);
                            sb.Append('^');
                            sb.Append(Pad[3]);
                            sb.Append('^');

                            var data = Encoding.Unicode.GetBytes(sb.ToString());

                            m_Server.Send(data, data.Length, remote);
                        }
                            break;

                        case 0x03: // Config Read Request
                        {
                            var Data = Global.Packed;

                            m_Server.Send(Data, Data.Length, remote);
                        }
                            break;

                        case 0x04: // Config Write Request
                        {
                            Global.Packed = buffer;
                        }
                            break;

                        case 0x05: // Pad Promote Request
                        {
                            int target = buffer[2];

                            lock (this)
                            {
                                if (Pad[target].State != DsState.Disconnected)
                                {
                                    var swap = Pad[target];
                                    Pad[target] = Pad[target - 1];
                                    Pad[target - 1] = swap;

                                    Pad[target].PadId = (DsPadId) (target);
                                    Pad[target - 1].PadId = (DsPadId) (target - 1);

                                    m_Reserved[target] = Pad[target].Local;
                                    m_Reserved[target - 1] = Pad[target - 1].Local;
                                }
                            }
                        }
                            break;

                        case 0x06: // Profile List
                        {
                            sb.Clear();
                            sb.Append(scpMap.Active);
                            sb.Append('^');

                            foreach (var profile in scpMap.Profiles)
                            {
                                sb.Append(profile);
                                sb.Append('^');
                            }

                            var data = Encoding.Unicode.GetBytes(sb.ToString());

                            m_Server.Send(data, data.Length, remote);
                        }
                            break;

                        case 0x07: // Set Active Profile
                        {
                            var data = new byte[buffer.Length - 2];

                            Array.Copy(buffer, 2, data, 0, data.Length);

                            scpMap.Active = Encoding.Unicode.GetString(data);
                        }
                            break;

                        case 0x08: // Get XML
                        {
                            var data = Encoding.UTF8.GetBytes(scpMap.Xml);

                            m_Server.Send(data, data.Length, remote);
                        }
                            break;

                        case 0x09: // Set XML
                        {
                            var data = new byte[buffer.Length - 2];

                            Array.Copy(buffer, 2, data, 0, data.Length);

                            scpMap.Xml = Encoding.UTF8.GetString(data);
                        }
                            break;

                        case 0x0A: // Pad Detail
                        {
                            serial = buffer[0];

                            var data = new byte[11];
                            // TODO: investigate
                            var temp = Encoding.Unicode.GetBytes(m_Pad[serial].Local);
                            Log.DebugFormat("temp = {0}", temp);

                            data[0] = serial;
                            data[1] = (byte) m_Pad[serial].State;
                            data[2] = (byte) m_Pad[serial].Model;
                            data[3] = (byte) m_Pad[serial].Connection;
                            data[4] = (byte) m_Pad[serial].Battery;
                            Array.Copy(m_Pad[serial].BD_Address, 0, data, 5, m_Pad[serial].BD_Address.Length);

                            m_Server.Send(data, data.Length, remote);
                        }
                            break;
                    }
                }
                catch (SocketException sex)
                {
                    if (sex.NativeErrorCode == 10004)
                        break;

                    Log.ErrorFormat("Socket exception: {0}", sex);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }
            }

            Log.Debug("-- Controller : UDP_Worker_Thread Exiting");
        }

        protected override void On_Arrival(object sender, ArrivalEventArgs e)
        {
            lock (this)
            {
                var bFound = false;
                var arrived = e.Device;

                for (var index = 0; index < m_Pad.Length && !bFound; index++)
                {
                    if (arrived.Local == m_Reserved[index])
                    {
                        if (m_Pad[index].State == DsState.Connected)
                        {
                            if (m_Pad[index].Connection == DsConnection.BTH)
                            {
                                m_Pad[index].Disconnect();
                            }

                            if (m_Pad[index].Connection == DsConnection.USB)
                            {
                                arrived.Disconnect();

                                e.Handled = false;
                                return;
                            }
                        }

                        bFound = true;

                        arrived.PadId = (DsPadId) index;
                        m_Pad[index] = arrived;
                    }
                }

                for (var index = 0; index < m_Pad.Length && !bFound; index++)
                {
                    if (m_Pad[index].State == DsState.Disconnected)
                    {
                        bFound = true;
                        m_Reserved[index] = arrived.Local;

                        arrived.PadId = (DsPadId) index;
                        m_Pad[index] = arrived;
                    }
                }

                if (bFound)
                {
                    scpBus.Plugin((int) arrived.PadId + 1);

                    Log.DebugFormat("++ Plugin Port #{0} for [{1}]", (int) arrived.PadId + 1, arrived.Local);
                }
                e.Handled = bFound;
            }
        }

        protected override void On_Report(object sender, ReportEventArgs e)
        {
            int serial = e.Report[(int) DsOffset.Pad];
            var model = (DsModel) e.Report[(int) DsOffset.Model];

            var report = m_Cache[serial].Report;
            var rumble = m_Cache[serial].Rumble;
            var mapped = m_Cache[serial].Mapped;

            if (scpMap.Remap(model, serial, m_Pad[serial].Local, e.Report, mapped))
            {
                scpBus.Parse(mapped, report, model);
            }
            else
            {
                scpBus.Parse(e.Report, report, model);
            }

            if (scpBus.Report(report, rumble) && (DsState) e.Report[1] == DsState.Connected)
            {
                var Large = rumble[3];
                var Small = rumble[4];

                if (rumble[1] == 0x08 && (Large != m_XInput[serial][0] || Small != m_XInput[serial][1]))
                {
                    m_XInput[serial][0] = Large;
                    m_XInput[serial][1] = Small;

                    Pad[serial].Rumble(Large, Small);
                }
            }

            if ((DsState) e.Report[1] != DsState.Connected)
            {
                m_XInput[serial][0] = m_XInput[serial][1] = 0;
                m_Native[serial][0] = m_Native[serial][1] = 0;
            }

            if (!Global.DisableNative) m_Client.Send(e.Report, e.Report.Length, m_ClientEp);
        }

        protected class Cache
        {
            private byte[] m_Mapped = new byte[ReportEventArgs.Length];
            private byte[] m_Report = new byte[BusDevice.ReportSize];
            private byte[] m_Rumble = new byte[BusDevice.RumbleSize];

            public byte[] Report
            {
                get { return m_Report; }
            }

            public byte[] Rumble
            {
                get { return m_Rumble; }
            }

            public byte[] Mapped
            {
                get { return m_Mapped; }
            }
        }
    }
}