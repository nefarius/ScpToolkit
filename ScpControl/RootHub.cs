using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using Libarius.System;
using ReactiveSockets;
using ScpControl.Bluetooth;
using ScpControl.Exceptions;
using ScpControl.Properties;
using ScpControl.Rx;
using ScpControl.ScpCore;
using ScpControl.Utilities;
using ScpControl.Wcf;

namespace ScpControl
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = false, InstanceContextMode = InstanceContextMode.Single)]
    public sealed partial class RootHub : ScpHub, IScpCommandService
    {
        private readonly LimitInstance _limitInstance = new LimitInstance(@"Global\ScpDsxRootHub");
        private readonly ReactiveListener _rxFeedServer = new ReactiveListener(Settings.Default.RootHubNativeFeedPort);
        private readonly IDictionary<int, ScpNativeFeedChannel> _nativeFeedSubscribers = new Dictionary<int, ScpNativeFeedChannel>();
        private volatile bool _mSuspended;
        private ServiceHost _rootHubServiceHost;
        private bool _serviceStarted;
        private readonly BthHub _bthHub = new BthHub();
        private readonly Cache[] _mCache = { new Cache(), new Cache(), new Cache(), new Cache() };

        private readonly byte[][] _mNative =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly IDsDevice[] _mPad =
        {
            new DsNull(DsPadId.One), new DsNull(DsPadId.Two), new DsNull(DsPadId.Three),
            new DsNull(DsPadId.Four)
        };

        private readonly string[] _mReserved = { string.Empty, string.Empty, string.Empty, string.Empty };

        private readonly byte[][] _mXInput =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly BusDevice _scpBus = new BusDevice();
        private readonly UsbHub _usbHub = new UsbHub();

        public bool IsNativeFeedAvailable()
        {
            return !GlobalConfiguration.Instance.DisableNative;
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

            Log.DebugFormat("Requested Pads local MAC = {0}", _mPad[serial].Local);

            data[0] = serial;
            data[1] = (byte)_mPad[serial].State;
            data[2] = (byte)_mPad[serial].Model;
            data[3] = (byte)_mPad[serial].Connection;
            data[4] = (byte)_mPad[serial].Battery;

            Array.Copy(_mPad[serial].BD_Address, 0, data, 5, _mPad[serial].BD_Address.Length);

            return new DsDetail((DsPadId)data[0], (DsState)data[1], (DsModel)data[2],
                _mPad[serial].Local.ToBytes().ToArray(),
                (DsConnection)data[3], (DsBattery)data[4]);
        }

        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            var serial = (byte)pad;
            if (Pad[serial].State == DsState.Connected)
            {
                if (large != _mNative[serial][0] || small != _mNative[serial][1])
                {
                    _mNative[serial][0] = large;
                    _mNative[serial][1] = small;

                    Pad[serial].Rumble(large, small);
                }
            }

            return false;
        }

        public IEnumerable<string> GetProfileList()
        {
            return scpMap.Profiles;
        }

        public IEnumerable<string> GetStatusData()
        {
            if (!_serviceStarted)
                return default(IEnumerable<string>);

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

                _mReserved[target] = Pad[target].Local;
                _mReserved[target - 1] = Pad[target - 1].Local;
            }
        }

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string Path)
        {
            if (_mSuspended) return DsPadId.None;

            // forward message for wired DS4 to usb hub
            if (Class == UsbDs4.USB_CLASS_GUID)
            {
                return _usbHub.Notify(notification, Class, Path);
            }

            // forward message for wired DS3 to usb hub
            if (Class == UsbDs3.USB_CLASS_GUID)
            {
                return _usbHub.Notify(notification, Class, Path);
            }

            // forward message for any wireless device to bluetooth hub
            if (Class == BthDongle.BTH_CLASS_GUID)
            {
                _bthHub.Notify(notification, Class, Path);
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

            _bthHub.Arrival += On_Arrival;
            _usbHub.Arrival += On_Arrival;

            _bthHub.Report += On_Report;
            _usbHub.Report += On_Report;
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
            get { return _mPad; }
        }

        public string Dongle
        {
            get { return _bthHub.Dongle; }
        }

        public string Master
        {
            get { return _bthHub.Master; }
        }

        public bool Pairable
        {
            get { return m_Started && _bthHub.Pairable; }
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

            #region Native feed server

            _rxFeedServer.Connections.Subscribe(socket =>
            {
                Log.InfoFormat("Client connected on native feed channel: {0}", socket.GetHashCode());
                var protocol = new ScpNativeFeedChannel(socket);

                lock (this)
                {
                    _nativeFeedSubscribers.Add(socket.GetHashCode(), protocol);
                }

                protocol.Receiver.Subscribe(packet =>
                {
                    Log.Debug("Uuuhh how did we end up here?!");
                });

                socket.Disconnected += (sender, e) =>
                {
                    Log.InfoFormat(
                        "Client disconnected from native feed channel {0}",
                        sender.GetHashCode());

                    lock (this)
                    {
                        _nativeFeedSubscribers.Remove(socket.GetHashCode());
                    }
                };

                socket.Disposed += (sender, e) =>
                {
                    Log.InfoFormat("Client disposed from native feed channel {0}",
                        sender.GetHashCode());

                    lock (this)
                    {
                        _nativeFeedSubscribers.Remove(socket.GetHashCode());
                    }
                };
            });

            #endregion

            scpMap.Open();

            opened |= _scpBus.Open(GlobalConfiguration.Instance.Bus);
            opened |= _usbHub.Open();
            opened |= _bthHub.Open();

            GlobalConfiguration.Load();
            return opened;
        }

        public override bool Start()
        {
            try
            {
                if (!_limitInstance.IsOnlyInstance) // existing root hub running as desktop app
                    throw new RootHubAlreadyStartedException("The root hub is already running, please close the ScpServer first!");
            }
            catch (UnauthorizedAccessException) // existing root hub running as service
            {
                throw new RootHubAlreadyStartedException("The root hub is already running, please stop the ScpService first!");
            }

            if (m_Started) return m_Started;

            Log.Info("Starting root hub");

            if (!_serviceStarted)
            {
                var baseAddress = new Uri("net.tcp://localhost:26760/ScpRootHubService");

                var binding = new NetTcpBinding();

                _rootHubServiceHost = new ServiceHost(this, baseAddress);
                _rootHubServiceHost.AddServiceEndpoint(typeof(IScpCommandService), binding, baseAddress);

                _rootHubServiceHost.Open();

                _serviceStarted = true;
            }

            try
            {
                _rxFeedServer.Start();
            }
            catch (SocketException sex)
            {
                Log.FatalFormat("Couldn't start native feed server: {0}", sex);
                return false;
            }

            scpMap.Start();

            m_Started |= _scpBus.Start();
            m_Started |= _usbHub.Start();
            m_Started |= _bthHub.Start();

            Log.Info("Root hub started");

            return m_Started;
        }

        public override bool Stop()
        {
            Log.Info("Root hub stop requested");

            _serviceStarted = false;

            if (_rootHubServiceHost != null)
                _rootHubServiceHost.Close();

            if (_rxFeedServer != null)
                _rxFeedServer.Dispose();

            scpMap.Stop();
            _scpBus.Stop();
            _usbHub.Stop();
            _bthHub.Stop();

            Log.Info("Root hub stopped");

            return true;
        }

        public override bool Close()
        {
            Stop();

            GlobalConfiguration.Save();

            return true;
        }

        public override bool Suspend()
        {
            _mSuspended = true;

            lock (_mPad)
            {
                foreach (var t in _mPad)
                    t.Disconnect();
            }

            _scpBus.Suspend();
            _usbHub.Suspend();
            _bthHub.Suspend();

            Log.Debug("++ Suspended");
            return true;
        }

        public override bool Resume()
        {
            Log.Debug("++ Resumed");

            _scpBus.Resume();
            for (var index = 0; index < _mPad.Length; index++)
            {
                if (_mPad[index].State != DsState.Disconnected)
                {
                    _scpBus.Plugin(index + 1);
                }
            }

            _usbHub.Resume();
            _bthHub.Resume();

            _mSuspended = false;
            return true;
        }

        #endregion

        #region Events

        protected override void On_Arrival(object sender, ArrivalEventArgs e)
        {
            var bFound = false;
            var arrived = e.Device;

            lock (_mPad)
            {
                for (var index = 0; index < _mPad.Length && !bFound; index++)
                {
                    if (arrived.Local == _mReserved[index])
                    {
                        if (_mPad[index].State == DsState.Connected)
                        {
                            if (_mPad[index].Connection == DsConnection.BTH)
                            {
                                _mPad[index].Disconnect();
                            }

                            if (_mPad[index].Connection == DsConnection.USB)
                            {
                                arrived.Disconnect();

                                e.Handled = false;
                                return;
                            }
                        }

                        bFound = true;

                        arrived.PadId = (DsPadId)index;
                        _mPad[index] = arrived;
                    }
                }

                for (var index = 0; index < _mPad.Length && !bFound; index++)
                {
                    if (_mPad[index].State == DsState.Disconnected)
                    {
                        bFound = true;
                        _mReserved[index] = arrived.Local;

                        arrived.PadId = (DsPadId)index;
                        _mPad[index] = arrived;
                    }
                }
            }

            if (bFound)
            {
                _scpBus.Plugin((int)arrived.PadId + 1);

                Log.DebugFormat("++ Plugin Port #{0} for [{1}]", (int)arrived.PadId + 1, arrived.Local);
            }
            e.Handled = bFound;
        }

        protected override void On_Report(object sender, ReportEventArgs e)
        {
            int serial = e.Report[(int)DsOffset.Pad];
            var model = (DsModel)e.Report[(int)DsOffset.Model];

            var report = _mCache[serial].Report;
            var rumble = _mCache[serial].Rumble;
            var mapped = _mCache[serial].Mapped;

            if (scpMap.Remap(model, serial, _mPad[serial].Local, e.Report, mapped))
            {
                _scpBus.Parse(mapped, report, model);
            }
            else
            {
                _scpBus.Parse(e.Report, report, model);
            }

            if (_scpBus.Report(report, rumble) && (DsState)e.Report[1] == DsState.Connected)
            {
                var Large = rumble[3];
                var Small = rumble[4];

                if (rumble[1] == 0x08 && (Large != _mXInput[serial][0] || Small != _mXInput[serial][1]))
                {
                    _mXInput[serial][0] = Large;
                    _mXInput[serial][1] = Small;

                    Pad[serial].Rumble(Large, Small);
                }
            }

            if ((DsState)e.Report[1] != DsState.Connected)
            {
                _mXInput[serial][0] = _mXInput[serial][1] = 0;
                _mNative[serial][0] = _mNative[serial][1] = 0;
            }

            if (GlobalConfiguration.Instance.DisableNative)
                return;

            lock (this)
            {
                // send native controller inputs to subscribed clients
                foreach (
                    var channel in _nativeFeedSubscribers.Select(nativeFeedSubscriber => nativeFeedSubscriber.Value))
                {
                    try
                    {
                        channel.SendAsync(e.Report);
                    }
                    catch (AggregateException)
                    {
                    }
                }
            }
        }

        #endregion
        
        public GlobalConfiguration RequestConfiguration()
        {
            return GlobalConfiguration.Request();
        }

        public void SubmitConfiguration(GlobalConfiguration configuration)
        {
            GlobalConfiguration.Submit(configuration);
        }
    }
}