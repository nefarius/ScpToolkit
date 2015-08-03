using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using log4net;
using ReactiveSockets;
using ScpControl.Properties;
using ScpControl.Rx;
using ScpControl.Wcf;

namespace ScpControl
{
    public sealed partial class ScpProxy : Component
    {
        private readonly ReactiveClient _rxFeedClient = new ReactiveClient(Settings.Default.RootHubNativeFeedHost, Settings.Default.RootHubNativeFeedPort);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool m_Active;
        private XmlDocument m_Map = new XmlDocument();

        private IScpCommandService _rootHub;

        private readonly XmlMapper m_Mapper = new XmlMapper();

        public XmlMapper Mapper
        {
            get { return m_Mapper; }
        }

        /// <summary>
        ///     Gets the currently active profile.
        /// </summary>
        public string ActiveProfile
        {
            get { return _rootHub.GetActiveProfile(); }
        }

        /// <summary>
        ///     Checks if the native feed is available.
        /// </summary>
        public bool IsNativeFeedAvailable
        {
            get { return _rootHub.IsNativeFeedAvailable(); }
        }

        public IList<string> StatusData
        {
            get
            {
                return _rootHub.GetStatusData().ToList();
            }
        }

        public void PromotePad(byte pad)
        {
            _rootHub.PromotePad(pad);
        }

        public IEnumerable<byte> ReadConfig()
        {
            return _rootHub.GetConfig();
        }

        public void WriteConfig(byte[] config)
        {
            _rootHub.SetConfig(config);
        }

        public bool Start()
        {
            try
            {
                if (!m_Active)
                {
                    var address = new EndpointAddress(new Uri("net.tcp://localhost:26760/ScpRootHubService"));
                    var binding = new NetTcpBinding();
                    var factory = new ChannelFactory<IScpCommandService>(binding, address);

                    _rootHub = factory.CreateChannel(address);

                    #region Feed client

                    var rootHubFeedChannel = new ScpNativeFeedChannel(_rxFeedClient);
                    rootHubFeedChannel.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(buffer =>
                    {
                        if (buffer.Length <= 0)
                            return;

                        var packet = new DsPacket();

                        OnFeedPacketReceived(packet.Load(buffer));
                    });

                    _rxFeedClient.ConnectAsync();

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
                    m_Active = false;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return !m_Active;
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
                        _rootHub.SetXml(m_Map.InnerXml);

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
                    _rootHub.SetActiveProfile(target);

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
            return _rootHub.GetPadDetail(pad);
        }

        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            return _rootHub.Rumble(pad, large, small);
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

        #region Ctors

        public ScpProxy()
        {
            InitializeComponent();
        }

        public ScpProxy(IContainer container)
            : this()
        {
            container.Add(this);
        }

        #endregion

        public void OnDisconnect()
        {
            //OnRootHubDisconnected(this, null);
        }
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