using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
        private readonly ReactiveClient _rxClient = new ReactiveClient(Settings.Default.RootHubRxHost, Settings.Default.RootHubRxPort);
        private readonly ScpByteChannel _rootHubChannel;
        private AutoResetEvent _activeProfileEvent = new AutoResetEvent(false);
        private string _activeProfile;
        private readonly ReactiveListener _rxServer = new ReactiveListener(26761);
        private ScpByteChannel _serverProtocol;

        public ScpProxy()
        {
            InitializeComponent();

            #region Client

            _rootHubChannel = new ScpByteChannel(_rxClient);
            _rootHubChannel.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(packet =>
            {
                /* var packet = new DsPacket();

                LogPacket(packet.Load(message.Payload)); */

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
                }
            });

            _rxClient.ConnectAsync().Wait();

            #endregion
        }

        public ScpProxy(IContainer container)
            : this()
        {
            container.Add(this);
        }

        public XmlMapper Mapper
        {
            get { return m_Mapper; }
        }

        public string Active
        {
            get
            {
                try
                {
                    byte[] send = { 0, 6 };

                    _rootHubChannel.SendAsync(ScpRequest.ProfileList, send);

                    _activeProfileEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }

                return _activeProfile;
            }
        }

        public bool Enabled
        {
            get
            {
                var native = false;

                try
                {
                    byte[] send = { 0, 3 };

                    _rootHubChannel.SendAsync(ScpRequest.ConfigRead, send);

                    /*
                        var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        var Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0)
                        {
                            native = Buffer[13] == 0;
                        }
                    */
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }

                return native;
            }
        }

        public event EventHandler<DsPacket> Packet;

        public bool Start()
        {
            try
            {
                if (!m_Active)
                {
                    #region Server

                    _rxServer.Connections.Subscribe(socket =>
                    {
                        Log.InfoFormat("New socket connected {0}", socket.GetHashCode());

                        _serverProtocol = new ScpByteChannel(socket);

                        _serverProtocol.Receiver.Subscribe(packet =>
                        {
                            var request = packet.Request;
                            var buffer = packet.Payload;

                            /*
                            switch (request)
                            {
                                case ScpRequest.GetXml:
                                    m_Map.LoadXml(buffer.ToUtf8());
                                    m_Mapper.Initialize(m_Map);
                                    break;
                            }
                             * */
                        });

                        socket.Disconnected += (sender, e) => Log.InfoFormat("Socket disconnected {0}", sender.GetHashCode());
                        socket.Disposed += (sender, e) => Log.InfoFormat("Socket disposed {0}", sender.GetHashCode());
                    });

                    #endregion

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
                    _rxClient.Disconnect();
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
            var Loaded = false;

            try
            {
                _rootHubChannel.SendAsync(ScpRequest.GetXml);

                Loaded = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Loaded;
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

                        buffer[1] = 0x09; // TODO: remove
                        Array.Copy(data, 0, buffer, 2, data.Length);

                        _rootHubChannel.SendAsync(ScpRequest.SetXml, buffer);

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

        public bool Select(Profile Target)
        {
            var selected = false;

            try
            {
                if (m_Active)
                {
                    var data = Encoding.Unicode.GetBytes(Target.Name);
                    var send = new byte[data.Length + 2];

                    send[1] = 0x07; // TODO: remove
                    Array.Copy(data, 0, send, 2, data.Length);

                    _rootHubChannel.SendAsync(ScpRequest.SetActiveProfile, send);

                    SetDefault(Target);
                    selected = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return selected;
        }

        public DsDetail Detail(DsPadId Pad)
        {
            DsDetail Detail = null;

            try
            {
                byte[] buffer = { (byte)Pad, 0x0A };

                _rootHubChannel.SendAsync(ScpRequest.PadDetail, buffer);

                /*
                 * var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    buffer = m_Server.Receive(ref ReferenceEp);

                    if (buffer.Length > 0)
                    {
                        var Local = new byte[6];
                        Array.Copy(buffer, 5, Local, 0, Local.Length);

                        Detail = new DsDetail((DsPadId)buffer[0], (DsState)buffer[1], (DsModel)buffer[2], Local,
                            (DsConnection)buffer[3], (DsBattery)buffer[4]);
                    }
                */
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Detail;
        }

        public bool Rumble(DsPadId Pad, byte Large, byte Small)
        {
            var Rumbled = false;

            try
            {
                if (m_Active)
                {
                    byte[] buffer = { (byte)Pad, 0x01, Large, Small };

                    _rootHubChannel.SendAsync(ScpRequest.Rumble, buffer);

                    Rumbled = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Rumbled;
        }

        public bool Remap(string Target, DsPacket Packet)
        {
            var remapped = false;

            try
            {
                if (m_Active)
                {
                    var output = new byte[Packet.Native.Length];

                    switch (Packet.Detail.Model)
                    {
                        case DsModel.DS3:
                            if (m_Mapper.RemapDs3(m_Mapper.Map[Target], Packet.Native, output))
                            {
                                Array.Copy(output, Packet.Native, output.Length);
                                Packet.Remapped();
                            }
                            break;
                        case DsModel.DS4:
                            if (m_Mapper.RemapDs4(m_Mapper.Map[Target], Packet.Native, output))
                            {
                                Array.Copy(output, Packet.Native, output.Length);
                                Packet.Remapped();
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

        public bool SetDefault(Profile Profile)
        {
            var set = true;

            try
            {
                foreach (var Item in m_Mapper.Map.Values)
                {
                    Item.Default = false;
                }

                Profile.Default = true;
            }
            catch (Exception ex)
            {
                set = false;
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return set;
        }

        private void LogPacket(DsPacket Data)
        {
            if (Packet != null)
            {
                Packet(this, Data);
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