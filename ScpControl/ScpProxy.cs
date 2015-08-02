using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using ReactiveSockets;
using ScpControl.Properties;
using ScpControl.Rx;
using ScpControl.Utilities;

namespace ScpControl
{
    public sealed partial class ScpProxy : Component
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly char[] m_Delim = { '^' };
        private string _activeProfile;
        private bool _nativeFeedAvailable;
        private DsDetail _padDetail;
        private bool m_Active;
        private XmlDocument m_Map = new XmlDocument();
        private readonly AutoResetEvent _activeProfileEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _nativeFeedEnabledEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _padDetailEvent = new AutoResetEvent(false);
        private readonly ScpCommandChannel _rootHubCommandChannel;

        private readonly ReactiveClient _rxCommandClient = new ReactiveClient(Settings.Default.RootHubCommandRxHost,
            Settings.Default.RootHubCommandRxPort);

        private readonly ReactiveClient _rxFeedClient = new ReactiveClient(Settings.Default.RootHubNativeFeedRxHost,
            Settings.Default.RootHubNativeFeedRxPort);

        private readonly XmlMapper m_Mapper = new XmlMapper();

        public XmlMapper Mapper
        {
            get { return m_Mapper; }
        }

        /// <summary>
        ///     Gets the currently active profile.
        /// </summary>
        public string Active
        {
            get
            {
                if (!_rxCommandClient.IsConnected)
                    return _activeProfile;

                // send request to root hub
                _rootHubCommandChannel.SendAsync(ScpRequest.ProfileList);
                // wait for response to arrive
                _activeProfileEvent.WaitOne(500);

                return _activeProfile;
            }
        }

        /// <summary>
        ///     Checks if the native feed is available.
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!_rxCommandClient.IsConnected)
                    return _nativeFeedAvailable;

                _rootHubCommandChannel.SendAsync(ScpRequest.NativeFeedAvailable);

                if (_nativeFeedEnabledEvent.WaitOne(500)) return _nativeFeedAvailable;

                Log.Warn("no response received");
                return false;
            }
        }

        public async Task<bool> Start()
        {
            try
            {
                if (!m_Active)
                {
                    await _rxCommandClient.ConnectAsync().ConfigureAwait(false);
                    await _rxFeedClient.ConnectAsync().ConfigureAwait(false);

                    m_Active = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return m_Active;
        }

        public bool Stop()
        {
            try
            {
                if (m_Active)
                {
                    if (_rxCommandClient.IsConnected)
                        _rxCommandClient.Disconnect();

                    if (_rxFeedClient.IsConnected)
                        _rxFeedClient.Disconnect();

                    m_Active = false;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return !m_Active;
        }

        public bool Load()
        {
            if (_rootHubCommandChannel == null)
                return false;

            // request configuration from root hub
            _rootHubCommandChannel.SendAsync(ScpRequest.GetXml);

            return true;
        }

        public bool Save()
        {
            var saved = false;

            try
            {
                if (m_Active)
                {
                    if (m_Mapper.Construct(ref m_Map))
                    {
                        var data = m_Map.InnerXml.ToBytes().ToArray();
                        var buffer = new byte[data.Length + 2];

                        buffer[1] = (byte)ScpRequest.SetXml;
                        Array.Copy(data, 0, buffer, 2, data.Length);

                        // send config back to hub for storage
                        _rootHubCommandChannel.SendAsync(ScpRequest.SetXml, buffer);

                        saved = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return saved;
        }

        public bool Select(Profile target)
        {
            var selected = false;

            try
            {
                if (m_Active)
                {
                    // request root hub to set new active profile
                    _rootHubCommandChannel.SendAsync(ScpRequest.SetActiveProfile, target.Name.ToBytes().ToArray());

                    SetDefault(target);
                    selected = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return selected;
        }

        /// <summary>
        ///     Receives details about the provided pad.
        /// </summary>
        /// <param name="pad">The pad ID to query details for.</param>
        /// <returns>The pad details returned from the root hub.</returns>
        public DsDetail Detail(DsPadId pad)
        {
            try
            {
                byte[] buffer = { (byte)pad, (byte)ScpRequest.PadDetail };

                _rootHubCommandChannel.SendAsync(ScpRequest.PadDetail, buffer);

                if (!_padDetailEvent.WaitOne(500))
                {
                    Log.WarnFormat("Couldn't get details for pad ID {0}", pad);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return _padDetail;
        }

        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            var rumbled = false;

            try
            {
                if (m_Active)
                {
                    byte[] buffer = { (byte)pad, (byte)ScpRequest.Rumble, large, small };

                    _rootHubCommandChannel.SendAsync(ScpRequest.Rumble, buffer);

                    rumbled = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return rumbled;
        }

        public bool Remap(string target, DsPacket packet)
        {
            var remapped = false;

            try
            {
                if (m_Active)
                {
                    if (!m_Mapper.Map.Any())
                        return false;

                    var output = new byte[packet.Native.Length];

                    switch (packet.Detail.Model)
                    {
                        case DsModel.DS3:
                            if (m_Mapper.RemapDs3(m_Mapper.Map[target], packet.Native, output))
                            {
                                Array.Copy(output, packet.Native, output.Length);
                                packet.Remapped();
                            }
                            break;
                        case DsModel.DS4:
                            if (m_Mapper.RemapDs4(m_Mapper.Map[target], packet.Native, output))
                            {
                                Array.Copy(output, packet.Native, output.Length);
                                packet.Remapped();
                            }
                            break;
                    }

                    remapped = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return remapped;
        }

        public bool SetDefault(Profile profile)
        {
            var set = true;

            try
            {
                foreach (var item in m_Mapper.Map.Values)
                {
                    item.Default = false;
                }

                profile.Default = true;
            }
            catch (Exception ex)
            {
                set = false;
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return set;
        }

        public void SubmitRequest(ScpRequest request)
        {
            lock (_rootHubCommandChannel)
                _rootHubCommandChannel.SendAsync(request).ConfigureAwait(false);
        }

        public void SubmitRequest(ScpRequest request, byte[] payload)
        {
            lock (_rootHubCommandChannel)
                _rootHubCommandChannel.SendAsync(request, payload).ConfigureAwait(false);
        }

        #region Ctors

        public ScpProxy()
        {
            InitializeComponent();

            try
            {
                #region Command cient

                try
                {
                    _rootHubCommandChannel = new ScpCommandChannel(_rxCommandClient);

                    _rxCommandClient.Disconnected += (sender, args) =>
                    {
                        Log.Info("Server connection has been closed");
                        OnRootHubDisconnected(args);
                    };

                    _rxCommandClient.Disposed += (sender, args) =>
                    {
                        Log.Info("Server connection has been disposed");
                        OnRootHubDisconnected(args);
                    };

                    _rootHubCommandChannel.Receiver.SubscribeOn(TaskPoolScheduler.Default)
                        .ObserveOn(Scheduler.CurrentThread).Subscribe(OnIncomingPacket);
                }
                catch (Exception ex)
                {
                    Log.FatalFormat("Couldn't connect to root hub: {0}", ex);
                }

                #endregion

                #region Feed client

                var rootHubFeedChannel = new ScpNativeFeedChannel(_rxFeedClient);
                rootHubFeedChannel.Receiver.SubscribeOn(Scheduler.Immediate).Subscribe(buffer =>
                {
                    if (buffer.Length <= 0)
                        return;

                    var packet = new DsPacket();

                    OnFeedPacketReceived(packet.Load(buffer));
                });

                #endregion
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Couldn't connect to root hub: {0}", ex);
            }
        }

        /// <summary>
        ///     This is where responses from the root hub are getting processed
        /// </summary>
        /// <param name="packet">The received packet.</param>
        private void OnIncomingPacket(ScpCommandPacket packet)
        {
            Log.DebugFormat("CMD IN Thread ID: {0}", Thread.CurrentThread.ManagedThreadId);

            var request = packet.Request;
            var buffer = packet.Payload;

            switch (request)
            {
                case ScpRequest.GetXml:
                    m_Map.LoadXml(buffer.ToUtf8());
                    m_Mapper.Initialize(m_Map);

                    OnXmlReceived(packet.ForwardPacket());
                    break;
                case ScpRequest.ProfileList:
                    var data = buffer.ToUtf8();
                    var split = data.Split(m_Delim, StringSplitOptions.RemoveEmptyEntries);

                    _activeProfile = split[0];
                    _activeProfileEvent.Set();
                    break;
                case ScpRequest.PadDetail:
                    var local = new byte[6];
                    Array.Copy(buffer, 5, local, 0, local.Length);

                    _padDetail = new DsDetail((DsPadId)buffer[0], (DsState)buffer[1], (DsModel)buffer[2],
                        local,
                        (DsConnection)buffer[3], (DsBattery)buffer[4]);

                    if (!_padDetailEvent.Set())
                        Log.ErrorFormat("Couldn't signal pad detail event");
                    break;
                case ScpRequest.NativeFeedAvailable:
                    _nativeFeedAvailable = BitConverter.ToBoolean(buffer, 0);

                    if (!_nativeFeedEnabledEvent.Set())
                        Log.ErrorFormat("Couldn't signal native feed available event");
                    break;
                case ScpRequest.StatusData:
                    OnStatusData(packet.ForwardPacket());

                    break;
                case ScpRequest.ConfigRead:
                    OnConfigReceived(packet.ForwardPacket());
                    break;
            }
        }

        public ScpProxy(IContainer container)
            : this()
        {
            container.Add(this);
        }

        #endregion
    }

    public class DsPacket : EventArgs
    {
        private Ds3Button m_Ds3Button = Ds3Button.None;
        private Ds4Button m_Ds4Button = Ds4Button.None;
        private int m_Packet;
        private readonly DsDetail m_Detail = new DsDetail();
        private readonly byte[] m_Local = new byte[6];
        private readonly byte[] m_Native = new byte[96];

        internal DsPacket()
        {
        }

        internal byte[] Native
        {
            get { return m_Native; }
        }

        public DsDetail Detail
        {
            get { return m_Detail; }
        }

        internal DsPacket Load(byte[] Native)
        {
            Array.Copy(Native, (int)DsOffset.Address, m_Local, 0, m_Local.Length);

            m_Detail.Load(
                (DsPadId)Native[(int)DsOffset.Pad],
                (DsState)Native[(int)DsOffset.State],
                (DsModel)Native[(int)DsOffset.Model],
                m_Local,
                (DsConnection)Native[(int)DsOffset.Connection],
                (DsBattery)Native[(int)DsOffset.Battery]
                );

            m_Packet = Native[4] << 0 | Native[5] << 8 | Native[6] << 16 | Native[7] << 24;
            Array.Copy(Native, m_Native, m_Native.Length);

            switch (m_Detail.Model)
            {
                case DsModel.DS3:
                    m_Ds3Button =
                        (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
                    break;
            }

            return this;
        }

        internal void Remapped()
        {
            switch (m_Detail.Model)
            {
                case DsModel.DS3:
                    m_Ds3Button =
                        (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
                    break;
            }
        }

        public bool Button(Ds3Button Flag)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return m_Ds3Button.HasFlag(Flag);
        }

        public bool Button(Ds4Button Flag)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return m_Ds4Button.HasFlag(Flag);
        }

        public byte Axis(Ds3Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return Native[(int)Offset];
        }

        public byte Axis(Ds4Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return Native[(int)Offset];
        }
    }

    public class DsDetail
    {
        private readonly byte[] m_Local = new byte[6];

        internal DsDetail()
        {
        }

        internal DsDetail(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            Pad = PadId;
            this.State = State;
            this.Model = Model;
            this.Mode = Mode;
            Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);
        }

        public DsPadId Pad { get; private set; }
        public DsState State { get; private set; }
        public DsModel Model { get; private set; }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }
        }

        public DsConnection Mode { get; private set; }
        public DsBattery Charge { get; private set; }

        internal DsDetail Load(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode,
            DsBattery Level)
        {
            Pad = PadId;
            this.State = State;
            this.Model = Model;
            this.Mode = Mode;
            Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);

            return this;
        }
    }
}