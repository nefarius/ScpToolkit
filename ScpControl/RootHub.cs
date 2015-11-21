using System;
using System.Collections.Concurrent;
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
using ScpControl.Plugins;
using ScpControl.Profiler;
using ScpControl.Properties;
using ScpControl.Rx;
using ScpControl.ScpCore;
using ScpControl.Sound;
using ScpControl.Usb;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;
using ScpControl.Usb.Gamepads;
using ScpControl.Utilities;
using ScpControl.Wcf;

namespace ScpControl
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = false, InstanceContextMode = InstanceContextMode.Single)]
    public sealed partial class RootHub : ScpHub, IScpCommandService
    {
        #region Private fields

        // Bluetooth hub
        private readonly BthHub _bthHub = new BthHub();
        private readonly Cache[] _mCache = {new Cache(), new Cache(), new Cache(), new Cache()};

        private readonly byte[][] _mNative =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        private readonly string[] _mReserved = {string.Empty, string.Empty, string.Empty, string.Empty};

        private readonly byte[][] _mXInput =
        {
            new byte[2] {0, 0}, new byte[2] {0, 0}, new byte[2] {0, 0},
            new byte[2] {0, 0}
        };

        // subscribed clients who receive the native stream
        private readonly IDictionary<int, ScpNativeFeedChannel> _nativeFeedSubscribers =
            new ConcurrentDictionary<int, ScpNativeFeedChannel>();

        private readonly IDsDevice[] _pads =
        {
            new DsNull(DsPadId.One), new DsNull(DsPadId.Two), new DsNull(DsPadId.Three),
            new DsNull(DsPadId.Four)
        };

        // virtual bus wrapper
        private readonly BusDevice _scpBus = new BusDevice();
        // USB hub
        private readonly UsbHub _usbHub = new UsbHub();
        // creates a system-wide mutex to check if the root hub has been instantiated already
        private LimitInstance _limitInstance;
        private volatile bool _mSuspended;
        // the WCF service host
        private ServiceHost _rootHubServiceHost;
        // server to broadcast native byte stream
        private ReactiveListener _rxFeedServer;
        private bool _serviceStarted;

        #endregion

        #region IScpCommandService methods

        /// <summary>
        ///     Checks if the native stream is available or disabled in configuration.
        /// </summary>
        /// <returns>True if feed is available, false otherwise.</returns>
        public bool IsNativeFeedAvailable()
        {
            return !GlobalConfiguration.Instance.DisableNative;
        }

        public string GetActiveProfile()
        {
            return scpMap.Active;
        }

        [Obsolete]
        public string GetXml()
        {
            return scpMap.Xml;
        }

        [Obsolete]
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
            var serial = (byte) pad;

            var data = new byte[11];

            Log.DebugFormat("Requested Pads local MAC = {0}", _pads[serial].Local);

            data[0] = serial;
            data[1] = (byte) _pads[serial].State;
            data[2] = (byte) _pads[serial].Model;
            data[3] = (byte) _pads[serial].Connection;
            data[4] = (byte) _pads[serial].Battery;

            Buffer.BlockCopy(_pads[serial].BdAddress, 0, data, 5, _pads[serial].BdAddress.Length);

            return new DsDetail((DsPadId) data[0], (DsState) data[1], (DsModel) data[2],
                _pads[serial].Local.ToBytes().ToArray(),
                (DsConnection) data[3], (DsBattery) data[4]);
        }

        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            var serial = (byte) pad;
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

                Pad[target].PadId = (DsPadId) (target);
                Pad[target - 1].PadId = (DsPadId) (target - 1);

                _mReserved[target] = Pad[target].Local;
                _mReserved[target - 1] = Pad[target - 1].Local;
            }
        }

        /// <summary>
        ///     Requests the currently active configuration set from the root hub.
        /// </summary>
        /// <returns>Returns the global configuration object.</returns>
        public GlobalConfiguration RequestConfiguration()
        {
            return GlobalConfiguration.Request();
        }

        /// <summary>
        ///     Submits an altered copy of the global configuration to the root hub and saves it.
        /// </summary>
        /// <param name="configuration">The global configuration object.</param>
        public void SubmitConfiguration(GlobalConfiguration configuration)
        {
            GlobalConfiguration.Submit(configuration);
            GlobalConfiguration.Save();
        }

        #endregion

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string path)
        {
            if (_mSuspended) return DsPadId.None;

            var classGuid = Guid.Parse(Class);

            // forward message for wired DS4 to usb hub
            if (classGuid == UsbDs4.DeviceClassGuid)
            {
                return _usbHub.Notify(notification, Class, path);
            }

            // forward message for wired DS3 to usb hub
            if (classGuid == UsbDs3.DeviceClassGuid)
            {
                return _usbHub.Notify(notification, Class, path);
            }

            // forward message for wired Generic Gamepad to usb hub
            if (classGuid == UsbGenericGamepad.DeviceClassGuid)
            {
                return _usbHub.Notify(notification, Class, path);
            }

            // forward message for any wireless device to bluetooth hub
            if (classGuid == BthDongle.DeviceClassGuid)
            {
                _bthHub.Notify(notification, Class, path);
            }

            return DsPadId.None;
        }

        #region Internal helpers

        private class Cache
        {
            private readonly byte[] m_Mapped = new byte[ScpHidReport.Length];
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

            _bthHub.Arrival += OnDeviceArrival;
            _usbHub.Arrival += OnDeviceArrival;

            _bthHub.Report += OnHidReportReceived;
            _usbHub.Report += OnHidReportReceived;
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
            get { return _pads; }
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

        /// <summary>
        ///     Opens and initializes devices and services listening and running on the local machine.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public override bool Open()
        {
            var opened = false;

            Log.Info("Initializing root hub");

            _limitInstance = new LimitInstance(@"Global\ScpDsxRootHub");

            try
            {
                if (!_limitInstance.IsOnlyInstance) // existing root hub running as desktop app
                    throw new RootHubAlreadyStartedException(
                        "The root hub is already running, please close the ScpServer first!");
            }
            catch (UnauthorizedAccessException) // existing root hub running as service
            {
                throw new RootHubAlreadyStartedException(
                    "The root hub is already running, please stop the ScpService first!");
            }

            Log.DebugFormat("++ {0} {1}", Assembly.GetExecutingAssembly().Location,
                Assembly.GetExecutingAssembly().GetName().Version);
            Log.DebugFormat("++ {0}", OsInfoHelper.OsInfo);

            #region Native feed server

            _rxFeedServer = new ReactiveListener(Settings.Default.RootHubNativeFeedPort);

            _rxFeedServer.Connections.Subscribe(socket =>
            {
                Log.InfoFormat("Client connected on native feed channel: {0}", socket.GetHashCode());
                var protocol = new ScpNativeFeedChannel(socket);

                _nativeFeedSubscribers.Add(socket.GetHashCode(), protocol);

                protocol.Receiver.Subscribe(packet => { Log.Debug("Uuuhh how did we end up here?!"); });

                socket.Disconnected += (sender, e) =>
                {
                    Log.InfoFormat(
                        "Client disconnected from native feed channel {0}",
                        sender.GetHashCode());

                    _nativeFeedSubscribers.Remove(socket.GetHashCode());
                };

                socket.Disposed += (sender, e) =>
                {
                    Log.InfoFormat("Client disposed from native feed channel {0}",
                        sender.GetHashCode());

                    _nativeFeedSubscribers.Remove(socket.GetHashCode());
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

        /// <summary>
        ///     Starts listening for incoming requests and starts all underlying hubs.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public override bool Start()
        {
            if (m_Started) return m_Started;

            Log.Info("Starting root hub");

            if (!_serviceStarted)
            {
                var baseAddress = new Uri("net.tcp://localhost:26760/ScpRootHubService");

                var binding = new NetTcpBinding
                {
                    TransferMode = TransferMode.Streamed,
                    Security = new NetTcpSecurity {Mode = SecurityMode.None}
                };

                _rootHubServiceHost = new ServiceHost(this, baseAddress);
                _rootHubServiceHost.AddServiceEndpoint(typeof (IScpCommandService), binding, baseAddress);

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

            // make some noise =)
            if (GlobalConfiguration.Instance.IsStartupSoundEnabled)
                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.StartupSoundFile);

            return m_Started;
        }

        /// <summary>
        ///     Stops all underlying hubs and disposes acquired resources.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
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

            m_Started = !m_Started;

            Log.Info("Root hub stopped");

            _limitInstance.Dispose();

            return true;
        }

        /// <summary>
        ///     Stops all underlying hubs, disposes acquired resources and saves the global configuration.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public override bool Close()
        {
            var retval = Stop();

            GlobalConfiguration.Save();

            return retval;
        }

        public override bool Suspend()
        {
            _mSuspended = true;

            lock (_pads)
            {
                foreach (var t in _pads)
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
            for (var index = 0; index < _pads.Length; index++)
            {
                if (_pads[index].State != DsState.Disconnected)
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

        protected override void OnDeviceArrival(object sender, ArrivalEventArgs e)
        {
            var bFound = false;
            var arrived = e.Device;

            lock (_pads)
            {
                for (var index = 0; index < _pads.Length && !bFound; index++)
                {
                    if (arrived.Local == _mReserved[index])
                    {
                        if (_pads[index].State == DsState.Connected)
                        {
                            if (_pads[index].Connection == DsConnection.BTH)
                            {
                                _pads[index].Disconnect();
                            }

                            if (_pads[index].Connection == DsConnection.USB)
                            {
                                arrived.Disconnect();

                                e.Handled = false;
                                return;
                            }
                        }

                        bFound = true;

                        arrived.PadId = (DsPadId) index;
                        _pads[index] = arrived;
                    }
                }

                for (var index = 0; index < _pads.Length && !bFound; index++)
                {
                    if (_pads[index].State == DsState.Disconnected)
                    {
                        bFound = true;
                        _mReserved[index] = arrived.Local;

                        arrived.PadId = (DsPadId) index;
                        _pads[index] = arrived;
                    }
                }
            }

            if (bFound)
            {
                _scpBus.Plugin((int) arrived.PadId + 1);

                Log.InfoFormat("++ Plugin Port #{0} for [{1}]", (int) arrived.PadId + 1, arrived.Local);
            }
            e.Handled = bFound;
        }

        protected override void OnHidReportReceived(object sender, ScpHidReport e)
        {
            var serial = (int) e.PadId;
            var model = e.Model;

            var report = _mCache[serial].Report;
            var rumble = _mCache[serial].Rumble;

            DualShockProfileManager.Instance.PassThroughAllProfiles(e);

            ScpPlugins.Instance.Process(e);

            _scpBus.Parse(e, report, model);

            if (_scpBus.Report(report, rumble) && (DsState) e.RawBytes[1] == DsState.Connected)
            {
                var large = rumble[3];
                var small = rumble[4];

                if (rumble[1] == 0x08 && (large != _mXInput[serial][0] || small != _mXInput[serial][1]))
                {
                    _mXInput[serial][0] = large;
                    _mXInput[serial][1] = small;

                    Pad[serial].Rumble(large, small);
                }
            }

            if ((DsState) e.RawBytes[1] != DsState.Connected)
            {
                _mXInput[serial][0] = _mXInput[serial][1] = 0;
                _mNative[serial][0] = _mNative[serial][1] = 0;
            }

            // skip broadcast if native feed is disabled
            if (GlobalConfiguration.Instance.DisableNative)
                return;

            // send native controller inputs to subscribed clients
            foreach (
                var channel in _nativeFeedSubscribers.Select(nativeFeedSubscriber => nativeFeedSubscriber.Value))
            {
                try
                {
                    channel.SendAsync(e.RawBytes);
                }
                catch (AggregateException)
                {
                    /* This might happen if the client disconnects while sending the 
                     * response is still in progress. The exception can be ignored. */
                }
            }
        }

        #endregion
    }
}