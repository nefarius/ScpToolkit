using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl
{
    public sealed partial class RootHub : ScpHub
    {
        private CancellationTokenSource _udpWorkerCancellationToken = new CancellationTokenSource();
        private readonly BthHub bthHub = new BthHub();
        private readonly Cache[] m_Cache = { new Cache(), new Cache(), new Cache(), new Cache() };
        private readonly UdpClient m_Client = new UdpClient();
        private readonly IPEndPoint m_ClientEp = new IPEndPoint(IPAddress.Loopback, 26761);

        private readonly byte[][] m_Native =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly IDsDevice[] m_Pad =
        {
            new DsNull(DsPadId.One), new DsNull(DsPadId.Two), new DsNull(DsPadId.Three),
            new DsNull(DsPadId.Four)
        };

        private readonly string[] m_Reserved = { string.Empty, string.Empty, string.Empty, string.Empty };
        private readonly IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);

        private readonly byte[][] m_XInput =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly BusDevice scpBus = new BusDevice();
        private readonly UsbHub usbHub = new UsbHub();
        private Task _udpWorkerTask;
        private UdpClient m_Server = new UdpClient();
        private volatile bool m_Suspended;

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

                if (m_Started)
                    _udpWorkerTask = Task.Factory.StartNew(UdpWorker,
                        _udpWorkerCancellationToken.Token);
            }

            return m_Started;
        }

        private void UdpWorker(object o)
        {
            var sb = new StringBuilder();
            var remote = new IPEndPoint(IPAddress.Loopback, 0);
            var token = (CancellationToken) o;

            // create new UDP channel; unblock receive after 500ms
            m_Server = new UdpClient(m_ServerEp) { Client = { ReceiveTimeout = 500 } };

            Log.Debug("-- Controller : UDP_Worker_Thread Starting");
            
            // loop endlessly until parent requested cancellation
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // swallow exception on timeout; returns null if no data received
                    var buffer = Log.TryCatchSilent(() => m_Server.Receive(ref remote));

                    if (buffer == null)
                        continue;

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
                            var data = Global.Packed;

                            m_Server.Send(data, data.Length, remote);
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
                            var temp = m_Pad[serial].Local;
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

            // TODO: status var shouldn't be necessary, refactor and remove!
            m_Started = !m_Started;
            m_Server.Close();

            Log.Debug("-- Controller : UDP_Worker_Thread Exiting");
        }

        public new async Task<bool> Stop()
        {
            // no need to stop if task isn't running anymore
            if (_udpWorkerTask.Status != TaskStatus.Running)
                return !m_Started;

            // signal task to stop work
            _udpWorkerCancellationToken.Cancel();
            // wait until task completed
            await Task.WhenAll(_udpWorkerTask);
            // reset cancellation token
            _udpWorkerCancellationToken = new CancellationTokenSource();

            scpMap.Stop();
            scpBus.Stop();
            usbHub.Stop();
            bthHub.Stop();

            return !m_Started;
        }

        public new async Task<bool> Close()
        {
            await Stop();

            Global.Save();

            return true;
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

                        arrived.PadId = (DsPadId)index;
                        m_Pad[index] = arrived;
                    }
                }

                for (var index = 0; index < m_Pad.Length && !bFound; index++)
                {
                    if (m_Pad[index].State == DsState.Disconnected)
                    {
                        bFound = true;
                        m_Reserved[index] = arrived.Local;

                        arrived.PadId = (DsPadId)index;
                        m_Pad[index] = arrived;
                    }
                }

                if (bFound)
                {
                    scpBus.Plugin((int)arrived.PadId + 1);

                    Log.DebugFormat("++ Plugin Port #{0} for [{1}]", (int)arrived.PadId + 1, arrived.Local);
                }
                e.Handled = bFound;
            }
        }

        protected override void On_Report(object sender, ReportEventArgs e)
        {
            int serial = e.Report[(int)DsOffset.Pad];
            var model = (DsModel)e.Report[(int)DsOffset.Model];

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

            if (scpBus.Report(report, rumble) && (DsState)e.Report[1] == DsState.Connected)
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

            if ((DsState)e.Report[1] != DsState.Connected)
            {
                m_XInput[serial][0] = m_XInput[serial][1] = 0;
                m_Native[serial][0] = m_Native[serial][1] = 0;
            }

            if (!Global.DisableNative) m_Client.Send(e.Report, e.Report.Length, m_ClientEp);
        }

        private class Cache
        {
            private readonly byte[] m_Mapped = new byte[ReportEventArgs.Length];
            private readonly byte[] m_Report = new byte[BusDevice.ReportSize];
            private readonly byte[] m_Rumble = new byte[BusDevice.RumbleSize];

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