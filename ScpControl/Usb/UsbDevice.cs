using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using HidSharp.ReportDescriptors.Parser;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Sound;
using ScpControl.Utilities;

namespace ScpControl.Usb
{
    /// <summary>
    ///     Represents a generic USB device.
    /// </summary>
    public partial class UsbDevice : ScpDevice, IDsDevice
    {
        private CancellationTokenSource _hidCancellationTokenSource = new CancellationTokenSource();

        public override string ToString()
        {
            switch (m_State)
            {
                case DsState.Disconnected:

                    return string.Format("Pad {0} : Disconnected", m_ControllerId + 1);

                case DsState.Reserved:

                    return string.Format("Pad {0} : {1} {2} - Reserved", m_ControllerId + 1, Model, Local);

                case DsState.Connected:

                    return string.Format("Pad {0} : {1} {2} - {3} {4:X8} {5}", m_ControllerId + 1, Model,
                        Local,
                        Connection,
                        PacketCounter,
                        Battery
                        );
            }

            throw new Exception();
        }

        /// <summary>
        ///     Worker thread polling for incoming USB interrupts.
        /// </summary>
        /// <param name="o">Task cancellation token.</param>
        private void HidWorker(object o)
        {
            var token = (CancellationToken) o;
            var transfered = 0;
            var buffer = new byte[64];

            Log.Debug("-- USB Device : HID_Worker_Thread Starting");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (ReadIntPipe(buffer, buffer.Length, ref transfered) && transfered > 0)
                    {
                        ParseHidReport(buffer);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }
            }

            Log.Debug("-- USB Device : HID_Worker_Thread Exiting");
        }

        private void On_Timer(object sender, EventArgs e)
        {
            lock (this)
            {
                Process(DateTime.Now);
            }
        }

        #region Protected fields

        protected byte m_BatteryStatus = 0;
        protected byte[] m_Buffer = new byte[64];
        protected byte m_CableStatus = 0;
        protected byte m_ControllerId;
        protected string m_Instance = string.Empty, m_Mac = string.Empty;
        protected bool m_IsDisconnect;
        protected DateTime m_Last = DateTime.Now, m_Tick = DateTime.Now, m_Disconnect = DateTime.Now;
        protected byte[] m_Local = new byte[6];
        protected byte[] m_Master = new byte[6];
        protected byte m_Model = 0;
        protected uint PacketCounter;
        protected byte m_PlugStatus = 0;
        protected bool m_Publish = false;
        protected readonly ScpHidReport InputReport = new ScpHidReport();
        protected DsState m_State = DsState.Disconnected;
        protected readonly ReportDescriptorParser ReportDescriptor = new ReportDescriptorParser();

        #endregion

        #region Events

        public event EventHandler<ScpHidReport> HidReportReceived;

        protected virtual void OnHidReportReceived()
        {
            InputReport.PadId = (DsPadId)m_ControllerId;
            InputReport.PadState = m_State;

            if (HidReportReceived != null) HidReportReceived(this, InputReport);
        }

        #endregion

        #region Ctors

        protected UsbDevice(Guid guid)
            : base(guid)
        {
            InitializeComponent();
        }

        public UsbDevice()
        {
            InitializeComponent();
        }

        public UsbDevice(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region Properties

        public virtual bool IsShutdown
        {
            get { return m_IsDisconnect; }
            set { m_IsDisconnect = value; }
        }

        public virtual DsModel Model
        {
            get { return (DsModel) m_Model; }
        }

        public virtual DsPadId PadId
        {
            get { return (DsPadId) m_ControllerId; }
            set
            {
                m_ControllerId = (byte) value;

                InputReport.PadId = PadId;
            }
        }

        public virtual DsConnection Connection
        {
            get { return DsConnection.USB; }
        }

        public virtual DsState State
        {
            get { return m_State; }
        }

        public virtual DsBattery Battery
        {
            get { return (DsBattery) m_BatteryStatus; }
        }

        public virtual byte[] BdAddress
        {
            get { return m_Local; }
        }

        public virtual string Local
        {
            get { return m_Mac; }
        }

        public virtual string Remote
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Master[0], m_Master[1], m_Master[2],
                    m_Master[3], m_Master[4], m_Master[5]);
            }
        }

        #endregion

        #region Actions

        public override bool Start()
        {
            if (!IsActive) return State == DsState.Connected;

            Buffer.BlockCopy(m_Local, 0, InputReport.RawBytes, (int) DsOffset.Address, m_Local.Length);

            InputReport.ConnectionType = Connection;
            InputReport.Model = Model;

            m_State = DsState.Connected;
            PacketCounter = 0;

            Task.Factory.StartNew(HidWorker, _hidCancellationTokenSource.Token);

            tmUpdate.Enabled = true;

            Rumble(0, 0);
            Log.DebugFormat("-- Started Device Instance [{0}] Local [{1}] Remote [{2}]", m_Instance, Local, Remote);

            // connection sound
            if (GlobalConfiguration.Instance.IsUsbConnectSoundEnabled)
                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.UsbConnectSoundFile);

            #region Request HID Report Descriptor

            // try to retrieve HID Report Descriptor
            var buffer = new byte[512];
            var transfered = 0;

            if (SendTransfer(UsbHidRequestType.GetDescriptor, UsbHidRequest.GetDescriptor,
                ToValue(UsbHidClassDescriptorType.Report),
                buffer, ref transfered) && transfered > 0)
            {
                Log.DebugFormat("-- HID Report Descriptor: {0}", buffer.ToHexString(transfered));

                // store report descriptor
                ReportDescriptor.Parse(buffer);
            }

            #endregion

            return State == DsState.Connected;
        }

        public virtual bool Rumble(byte large, byte small)
        {
            return false;
        }

        public virtual bool Pair(byte[] master)
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            return true;
        }

        protected virtual void Process(DateTime now)
        {
        }

        protected virtual void ParseHidReport(byte[] report)
        {
        }

        protected virtual bool Shutdown()
        {
            Stop();

            return RestartDevice(m_Instance);
        }

        public override bool Stop()
        {
            if (IsActive)
            {
                tmUpdate.Enabled = false;
                m_State = DsState.Reserved;

                _hidCancellationTokenSource.Cancel();
                _hidCancellationTokenSource = new CancellationTokenSource();

                OnHidReportReceived();
            }

            return base.Stop();
        }

        public override bool Close()
        {
            if (IsActive)
            {
                base.Close();

                tmUpdate.Enabled = false;
                m_State = DsState.Disconnected;

                OnHidReportReceived();
            }

            return !IsActive;
        }

        #endregion
    }
}