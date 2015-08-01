using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private readonly XmlMapper m_Mapper = new XmlMapper();
        private bool m_Active;
        private XmlDocument m_Map = new XmlDocument();
        private readonly ReactiveClient _rxCommandClient = new ReactiveClient(Settings.Default.RootHubCommandRxHost, Settings.Default.RootHubCommandRxPort);
        private readonly ScpByteChannel _rootHubCommandChannel;
        private readonly ReactiveClient _rxFeedClient = new ReactiveClient(Settings.Default.RootHubNativeFeedRxHost, Settings.Default.RootHubNativeFeedRxPort);
        private readonly AutoResetEvent _activeProfileEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _padDetailEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _nativeFeedEnabledEvent = new AutoResetEvent(false);
        private string _activeProfile;
        private DsDetail _padDetail;
        private bool _nativeFeedAvailable;

        #region Ctors

        public ScpProxy()
        {
            InitializeComponent();

            try
            {
                #region Command cient

                try
                {
                    _rootHubCommandChannel = new ScpByteChannel(_rxCommandClient);
                    _rootHubCommandChannel.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(packet =>
                    {
                        var request = packet.Request;
                        var buffer = packet.Payload;

                        switch (request)
                        {
                            case ScpRequest.GetXml:
                                m_Map.LoadXml(buffer.ToUtf8());
                                m_Mapper.Initialize(m_Map);
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

                                _padDetailEvent.Set();
                                break;
                            case ScpRequest.NativeFeedAvailable:
                                _nativeFeedAvailable = BitConverter.ToBoolean(buffer, 0);
                                _nativeFeedEnabledEvent.Set();
                                break;
                        }
                    });

                    _rxCommandClient.ConnectAsync().Wait();
                }
                catch (Exception ex)
                {
                    Log.FatalFormat("Couldn't connect to root hub: {0}", ex);
                }

                #endregion

                #region Feed client

                var rootHubFeedChannel = new ScpByteChannel(_rxFeedClient);
                rootHubFeedChannel.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(packet =>
                {
                    var request = packet.Request;
                    var buffer = packet.Payload;

                    switch (request)
                    {
                        case ScpRequest.NativeFeed:
                            var dsPacket = new DsPacket();

                            LogPacket(dsPacket.Load(buffer));
                            break;
                    }
                });

                _rxFeedClient.ConnectAsync().Wait();

                #endregion
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Couldn't connect to root hub: {0}", ex);
            }
        }

        public ScpProxy(IContainer container)
            : this()
        {
            container.Add(this);
        }

        #endregion

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
                _activeProfileEvent.WaitOne();

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

                _nativeFeedEnabledEvent.WaitOne();

                return _nativeFeedAvailable;
            }
        }

        public event EventHandler<DsPacket> Packet;

        public bool Start()
        {
            try
            {
                if (!m_Active)
                {
                    // TODO: remove or improve

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
                    // TODO: improve
                    //_rxCommandClient.Disconnect();
                    //_rxFeedClient.Disconnect();

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
                    var data = Encoding.Unicode.GetBytes(target.Name);
                    var send = new byte[data.Length + 2];

                    send[1] = (byte)ScpRequest.SetActiveProfile;
                    Array.Copy(data, 0, send, 2, data.Length);

                    // request root hub to set new active profile
                    _rootHubCommandChannel.SendAsync(ScpRequest.SetActiveProfile, send);

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

                _rootHubCommandChannel.SendAsync(ScpRequest.PadDetail, buffer).Wait();

                _padDetailEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return _padDetail;
        }

        public bool Rumble(DsPadId Pad, byte Large, byte Small)
        {
            var rumbled = false;

            try
            {
                if (m_Active)
                {
                    byte[] buffer = { (byte)Pad, (byte)ScpRequest.Rumble, Large, Small };

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

        private void LogPacket(DsPacket data)
        {
            if (Packet != null)
            {
                Packet(this, data);
            }
        }
    }

    public class DsPacket : EventArgs
    {
        private DsDetail m_Detail = new DsDetail();
        private Ds3Button m_Ds3Button = Ds3Button.None;
        private Ds4Button m_Ds4Button = Ds4Button.None;
        private byte[] m_Local = new byte[6];
        private byte[] m_Native = new byte[96];
        private int m_Packet;

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
        private DsBattery m_Charge;
        private byte[] m_Local = new byte[6];
        private DsConnection m_Mode;
        private DsModel m_Model;
        private DsPadId m_Serial;
        private DsState m_State;

        internal DsDetail()
        {
        }

        internal DsDetail(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);
        }

        public DsPadId Pad
        {
            get { return m_Serial; }
        }

        public DsState State
        {
            get { return m_State; }
        }

        public DsModel Model
        {
            get { return m_Model; }
        }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }
        }

        public DsConnection Mode
        {
            get { return m_Mode; }
        }

        public DsBattery Charge
        {
            get { return m_Charge; }
        }

        internal DsDetail Load(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode,
            DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);

            return this;
        }
    }
}