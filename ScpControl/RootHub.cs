using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using ScpControl.Wcf;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public sealed partial class RootHub : ScpHub, IScpCommandService
    {
        private volatile bool m_Suspended;
        private ServiceHost myServiceHost;
        private bool serviceStarted;
        private readonly BthHub bthHub = new BthHub();
        private readonly Cache[] m_Cache = { new Cache(), new Cache(), new Cache(), new Cache() };

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

        private readonly byte[][] m_XInput =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly BusDevice scpBus = new BusDevice();
        private readonly UsbHub usbHub = new UsbHub();

        public bool IsNativeFeedAvailable()
        {
            return !Global.DisableNative;
        }

        public string GetActiveProfile()
        {
            return scpMap.Active;
        }

        public string GetXml()
        {
            return scpMap.Xml;
        }

        public void SetXml(string xml)
        {
            scpMap.Xml = xml;
        }

        public void SetActiveProfile(Profile profile)
        {
            scpMap.Active = profile.Name;
        }

        public DsDetail GetPadDetail(DsPadId pad)
        {
            var serial = (byte)pad;

            var data = new byte[11];

            Log.DebugFormat("Requested Pads local MAC = {0}", m_Pad[serial].Local);

            data[0] = serial;
            data[1] = (byte)m_Pad[serial].State;
            data[2] = (byte)m_Pad[serial].Model;
            data[3] = (byte)m_Pad[serial].Connection;
            data[4] = (byte)m_Pad[serial].Battery;

            Array.Copy(m_Pad[serial].BD_Address, 0, data, 5, m_Pad[serial].BD_Address.Length);

            return new DsDetail((DsPadId)data[0], (DsState)data[1], (DsModel)data[2],
                m_Pad[serial].Local.ToBytes().ToArray(),
                (DsConnection)data[3], (DsBattery)data[4]);
        }

        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            var serial = (byte)pad;
            if (Pad[serial].State == DsState.Connected)
            {
                if (large != m_Native[serial][0] || small != m_Native[serial][1])
                {
                    m_Native[serial][0] = large;
                    m_Native[serial][1] = small;

                    Pad[serial].Rumble(large, small);
                }
            }

            return false;
        }

        public IEnumerable<string> GetProfileList()
        {
            return scpMap.Profiles;
        }

        public IEnumerable<byte> GetConfig()
        {
            return Global.Packed;
        }

        public void SetConfig(byte[] buffer)
        {
            Global.Packed = buffer;
        }

        public IEnumerable<string> GetStatusData()
        {
            var list = new List<string>
            {
                Dongle,
                Pad[0].ToString(),
                Pad[1].ToString(),
                Pad[2].ToString(),
                Pad[3].ToString()
            };

            return list;
        }

        public void PromotePad(byte pad)
        {
            int target = pad;

            if (Pad[target].State != DsState.Disconnected)
            {
                var swap = Pad[target];
                Pad[target] = Pad[target - 1];
                Pad[target - 1] = swap;

                Pad[target].PadId = (DsPadId)(target);
                Pad[target - 1].PadId = (DsPadId)(target - 1);

                m_Reserved[target] = Pad[target].Local;
                m_Reserved[target - 1] = Pad[target - 1].Local;
            }
        }

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string Path)
        {
            if (m_Suspended) return DsPadId.None;

            // forward message for wired DS4 to usb hub
            if (Class == UsbDs4.USB_CLASS_GUID)
            {
                return usbHub.Notify(notification, Class, Path);
            }

            // forward message for wired DS3 to usb hub
            if (Class == UsbDs3.USB_CLASS_GUID)
            {
                return usbHub.Notify(notification, Class, Path);
            }

            // forward message for any wireless device to bluetooth hub
            if (Class == BthDongle.BTH_CLASS_GUID)
            {
                bthHub.Notify(notification, Class, Path);
            }

            return DsPadId.None;
        }

        #region Internal helpers

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

        #endregion

        #region Ctors

        public RootHub()
        {
            InitializeComponent();

            bthHub.Arrival += On_Arrival;
            usbHub.Arrival += On_Arrival;

            bthHub.Report += On_Report;
            usbHub.Report += On_Report;
        }

        public RootHub(IContainer container)
            : this()
        {
            container.Add(this);
        }

        #endregion

        #region Properties

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

        #endregion

        #region Actions

        public override bool Open()
        {
            var opened = false;

            Log.Info("Initializing root hub");

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
            if (m_Started) return m_Started;

            Log.Info("Starting root hub");

            if (!serviceStarted)
            {
                var baseAddress = new Uri("net.tcp://localhost:26760/ScpRootHubService");

                var binding = new NetTcpBinding();

                myServiceHost = new ServiceHost(this, baseAddress);
                myServiceHost.AddServiceEndpoint(typeof(IScpCommandService), binding, baseAddress);

                myServiceHost.Open();

                serviceStarted = true;
            }

            scpMap.Start();

            m_Started |= scpBus.Start();
            m_Started |= usbHub.Start();
            m_Started |= bthHub.Start();

            Log.Info("Root hub started");

            return m_Started;
        }

        public override bool Stop()
        {
            Log.Info("Root hub stop requested");

            scpMap.Stop();
            scpBus.Stop();
            usbHub.Stop();
            bthHub.Stop();

            Log.Info("Root hub stopped");

            return true;
        }

        public override bool Close()
        {
            Stop();

            Global.Save();

            return true;
        }

        public override bool Suspend()
        {
            m_Suspended = true;

            lock (m_Pad)
            {
                foreach (var t in m_Pad)
                    t.Disconnect();
            }

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

        #endregion

        #region Events

        protected override void On_Arrival(object sender, ArrivalEventArgs e)
        {
            var bFound = false;
            var arrived = e.Device;

            lock (m_Pad)
            {
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
            }

            if (bFound)
            {
                scpBus.Plugin((int)arrived.PadId + 1);

                Log.DebugFormat("++ Plugin Port #{0} for [{1}]", (int)arrived.PadId + 1, arrived.Local);
            }
            e.Handled = bFound;
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

            if (Global.DisableNative)
                return;

            // TODO: implement feed!
        }

        #endregion
    }
}