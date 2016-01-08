using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using HidSharp.ReportDescriptors.Parser;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Sound;
using ScpControl.Utilities;

namespace ScpControl.Usb
{
    /// <summary>
    ///     Represents a generic Usb device.
    /// </summary>
    public partial class UsbDevice : ScpDevice, IDsDevice
    {
        #region Private fields

        private readonly IObservable<long> _outputReportSchedule = Observable.Interval(TimeSpan.FromMilliseconds(10),
            Scheduler.Default);

        private CancellationTokenSource _hidCancellationTokenSource = new CancellationTokenSource();

        private IDisposable _outputReportTask;

        private readonly TaskQueue _inputReportQueue = new TaskQueue();

        #endregion

        #region Private methods

        /// <summary>
        ///     Worker thread polling for incoming Usb interrupts.
        /// </summary>
        /// <param name="o">Task cancellation token.</param>
        private void HidWorker(object o)
        {
            var token = (CancellationToken) o;
            var transfered = 0;
            var buffer = new byte[64];

            Log.Debug("-- Usb Device : HID_Worker_Thread Starting");

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

            Log.Debug("-- Usb Device : HID_Worker_Thread Exiting");
        }

        #endregion

        #region Protected fields

        protected byte[] m_Buffer = new byte[64];
        protected byte m_CableStatus = 0;
        protected string m_Instance = string.Empty;
        protected DateTime m_Last = DateTime.Now, m_Tick = DateTime.Now, m_Disconnect = DateTime.Now;
        protected uint PacketCounter;
        protected byte m_PlugStatus = 0;
        protected bool m_Publish = false;
        protected readonly ReportDescriptorParser ReportDescriptor = new ReportDescriptorParser();

        #endregion

        #region Events

        public event EventHandler<ScpHidReport> HidReportReceived;

        protected void OnHidReportReceived(ScpHidReport report)
        {
            if (GlobalConfiguration.Instance.UseAsyncHidReportProcessing)
            {
                _inputReportQueue.Enqueue(() => Task.Run(() =>
                {
                    if (HidReportReceived != null)
                        HidReportReceived.Invoke(this, report);
                }));
            }
            else
            {
                if (HidReportReceived != null)
                    HidReportReceived.Invoke(this, report);
            }
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

        public virtual bool IsShutdown { get; set; }

        /// <summary>
        ///     Controller model.
        /// </summary>
        public virtual DsModel Model { get; protected set; }

        public virtual DsPadId PadId { get; set; }

        /// <summary>
        ///     Controller connection type.
        /// </summary>
        public virtual DsConnection Connection
        {
            get { return DsConnection.Usb; }
        }

        /// <summary>
        ///     Controller connection state.
        /// </summary>
        public virtual DsState State { get; protected set; }

        /// <summary>
        ///     Battery charging level.
        /// </summary>
        public virtual DsBattery Battery { get; protected set; }

        public virtual PhysicalAddress DeviceAddress { get; protected set; }

        public virtual PhysicalAddress HostAddress { get; protected set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Crafts a new <see cref="ScpHidReport"/> with current devices meta data.
        /// </summary>
        /// <returns>The new HID <see cref="ScpHidReport"/>.</returns>
        public ScpHidReport NewHidReport()
        {
            return new ScpHidReport
            {
                PadId = PadId,
                PadState = State,
                ConnectionType = Connection,
                Model = Model,
                PadMacAddress = DeviceAddress,
                BatteryStatus = (byte) Battery
            };
        }

        public override bool Start()
        {
            if (!IsActive) return State == DsState.Connected;

            State = DsState.Connected;
            PacketCounter = 0;

            Task.Factory.StartNew(HidWorker, _hidCancellationTokenSource.Token);

            _outputReportTask = _outputReportSchedule.Subscribe(tick => Process(DateTime.Now));

            Rumble(0, 0);
            Log.DebugFormat("-- Started Device Instance [{0}] Local [{1}] Remote [{2}]", m_Instance,
                DeviceAddress.AsFriendlyName(), HostAddress.AsFriendlyName());

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

        public virtual bool Pair(PhysicalAddress master)
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            return true;
        }

        public override bool Stop()
        {
            if (IsActive)
            {
                if (_outputReportTask != null)
                    _outputReportTask.Dispose();

                State = DsState.Reserved;

                _hidCancellationTokenSource.Cancel();
                _hidCancellationTokenSource = new CancellationTokenSource();

                OnHidReportReceived(NewHidReport());
            }

            return base.Stop();
        }

        public override bool Close()
        {
            if (IsActive)
            {
                base.Close();

                if (_outputReportTask != null)
                    _outputReportTask.Dispose();

                State = DsState.Disconnected;

                OnHidReportReceived(NewHidReport());
            }

            return !IsActive;
        }

        public override string ToString()
        {
            switch (State)
            {
                case DsState.Disconnected:

                    return string.Format("Pad {0} : Disconnected", PadId);

                case DsState.Reserved:

                    return string.Format("Pad {0} : {1} {2} - Reserved", PadId, Model, DeviceAddress.AsFriendlyName());

                case DsState.Connected:

                    return string.Format("Pad {0} : {1} {2} - {3} {4:X8} {5}", PadId, Model,
                        DeviceAddress.AsFriendlyName(),
                        Connection,
                        PacketCounter,
                        Battery
                        );
            }

            throw new Exception();
        }

        #endregion

        #region Protected methods

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

        #endregion
    }
}
