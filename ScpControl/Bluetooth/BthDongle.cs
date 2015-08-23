using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScpControl.ScpCore;

namespace ScpControl.Bluetooth
{
    /// <summary>
    ///     Communication logic for Bluetooth host dongles.
    /// </summary>
    public sealed partial class BthDongle : ScpDevice, IBthDevice
    {
        public const string BTH_CLASS_GUID = "{2F87C733-60E0-4355-8515-95D6978418B2}";
        private CancellationTokenSource _hciCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _l2CapCancellationTokenSource = new CancellationTokenSource();
        private string _hciVersion = string.Empty;
        private byte _hidReportId = 0x01;
        private string _lmpVersion = string.Empty;
        private byte[] _localMac = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private DsState _state = DsState.Disconnected;
        private readonly ConnectionList _connected = new ConnectionList();

        public BthDongle()
            : base(BTH_CLASS_GUID)
        {
            Initialised = false;
            InitializeComponent();
        }

        public BthDongle(IContainer container)
            : base(BTH_CLASS_GUID)
        {
            Initialised = false;
            container.Add(this);

            InitializeComponent();
        }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", _localMac[5], _localMac[4], _localMac[3],
                    _localMac[2], _localMac[1], _localMac[0]);
            }
        }

        public string HciVersion
        {
            get { return _hciVersion; }
            set { _hciVersion = value; }
        }

        public string LmpVersion
        {
            get { return _lmpVersion; }
            set { _lmpVersion = value; }
        }

        public DsState State
        {
            get { return _state; }
        }

        public bool Initialised { get; private set; }

        #region HIDP Commands

        public int HID_Command(byte[] Handle, byte[] Channel, byte[] Data)
        {
            var Transfered = 0;
            var Buffer = new byte[Data.Length + 8];

            Buffer[0] = Handle[0];
            Buffer[1] = Handle[1];
            Buffer[2] = (byte)((Data.Length + 4) % 256);
            Buffer[3] = (byte)((Data.Length + 4) / 256);
            Buffer[4] = (byte)(Data.Length % 256);
            Buffer[5] = (byte)(Data.Length / 256);
            Buffer[6] = Channel[0];
            Buffer[7] = Channel[1];

            for (var i = 0; i < Data.Length; i++) Buffer[i + 8] = Data[i];

            WriteBulkPipe(Buffer, Data.Length + 8, ref Transfered);
            return Transfered;
        }

        #endregion

        public event EventHandler<ArrivalEventArgs> Arrival;
        public event EventHandler<ReportEventArgs> Report;

        private bool LogArrival(IDsDevice arrived)
        {
            var args = new ArrivalEventArgs(arrived);

            if (Arrival != null)
            {
                Arrival(this, args);
            }

            return args.Handled;
        }

        public override bool Open(int instance = 0)
        {
            if (base.Open(instance))
            {
                _state = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Open(string devicePath)
        {
            if (base.Open(devicePath))
            {
                _state = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            if (!IsActive) return State == DsState.Connected;

            _state = DsState.Connected;

            Task.Factory.StartNew(HicWorker, _hciCancellationTokenSource.Token);
            Task.Factory.StartNew(L2CapWorker, _l2CapCancellationTokenSource.Token);

            return State == DsState.Connected;
        }

        public override bool Stop()
        {
            if (!IsActive) return base.Stop();

            _state = DsState.Reserved;

            // disconnect all connected devices gracefully
            foreach (var device in _connected.Values)
            {
                device.Disconnect();
                device.Stop();
            }

            // notify tasks to stop work
            _hciCancellationTokenSource.Cancel();
            _l2CapCancellationTokenSource.Cancel();
            // reset tokens
            _hciCancellationTokenSource = new CancellationTokenSource();
            _l2CapCancellationTokenSource = new CancellationTokenSource();

            _connected.Clear();

            return base.Stop();
        }

        public override bool Close()
        {
            var closed = base.Close();

            _state = DsState.Disconnected;

            return closed;
        }

        public override string ToString()
        {
            switch (State)
            {
                case DsState.Reserved:
                    if (Initialised)
                    {
                        return
                            string.Format("Host Address : {0}\n\nHCI Version  : {1}\n\nLMP Version  : {2}\n\nReserved",
                                Local,
                                _hciVersion,
                                _lmpVersion
                                );
                    }
                    return "Host Address : <Error>";

                case DsState.Connected:
                    if (Initialised)
                    {
                        return string.Format("Host Address : {0}\n\nHCI Version  : {1}\n\nLMP Version  : {2}",
                            Local,
                            _hciVersion,
                            _lmpVersion
                            );
                    }
                    return "Host Address : <Error>";
            }

            return "Host Address : Disconnected";
        }

        private BthDevice Add(byte lsb, byte msb, string name)
        {
            BthDevice connection = null;

            if (_connected.Count < 4)
            {
                if (name == "Wireless Controller")
                    connection = new BthDs4(this, _localMac, lsb, msb);
                else
                    connection = new BthDs3(this, _localMac, lsb, msb);

                _connected[connection.HciHandle] = connection;
            }

            return connection;
        }

        /// <summary>
        ///     Returns an existing Bluetooth connection based on an incoming buffer.
        /// </summary>
        /// <param name="lsb">Least significant bit in byte stream.</param>
        /// <param name="msb">Most significant bit in byte stream.</param>
        /// <returns>The Bluetooth connection, null if not found.</returns>
        private BthDevice GetConnection(byte lsb, byte msb)
        {
            var hande = new BthHandle(lsb, msb);

            return (!_connected.Any() | !_connected.ContainsKey(hande)) ? null : _connected[hande];
        }

        private void Remove(byte lsb, byte msb)
        {
            var connection = new BthHandle(lsb, msb);

            if (!_connected.ContainsKey(connection))
                return;

            _connected[connection].Stop();
            _connected.Remove(connection);
        }

        private class ConnectionList : SortedDictionary<BthHandle, BthDevice>
        {
        }

        #region Events

        private void OnInitialised(BthDevice connection)
        {
            if (LogArrival(connection))
            {
                connection.HidReportReceived += On_Report;
                connection.Start();
            }
        }

        private void OnCompletedCount(byte lsb, byte msb, ushort count)
        {
            if (count > 0) _connected[new BthHandle(lsb, msb)].Completed();
        }

        private void On_Report(object sender, ReportEventArgs e)
        {
            if (Report != null) Report(sender, e);
        }

        #endregion
    }
}
