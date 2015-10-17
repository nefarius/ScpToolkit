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
using ScpControl.ScpCore;
using ScpControl.Shared.Utilities;
using ScpControl.Shared.XInput;
using ScpControl.Wcf;

namespace ScpControl
{
    public sealed partial class ScpProxy : Component
    {
        #region XInput extensions

        /// <summary>
        ///     Used by ScpXInputBridge to request pressure sensitive button information.
        /// </summary>
        /// <param name="dwUserIndex">The pad index to request data from (zero-based).</param>
        /// <returns>The pressure sensitive button/axis information.</returns>
        public SCP_EXTN GetExtended(uint dwUserIndex)
        {
            DsPacket packet;
            var extended = default(SCP_EXTN);

            try
            {
                packet = _packetCache[(DsPadId) dwUserIndex];
            }
            catch (KeyNotFoundException)
            {
                return extended;
            }

            var native = packet.Native;

            switch (packet.Detail.Model)
            {
                case DsModel.None:
                    break;
                case DsModel.DS3:
                    // translate and wrap button/axis information
                    extended = new SCP_EXTN
                    {
                        SCP_UP = native.GetDpadUpAnalog().ToPressure(),
                        SCP_RIGHT = native.GetDpadRightAnalog().ToPressure(),
                        SCP_DOWN = native.GetDpadDownAnalog().ToPressure(),
                        SCP_LEFT = native.GetDpadLeftAnalog().ToPressure(),
                        SCP_LX = native.GetLeftAxisX(),
                        SCP_LY = native.GetLeftAxisY(),
                        SCP_L1 = native.GetLeftShoulderAnalog().ToPressure(),
                        SCP_L2 = native.GetLeftTriggerAnalog().ToPressure(),
                        SCP_L3 = native.GetLeftThumb() ? 1.0f : 0.0f,
                        SCP_RX = native.GetRightAxisX(),
                        SCP_RY = native.GetRightAxisY(),
                        SCP_R1 = native.GetRightShoulderAnalog().ToPressure(),
                        SCP_R2 = native.GetRightTriggerAnalog().ToPressure(),
                        SCP_R3 = native.GetRightThumb() ? 1.0f : 0.0f,
                        SCP_T = native.GetTriangleAnalog().ToPressure(),
                        SCP_C = native.GetCircleAnalog().ToPressure(),
                        SCP_X = native.GetCrossAnalog().ToPressure(),
                        SCP_S = native.GetSquareAnalog().ToPressure(),
                        SCP_SELECT = native.GetSelect() ? 1.0f : 0.0f,
                        SCP_START = native.GetStart() ? 1.0f : 0.0f,
                        SCP_PS = native.GetPs() ? 1.0f : 0.0f
                    };
                    break;
                    // TODO: implement DS4 and Generic
            }

            return extended;
        }

        #endregion

        #region Private fields

        private readonly ReactiveClient _rxFeedClient = new ReactiveClient(Settings.Default.RootHubNativeFeedHost,
            Settings.Default.RootHubNativeFeedPort);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<DsPadId, DsPacket> _packetCache = new Dictionary<DsPadId, DsPacket>();

        private XmlDocument _xmlMap = new XmlDocument();

        private IScpCommandService _rootHub;

        private readonly XmlMapper _xmlMapper = new XmlMapper();

        #endregion

        #region Public properties

        public bool IsActive { get; private set; }

        public XmlMapper Mapper
        {
            get { return _xmlMapper; }
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
            get { return _rootHub.GetStatusData().ToList(); }
        }

        #endregion

        #region WCF methods

        public void PromotePad(byte pad)
        {
            _rootHub.PromotePad(pad);
        }

        public GlobalConfiguration ReadConfig()
        {
            return _rootHub.RequestConfiguration();
        }

        public void WriteConfig(GlobalConfiguration config)
        {
            _rootHub.SubmitConfiguration(config);
        }

        public bool Save()
        {
            var saved = false;

            try
            {
                if (IsActive)
                {
                    if (_xmlMapper.Construct(ref _xmlMap))
                    {
                        _rootHub.SetXml(_xmlMap.InnerXml);

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
                if (IsActive)
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

        /// <summary>
        ///     Submit a rumble request for a specified pad.
        /// </summary>
        /// <param name="pad">The target pad.</param>
        /// <param name="large">Rumble with the large (typically left) motor.</param>
        /// <param name="small">Rumble with the small (typically right) motor.</param>
        /// <returns>Returns request status.</returns>
        public bool Rumble(DsPadId pad, byte large, byte small)
        {
            return _rootHub.Rumble(pad, large, small);
        }

        public bool Remap(string target, DsPacket packet)
        {
            var remapped = false;

            try
            {
                if (IsActive)
                {
                    if (!_xmlMapper.Map.Any())
                        return false;

                    var output = new byte[packet.Native.Length];

                    switch (packet.Detail.Model)
                    {
                        case DsModel.DS3:
                            if (_xmlMapper.RemapDs3(_xmlMapper.Map[target], packet.Native, output))
                            {
                                Array.Copy(output, packet.Native, output.Length);
                                packet.Remapped();
                            }
                            break;
                        case DsModel.DS4:
                            if (_xmlMapper.RemapDs4(_xmlMapper.Map[target], packet.Native, output))
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
                foreach (var item in _xmlMapper.Map.Values)
                {
                    item.IsDefault = false;
                }

                profile.IsDefault = true;
            }
            catch (Exception ex)
            {
                set = false;
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return set;
        }

        #endregion

        #region Component actions

        public bool Start()
        {
            try
            {
                if (!IsActive)
                {
                    #region WCF client

                    var address = new EndpointAddress(new Uri("net.tcp://localhost:26760/ScpRootHubService"));
                    var binding = new NetTcpBinding
                    {
                        TransferMode = TransferMode.Streamed,
                        Security = new NetTcpSecurity {Mode = SecurityMode.None}
                    };
                    var factory = new ChannelFactory<IScpCommandService>(binding, address);

                    _rootHub = factory.CreateChannel(address);

                    #endregion

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

                    if (_rootHub != null)
                    {
                        _xmlMap.LoadXml(_rootHub.GetXml());
                        _xmlMapper.Initialize(_xmlMap);
                    }
                    else
                    {
                        Log.Error("Couldn't initialize XML mapper");
                    }

                    IsActive = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return IsActive;
        }

        public bool Stop()
        {
            // TODO: refactor useless bits
            try
            {
                if (IsActive)
                {
                    IsActive = false;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return !IsActive;
        }

        #endregion

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

        #region Public events

        public event EventHandler<DsPacket> NativeFeedReceived;

        public event EventHandler<EventArgs> RootHubDisconnected;

        #endregion

        #region Event methods

        private void OnFeedPacketReceived(DsPacket data)
        {
            _packetCache[data.Detail.Pad] = data;

            if (NativeFeedReceived != null)
            {
                NativeFeedReceived(this, data);
            }
        }

        private void OnRootHubDisconnected(object sender, EventArgs args)
        {
            if (RootHubDisconnected != null)
            {
                RootHubDisconnected(sender, args);
            }
        }

        #endregion
    }
}