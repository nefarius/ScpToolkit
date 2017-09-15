using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using HidReport.Contract.Enums;
using log4net;
using ReactiveSockets;
using ScpControl.Properties;
using ScpControl.Rx;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
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
            ScpHidReport inputReport;
            var extended = default(SCP_EXTN);

            try
            {
                inputReport = _packetCache[(DsPadId) dwUserIndex];
            }
            catch (KeyNotFoundException)
            {
                return extended;
            }

            var hidReport = inputReport.HidReport;
            switch (inputReport.Model)
            {
                case DsModel.None:
                    break;
                case DsModel.DS3:
                    // translate and wrap button/axis information
                    extended = new SCP_EXTN
                    {
                        SCP_UP     = hidReport [AxesEnum.Up].Value,
                        SCP_RIGHT  = hidReport [AxesEnum.Right].Value,
                        SCP_DOWN   = hidReport [AxesEnum.Down].Value,
                        SCP_LEFT   = hidReport [AxesEnum.Left].Value,
                        SCP_LX     = hidReport [AxesEnum.Lx].Axis,
                        SCP_LY     = -hidReport[AxesEnum.Ly].Axis,
                        SCP_L1     = hidReport [AxesEnum.L1].Value,
                        SCP_L2     = hidReport [AxesEnum.L2].Value,
                        SCP_L3     = hidReport [ButtonsEnum.L3].Value,
                        SCP_RX     = hidReport [AxesEnum.Rx].Axis,
                        SCP_RY     = -hidReport[AxesEnum.Ry].Axis,
                        SCP_R1     = hidReport [AxesEnum.R1].Value,
                        SCP_R2     = hidReport [AxesEnum.R2].Value,
                        SCP_R3     = hidReport [ButtonsEnum.R3].Value,
                        SCP_T      = hidReport [AxesEnum.Triangle].Value,
                        SCP_C      = hidReport [AxesEnum.Circle].Value,
                        SCP_X      = hidReport [AxesEnum.Cross].Value,
                        SCP_S      = hidReport [AxesEnum.Square].Value,
                        SCP_SELECT = hidReport [ButtonsEnum.Select].Value,
                        SCP_START  = hidReport [ButtonsEnum.Start].Value,
                        SCP_PS     = hidReport [ButtonsEnum.Ps].Value
                    };
                    break;
                case DsModel.DS4:
                    extended = new SCP_EXTN
                    {
                        SCP_UP = hidReport[ButtonsEnum.Up].Value,
                        SCP_RIGHT = hidReport[ButtonsEnum.Right].Value,
                        SCP_DOWN = hidReport[ButtonsEnum.Down].Value,
                        SCP_LEFT = hidReport[ButtonsEnum.Left].Value,
                        SCP_LX = hidReport[AxesEnum.Lx].Value,
                        SCP_LY = hidReport[AxesEnum.Ly].Value,
                        SCP_L1 = hidReport[ButtonsEnum.L1].Value,
                        SCP_L2 = hidReport[AxesEnum.L2].Value,
                        SCP_L3 = hidReport[ButtonsEnum.L3].Value,
                        SCP_RX = hidReport[AxesEnum.Rx].Value,
                        SCP_RY = hidReport[AxesEnum.Ry].Value,
                        SCP_R1 = hidReport[ButtonsEnum.R1].Value,
                        SCP_R2 = hidReport[AxesEnum.R2].Value,
                        SCP_R3 = hidReport[ButtonsEnum.R3].Value,
                        SCP_T = hidReport[ButtonsEnum.Triangle].Value,
                        SCP_C = hidReport[ButtonsEnum.Circle].Value,
                        SCP_X = hidReport[ButtonsEnum.Cross].Value,
                        SCP_S = hidReport[ButtonsEnum.Square].Value,
                        SCP_SELECT = hidReport[ButtonsEnum.Share].Value,
                        SCP_START = hidReport[ButtonsEnum.Options].Value,
                        SCP_PS = hidReport[ButtonsEnum.Ps].Value
                    };
                    break;
            }

            return extended;
        }

        public ScpHidReport GetReport(uint dwUserIndex)
        {
            try
            {
                return _packetCache[(DsPadId) dwUserIndex];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        #endregion

        #region Private fields

        private readonly ReactiveClient _rxFeedClient = new ReactiveClient(Settings.Default.RootHubNativeFeedHost,
            Settings.Default.RootHubNativeFeedPort);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // caches the latest HID report for every pad in a thread-save dictionary
        private readonly IDictionary<DsPadId, ScpHidReport> _packetCache =
            new ConcurrentDictionary<DsPadId, ScpHidReport>();

        private IScpCommandService _rootHub;

        #endregion

        #region Public properties

        public bool IsActive { get; private set; }

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

        /// <summary>
        ///     Receives details about the provided pad.
        /// </summary>
        /// <param name="pad">The pad ID to query details for.</param>
        /// <returns>The pad details returned from the root hub.</returns>
        public DualShockPadMeta Detail(DsPadId pad)
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

        public IEnumerable<DualShockProfile> GetProfiles()
        {
            return _rootHub.GetProfiles();
        }

        public void SubmitProfile(DualShockProfile profile)
        {
            _rootHub.SubmitProfile(profile);
        }

        public void RemoveProfile(DualShockProfile profile)
        {
            _rootHub.RemoveProfile(profile);
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
                        ScpHidReport report;
                        using (var ms = new MemoryStream(buffer))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            report = (ScpHidReport)formatter.Deserialize(ms);
                        }
                        OnFeedPacketReceived(report);
                    });

                    _rxFeedClient.ConnectAsync();

                    #endregion

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
                    //_rxFeedClient.Disconnect();
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

        public event EventHandler<ScpHidReport> NativeFeedReceived;

        public event EventHandler<EventArgs> RootHubDisconnected;

        #endregion

        #region Event methods

        private void OnFeedPacketReceived(ScpHidReport data)
        {
            _packetCache[data.PadId] = data;

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