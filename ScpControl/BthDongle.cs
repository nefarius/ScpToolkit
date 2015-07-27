using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScpControl
{
    public sealed partial class BthDongle : ScpDevice, IBthDevice
    {
        public const string BTH_CLASS_GUID = "{2F87C733-60E0-4355-8515-95D6978418B2}";
        private CancellationTokenSource _hciCancellationTokenSource = new CancellationTokenSource();
        private Task _hciWorkerTask;
        private CancellationTokenSource _l2CapCancellationTokenSource = new CancellationTokenSource();
        private Task _l2CapWorkerTask;
        private string m_HCI_Version = string.Empty;
        private byte m_Id = 0x01;
        private string m_LMP_Version = string.Empty;
        private byte[] m_Local = new byte[6] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        private DsState m_State = DsState.Disconnected;
        private readonly ConnectionList m_Connected = new ConnectionList();

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
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[5], m_Local[4], m_Local[3],
                    m_Local[2], m_Local[1], m_Local[0]);
            }
        }

        public string HCI_Version
        {
            get { return m_HCI_Version; }
            set { m_HCI_Version = value; }
        }

        public string LMP_Version
        {
            get { return m_LMP_Version; }
            set { m_LMP_Version = value; }
        }

        public DsState State
        {
            get { return m_State; }
        }

        public bool Initialised { get; private set; }

        #region HIDP Commands

        public int HID_Command(byte[] Handle, byte[] Channel, byte[] Data)
        {
            var Transfered = 0;
            var Buffer = new byte[Data.Length + 8];

            Buffer[0] = Handle[0];
            Buffer[1] = Handle[1];
            Buffer[2] = (byte) ((Data.Length + 4)%256);
            Buffer[3] = (byte) ((Data.Length + 4)/256);
            Buffer[4] = (byte) (Data.Length%256);
            Buffer[5] = (byte) (Data.Length/256);
            Buffer[6] = Channel[0];
            Buffer[7] = Channel[1];

            for (var i = 0; i < Data.Length; i++) Buffer[i + 8] = Data[i];

            WriteBulkPipe(Buffer, Data.Length + 8, ref Transfered);
            return Transfered;
        }

        #endregion

        public event EventHandler<ArrivalEventArgs> Arrival;
        public event EventHandler<ReportEventArgs> Report;

        private bool LogArrival(IDsDevice Arrived)
        {
            var args = new ArrivalEventArgs(Arrived);

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
                m_State = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Open(string devicePath)
        {
            if (base.Open(devicePath))
            {
                m_State = DsState.Reserved;
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            if (IsActive)
            {
                m_State = DsState.Connected;

                _hciWorkerTask = Task.Factory.StartNew(HicWorker, _hciCancellationTokenSource.Token);
                _l2CapWorkerTask = Task.Factory.StartNew(L2CapWorker, _l2CapCancellationTokenSource.Token);
            }

            return State == DsState.Connected;
        }

        public new async Task<bool> Stop()
        {
            if (IsActive)
            {
                m_State = DsState.Reserved;

                foreach (var device in m_Connected.Values)
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

                // run async to avoid deadlock when called from ScpServer
                await Task.Run(() => HCI_Reset());

                m_Connected.Clear();
            }

            return base.Stop();
        }

        public override bool Close()
        {
            var Closed = base.Close();

            m_State = DsState.Disconnected;

            return Closed;
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
                                m_HCI_Version,
                                m_LMP_Version
                                );
                    }
                    return "Host Address : <Error>";

                case DsState.Connected:
                    if (Initialised)
                    {
                        return string.Format("Host Address : {0}\n\nHCI Version  : {1}\n\nLMP Version  : {2}",
                            Local,
                            m_HCI_Version,
                            m_LMP_Version
                            );
                    }
                    return "Host Address : <Error>";
            }

            return "Host Address : Disconnected";
        }

        private BthDevice Add(byte Lsb, byte Msb, string Name)
        {
            BthDevice Connection = null;

            if (m_Connected.Count < 4)
            {
                if (Name == "Wireless Controller")
                    Connection = new BthDs4(this, m_Local, Lsb, Msb);
                else
                    Connection = new BthDs3(this, m_Local, Lsb, Msb);

                m_Connected[Connection.HCI_Handle] = Connection;
            }

            return Connection;
        }

        private BthDevice Get(byte Lsb, byte Msb)
        {
            var hande = new BthHandle(Lsb, Msb);

            return (!m_Connected.Any() | !m_Connected.ContainsKey(hande)) ? null : m_Connected[hande];
        }

        private void Remove(byte Lsb, byte Msb)
        {
            var connection = new BthHandle(Lsb, Msb);

            if (!m_Connected.ContainsKey(connection))
                return;

            m_Connected[connection].Stop();
            m_Connected.Remove(connection);
        }

        private class ConnectionList : SortedDictionary<BthHandle, BthDevice>
        {
        }

        #region Events

        private void OnInitialised(BthDevice Connection)
        {
            if (LogArrival(Connection))
            {
                Connection.Report += On_Report;
                Connection.Start();
            }
        }

        private void OnCompletedCount(byte Lsb, byte Msb, ushort Count)
        {
            if (Count > 0) m_Connected[new BthHandle(Lsb, Msb)].Completed();
        }

        private void On_Report(object sender, ReportEventArgs e)
        {
            if (Report != null) Report(sender, e);
        }

        #endregion

        #region Worker Threads

        private void L2CAP_DS4(BthDevice Connection, byte[] Buffer, int Transfered)
        {
            byte[] L2_DCID, L2_SCID;

            var Event = L2CAP.Code.L2CAP_Reserved;

            if (Buffer[6] == 0x01 && Buffer[7] == 0x00) // Control Channel
            {
                if (Enum.IsDefined(typeof (L2CAP.Code), Buffer[8]))
                {
                    Event = (L2CAP.Code) Buffer[8];

                    switch (Event)
                    {
                        case L2CAP.Code.L2CAP_Command_Reject:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] PSM [{2:X2}]", Event, Buffer[8], Buffer[12]);

                            L2_SCID = new byte[2] {Buffer[14], Buffer[15]};
                            L2_DCID = Connection.Set((L2CAP.PSM) Buffer[12], L2_SCID);

                            if (L2CAP.PSM.HID_Interrupt == (L2CAP.PSM) Buffer[12]) Connection.Started = true;

                            L2CAP_Connection_Response(Connection.HCI_Handle.Bytes, Buffer[9], L2_SCID, L2_DCID, 0x00);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Connection_Response,
                                (byte) L2CAP.Code.L2CAP_Connection_Response);

                            L2CAP_Configuration_Request(Connection.HCI_Handle.Bytes, m_Id++, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Request,
                                (byte) L2CAP.Code.L2CAP_Configuration_Request);
                            break;

                        case L2CAP.Code.L2CAP_Connection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}]", Event, Buffer[8], Buffer[16]);
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);

                            L2_SCID = Connection.Get_SCID(Buffer[12], Buffer[13]);

                            L2CAP_Configuration_Response(Connection.HCI_Handle.Bytes, Buffer[9], L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Response,
                                (byte) L2CAP.Code.L2CAP_Configuration_Response);

                            if (Connection.SvcStarted)
                            {
                                Connection.CanStartHid = true;
                            }
                            break;

                        case L2CAP.Code.L2CAP_Configuration_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);

                            if (Connection.Started)
                            {
                                OnInitialised(Connection);
                            }
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Request:

                            Log.DebugFormat(">> {0} [{1:X2}] Handle [{2:X2}{3:X2}]", Event, Buffer[8], Buffer[15],
                                Buffer[14]);

                            L2_SCID = new byte[2] {Buffer[14], Buffer[15]};

                            L2CAP_Disconnection_Response(Connection.HCI_Handle.Bytes, Buffer[9], L2_SCID, L2_SCID);
                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Response,
                                (byte) L2CAP.Code.L2CAP_Disconnection_Response);
                            break;

                        case L2CAP.Code.L2CAP_Disconnection_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Echo_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Echo_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Request:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        case L2CAP.Code.L2CAP_Information_Response:

                            Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[8]);
                            break;

                        default:
                            break;
                    }
                }
            }
            else if (Buffer[8] == 0xA1 && Buffer[9] == 0x11) Connection.Parse(Buffer);
            else if (Connection.InitReport(Buffer))
            {
                Connection.CanStartHid = true;
            }
        }

        private void L2CapWorker(object o)
        {
            var token = (CancellationToken) o;
            var buffer = new byte[512];

            var transfered = 0;

            Log.DebugFormat("-- Bluetooth  : L2CAP_Worker_Thread Starting [{0:X2},{1:X2}]", m_BulkIn, m_BulkOut);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (ReadBulkPipe(buffer, buffer.Length, ref transfered) && transfered > 0)
                    {
                        var connection = Get(buffer[0], buffer[1]);

                        if (connection == null)
                            continue;

                        if (connection.Model == DsModel.DS4)
                        {
                            L2CAP_DS4(connection, buffer, transfered);
                        }
                        else
                        {
                            byte[] L2_DCID;
                            byte[] L2_SCID;

                            if (buffer[6] == 0x01 && buffer[7] == 0x00) // Control Channel
                            {
                                if (Enum.IsDefined(typeof (L2CAP.Code), buffer[8]))
                                {
                                    var Event = (L2CAP.Code) buffer[8];

                                    switch (Event)
                                    {
                                        case L2CAP.Code.L2CAP_Command_Reject:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                                            break;

                                        case L2CAP.Code.L2CAP_Connection_Request:

                                            Log.DebugFormat(">> {0} [{1:X2}] PSM [{2:X2}]", Event, buffer[8], buffer[12]);

                                            L2_SCID = new byte[2] {buffer[14], buffer[15]};
                                            L2_DCID = connection.Set((L2CAP.PSM) buffer[12], L2_SCID);

                                            L2CAP_Connection_Response(connection.HCI_Handle.Bytes, buffer[9], L2_SCID,
                                                L2_DCID, 0x00);
                                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Connection_Response,
                                                (byte) L2CAP.Code.L2CAP_Connection_Response);

                                            L2CAP_Configuration_Request(connection.HCI_Handle.Bytes, m_Id++, L2_SCID);
                                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Request,
                                                (byte) L2CAP.Code.L2CAP_Configuration_Request);
                                            break;

                                        case L2CAP.Code.L2CAP_Connection_Response:

                                            Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}]", Event, buffer[8], buffer[16]);

                                            if (buffer[16] == 0) // Success
                                            {
                                                L2_SCID = new byte[2] {buffer[12], buffer[13]};
                                                L2_DCID = new byte[2] {buffer[14], buffer[15]};

                                                var DCID = (ushort) (buffer[15] << 8 | buffer[14]);

                                                connection.Set(L2CAP.PSM.HID_Service, L2_SCID[0], L2_SCID[1], DCID);

                                                L2CAP_Configuration_Request(connection.HCI_Handle.Bytes, m_Id++, L2_SCID);
                                                Log.DebugFormat("<< {0} [{1:X2}]",
                                                    L2CAP.Code.L2CAP_Configuration_Request,
                                                    (byte) L2CAP.Code.L2CAP_Configuration_Request);
                                            }
                                            break;

                                        case L2CAP.Code.L2CAP_Configuration_Request:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                                            L2_SCID = connection.Get_SCID(buffer[12], buffer[13]);

                                            L2CAP_Configuration_Response(connection.HCI_Handle.Bytes, buffer[9], L2_SCID);
                                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Configuration_Response,
                                                (byte) L2CAP.Code.L2CAP_Configuration_Response);

                                            if (connection.SvcStarted)
                                            {
                                                connection.CanStartHid = true;
                                                connection.InitReport(buffer);
                                            }
                                            break;

                                        case L2CAP.Code.L2CAP_Configuration_Response:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                                            if (connection.CanStartSvc)
                                            {
                                                if (connection.ServiceByPass)
                                                {
                                                    Log.DebugFormat(">> ServiceByPass [{0} - {1}]", connection.Local,
                                                        connection.Remote_Name);

                                                    connection.CanStartSvc = false;
                                                    OnInitialised(connection);
                                                }
                                                else
                                                {
                                                    var DCID = BthConnection.DCID++;
                                                    L2_DCID = new byte[2]
                                                    {(byte) ((DCID >> 0) & 0xFF), (byte) ((DCID >> 8) & 0xFF)};

                                                    L2CAP_Connection_Request(connection.HCI_Handle.Bytes, m_Id++,
                                                        L2_DCID,
                                                        L2CAP.PSM.HID_Service);
                                                    Log.DebugFormat("<< {0} [{1:X2}] PSM [{2:X2}]",
                                                        L2CAP.Code.L2CAP_Connection_Request,
                                                        (byte) L2CAP.Code.L2CAP_Connection_Request,
                                                        (byte) L2CAP.PSM.HID_Service);
                                                }
                                            }
                                            break;

                                        case L2CAP.Code.L2CAP_Disconnection_Request:

                                            Log.DebugFormat(">> {0} [{1:X2}] Handle [{2:X2}{3:X2}]", Event, buffer[8],
                                                buffer[15], buffer[14]);

                                            L2_SCID = new byte[2] {buffer[14], buffer[15]};

                                            L2CAP_Disconnection_Response(connection.HCI_Handle.Bytes, buffer[9], L2_SCID,
                                                L2_SCID);
                                            Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Response,
                                                (byte) L2CAP.Code.L2CAP_Disconnection_Response);
                                            break;

                                        case L2CAP.Code.L2CAP_Disconnection_Response:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);

                                            if (connection.CanStartHid)
                                            {
                                                connection.SvcStarted = false;
                                                OnInitialised(connection);
                                            }
                                            break;

                                        case L2CAP.Code.L2CAP_Echo_Request:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                                            break;

                                        case L2CAP.Code.L2CAP_Echo_Response:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                                            break;

                                        case L2CAP.Code.L2CAP_Information_Request:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                                            break;

                                        case L2CAP.Code.L2CAP_Information_Response:

                                            Log.DebugFormat(">> {0} [{1:X2}]", Event, buffer[8]);
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                            else if (buffer[8] == 0xA1 && buffer[9] == 0x01 && transfered == 58)
                                connection.Parse(buffer);
                            else if (connection.InitReport(buffer))
                            {
                                connection.CanStartHid = true;

                                L2_DCID = connection.Get_DCID(L2CAP.PSM.HID_Service);
                                L2_SCID = connection.Get_SCID(L2CAP.PSM.HID_Service);

                                L2CAP_Disconnection_Request(connection.HCI_Handle.Bytes, m_Id++, L2_SCID, L2_DCID);
                                Log.DebugFormat("<< {0} [{1:X2}]", L2CAP.Code.L2CAP_Disconnection_Request,
                                    (byte) L2CAP.Code.L2CAP_Disconnection_Request);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }
            }

            Log.Debug("-- Bluetooth  : L2CAP_Worker_Thread Exiting");
        }

        private void HicWorker(object o)
        {
            var token = (CancellationToken) o;
            var nameList = new SortedDictionary<string, string>();
            StringBuilder nm = new StringBuilder(), debug = new StringBuilder();

            var bStarted = false;
            var bd = string.Empty;

            var Buffer = new byte[512];
            var BD_Addr = new byte[6];
            var BD_Link = new byte[16];

            var Transfered = 0;
            HCI.Event Event;
            var Command = HCI.Command.HCI_Null;
            var Connection = new BthConnection();

            Log.DebugFormat("-- Bluetooth  : HCI_Worker_Thread Starting [{0:X2}]", m_IntIn);

            HCI_Reset();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (ReadIntPipe(Buffer, Buffer.Length, ref Transfered) && Transfered > 0)
                    {
                        if (Enum.IsDefined(typeof (HCI.Event), Buffer[0]))
                        {
                            Event = (HCI.Event) Buffer[0];

                            switch (Event)
                            {
                                case HCI.Event.HCI_Command_Complete_EV:

                                    Command = (HCI.Command) (ushort) (Buffer[3] | Buffer[4] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, Buffer[0], Buffer[5],
                                        Command);
                                    break;

                                case HCI.Event.HCI_Command_Status_EV:

                                    Command = (HCI.Command) (ushort) (Buffer[4] | Buffer[5] << 8);
                                    Log.DebugFormat(">> {0} [{1:X2}] [{2:X2}] [{3}]", Event, Buffer[0], Buffer[2],
                                        Command);

                                    if (Buffer[2] != 0)
                                    {
                                        switch (Command)
                                        {
                                            case HCI.Command.HCI_Write_Simple_Pairing_Mode:
                                            case HCI.Command.HCI_Write_Authentication_Enable:
                                            case HCI.Command.HCI_Set_Event_Mask:

                                                Global.DisableSSP = true;
                                                Log.Debug(
                                                    "-- Simple Pairing not supported on this device. [SSP Disabled]");
                                                Transfered = HCI_Write_Scan_Enable();
                                                break;
                                        }
                                    }
                                    break;

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:
                                    break;

                                default:
                                    Log.DebugFormat(">> {0} [{1:X2}]", Event, Buffer[0]);
                                    break;
                            }

                            switch (Event)
                            {
                                case HCI.Event.HCI_Command_Complete_EV:

                                    if (Command == HCI.Command.HCI_Reset && Buffer[5] == 0 && !bStarted)
                                    {
                                        bStarted = true;
                                        Thread.Sleep(250);

                                        Transfered = HCI_Read_BD_Addr();
                                    }

                                    if (Command == HCI.Command.HCI_Read_BD_ADDR && Buffer[5] == 0)
                                    {
                                        m_Local = new[]
                                        {Buffer[6], Buffer[7], Buffer[8], Buffer[9], Buffer[10], Buffer[11]};

                                        Transfered = HCI_Read_Buffer_Size();
                                    }

                                    if (Command == HCI.Command.HCI_Read_Buffer_Size && Buffer[5] == 0)
                                    {
                                        Log.DebugFormat("-- {0:X2}{1:X2}, {2:X2}, {3:X2}{4:X2}, {5:X2}{6:X2}", Buffer[7],
                                            Buffer[6], Buffer[8], Buffer[10], Buffer[9], Buffer[12], Buffer[11]);

                                        Transfered = HCI_Read_Local_Version_Info();
                                    }

                                    if (Command == HCI.Command.HCI_Read_Local_Version_Info && Buffer[5] == 0)
                                    {
                                        HCI_Version = string.Format("{0}.{1:X4}", Buffer[6], Buffer[8] << 8 | Buffer[7]);
                                        LMP_Version = string.Format("{0}.{1:X4}", Buffer[9],
                                            Buffer[13] << 8 | Buffer[12]);

                                        Log.DebugFormat("-- Master {0}, HCI_Version {1}, LMP_Version {2}", Local,
                                            HCI_Version, LMP_Version);

                                        if (Global.DisableSSP)
                                        {
                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                        else
                                        {
                                            Transfered = HCI_Write_Simple_Pairing_Mode();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Simple_Pairing_Mode)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Write_Simple_Pairing_Debug_Mode();
                                        }
                                        else
                                        {
                                            Global.DisableSSP = true;
                                            Log.Debug("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode)
                                    {
                                        Transfered = HCI_Write_Authentication_Enable();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Authentication_Enable)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Set_Event_Mask();
                                        }
                                        else
                                        {
                                            Global.DisableSSP = true;
                                            Log.Debug("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Set_Event_Mask)
                                    {
                                        if (Buffer[5] == 0)
                                        {
                                            Transfered = HCI_Write_Page_Timeout();
                                        }
                                        else
                                        {
                                            Global.DisableSSP = true;
                                            Log.Debug("-- Simple Pairing not supported on this device. [SSP Disabled]");

                                            Transfered = HCI_Write_Scan_Enable();
                                        }
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Timeout && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Page_Scan_Activity();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Scan_Activity && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Page_Scan_Type();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Page_Scan_Type && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Scan_Activity();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Scan_Activity && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Scan_Type();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Scan_Type && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Inquiry_Mode();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Inquiry_Mode && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Class_of_Device();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Class_of_Device && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Extended_Inquiry_Response();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Extended_Inquiry_Response && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Local_Name();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Local_Name && Buffer[5] == 0)
                                    {
                                        Transfered = HCI_Write_Scan_Enable();
                                    }

                                    if (Command == HCI.Command.HCI_Write_Scan_Enable && Buffer[5] == 0)
                                    {
                                        Initialised = true;
                                    }
                                    break;

                                case HCI.Event.HCI_Connection_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_Delete_Stored_Link_Key(BD_Addr);
                                    Transfered = HCI_Remote_Name_Request(BD_Addr);
                                    break;

                                case HCI.Event.HCI_Connection_Complete_EV:

                                    bd = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Buffer[10],
                                        Buffer[9], Buffer[8], Buffer[7], Buffer[6], Buffer[5]);

                                    if (!nameList.Any())
                                        break;

                                    Connection = Add(Buffer[3], (byte) (Buffer[4] | 0x20), nameList[bd]);

                                    if (nameList[bd].Contains("-ghic") || bd.StartsWith("00:26:5C") ||
                                        bd.StartsWith("00:16:FE:71")) Connection.ServiceByPass = true;

                                    Connection.Remote_Name = nameList[bd];
                                    nameList.Remove(bd);
                                    Connection.BD_Address = new[]
                                    {Buffer[10], Buffer[9], Buffer[8], Buffer[7], Buffer[6], Buffer[5]};
                                    break;

                                case HCI.Event.HCI_Disconnection_Complete_EV:

                                    Remove(Buffer[3], (byte) (Buffer[4] | 0x20));
                                    break;

                                case HCI.Event.HCI_Number_Of_Completed_Packets_EV:

                                    for (byte Index = 0, Ptr = 3; Index < Buffer[2]; Index++, Ptr += 4)
                                    {
                                        OnCompletedCount(Buffer[Ptr], (byte) (Buffer[Ptr + 1] | 0x20),
                                            (ushort) (Buffer[Ptr + 2] | Buffer[Ptr + 3] << 8));
                                    }
                                    break;

                                case HCI.Event.HCI_Remote_Name_Request_Complete_EV:

                                    bd = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Buffer[8], Buffer[7],
                                        Buffer[6], Buffer[5], Buffer[4], Buffer[3]);
                                    nm = new StringBuilder();

                                    for (var Index = 9; Index < Buffer.Length; Index++)
                                    {
                                        if (Buffer[Index] > 0) nm.Append((char) Buffer[Index]);
                                        else break;
                                    }

                                    var Name = nm.ToString();

                                    Log.DebugFormat("-- Remote Name : {0} - {1}", bd, Name);

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 3];

                                    if (Name.StartsWith("PLAYSTATION(R)3") || Name == "Navigation Controller" ||
                                        Name == "Wireless Controller")
                                    {
                                        nameList.Add(bd, nm.ToString());

                                        Transfered = HCI_Accept_Connection_Request(BD_Addr, 0x00);
                                    }
                                    else
                                    {
                                        Transfered = HCI_Reject_Connection_Request(BD_Addr, 0x0F);
                                    }
                                    break;

                                case HCI.Event.HCI_Link_Key_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_Link_Key_Request_Reply(BD_Addr);
                                    Transfered = HCI_Set_Connection_Encryption(Connection.HCI_Handle);
                                    break;

                                case HCI.Event.HCI_PIN_Code_Request_EV:

                                    for (var i = 0; i < 6; i++) BD_Addr[i] = Buffer[i + 2];

                                    Transfered = HCI_PIN_Code_Request_Negative_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_IO_Capability_Request_EV:

                                    Transfered = HCI_IO_Capability_Request_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_User_Confirmation_Request_EV:

                                    Transfered = HCI_User_Confirmation_Request_Reply(BD_Addr);
                                    break;

                                case HCI.Event.HCI_Link_Key_Notification_EV:

                                    for (var Index = 0; Index < 6; Index++) BD_Addr[Index] = Buffer[Index + 2];
                                    for (var Index = 0; Index < 16; Index++) BD_Link[Index] = Buffer[Index + 8];

                                    Transfered = HCI_Set_Connection_Encryption(Connection.HCI_Handle);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error in HCI_Worker_Thread: {0}", ex);
                }
            }

            Log.Debug("-- Bluetooth  : HCI_Worker_Thread Exiting");
        }

        #endregion

        #region HCI Commands

        private int HCI_Command(HCI.Command Command, byte[] Buffer)
        {
            var Transfered = 0;

            Buffer[0] = (byte) (((uint) Command >> 0) & 0xFF);
            Buffer[1] = (byte) (((uint) Command >> 8) & 0xFF);
            Buffer[2] = (byte) (Buffer.Length - 3);

            SendTransfer(0x20, 0x00, 0x0000, Buffer, ref Transfered);

            Log.DebugFormat("<< {0} [{1:X4}]", Command, (ushort) Command);
            return Transfered;
        }

        private int HCI_Accept_Connection_Request(byte[] BD_Addr, byte Role)
        {
            var Buffer = new byte[10];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = Role;

            return HCI_Command(HCI.Command.HCI_Accept_Connection_Request, Buffer);
        }

        private int HCI_Reject_Connection_Request(byte[] BD_Addr, byte Reason)
        {
            var Buffer = new byte[10];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = Reason;

            return HCI_Command(HCI.Command.HCI_Reject_Connection_Request, Buffer);
        }

        private int HCI_Remote_Name_Request(byte[] BD_Addr)
        {
            var Buffer = new byte[13];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x01;
            Buffer[10] = 0x00;
            Buffer[11] = 0x00;
            Buffer[12] = 0x00;

            return HCI_Command(HCI.Command.HCI_Remote_Name_Request, Buffer);
        }

        private int HCI_Reset()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Reset, Buffer);
        }

        private int HCI_Write_Scan_Enable()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Scan_Enable, Buffer);
        }

        private int HCI_Read_Local_Version_Info()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Local_Version_Info, Buffer);
        }

        private int HCI_Read_BD_Addr()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_BD_ADDR, Buffer);
        }

        private int HCI_Read_Buffer_Size()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Buffer_Size, Buffer);
        }


        private int HCI_Link_Key_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[25];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            for (var Index = 0; Index < Global.BD_Link.Length; Index++) Buffer[Index + 9] = Global.BD_Link[Index];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Reply, Buffer);
        }

        private int HCI_Link_Key_Request_Negative_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[9];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, Buffer);
        }

        private int HCI_PIN_Code_Request_Negative_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[16];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, Buffer);
        }

        private int HCI_Set_Connection_Encryption(BthHandle Handle)
        {
            var Buffer = new byte[6];

            Buffer[3] = Handle.Bytes[0];
            Buffer[4] = (byte) (Handle.Bytes[1] ^ 0x20);
            Buffer[5] = 0x01;

            return HCI_Command(HCI.Command.HCI_Set_Connection_Encryption, Buffer);
        }

        private int HCI_User_Confirmation_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[9];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_User_Confirmation_Request_Reply, Buffer);
        }

        private int HCI_IO_Capability_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[12];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x01;
            Buffer[10] = 0x00;
            Buffer[11] = 0x05;

            return HCI_Command(HCI.Command.HCI_IO_Capability_Request_Reply, Buffer);
        }

        private int HCI_Create_Connection(byte[] BD_Addr, byte[] Offset)
        {
            var Buffer = new byte[16];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x18;
            Buffer[10] = 0xCC;
            Buffer[11] = 0x01;
            Buffer[12] = 0x00;
            Buffer[13] = Offset[0];
            Buffer[14] = (byte) (Offset[1] | 0x80);
            Buffer[15] = 0x01;

            return HCI_Command(HCI.Command.HCI_Create_Connection, Buffer);
        }

        private int HCI_Set_Event_Mask()
        {
            var Buffer = new byte[11];
            // 00 25 5F FF FF FF FF FF
            Buffer[3] = 0xFF;
            Buffer[4] = 0xFF;
            Buffer[5] = 0xFF;
            Buffer[6] = 0xFF;
            Buffer[7] = 0xFF;
            Buffer[8] = 0x5F; // 0xFF;
            Buffer[9] = 0x25; // 0xBF;
            Buffer[10] = 0x00; // 0x3D;

            return HCI_Command(HCI.Command.HCI_Set_Event_Mask, Buffer);
        }

        private int HCI_Write_Local_Name()
        {
            var Buffer = new byte[251];

            Buffer[3] = 0x45;
            Buffer[4] = 0x4E;
            Buffer[5] = 0x54;
            Buffer[6] = 0x52;
            Buffer[7] = 0x4F;
            Buffer[8] = 0x50;
            Buffer[9] = 0x59;

            return HCI_Command(HCI.Command.HCI_Write_Local_Name, Buffer);
        }

        private int HCI_Write_Extended_Inquiry_Response()
        {
            var Buffer = new byte[244];

            Buffer[3] = 0x00;
            Buffer[4] = 0x08;
            Buffer[5] = 0x09;
            Buffer[6] = 0x45;
            Buffer[7] = 0x4E;
            Buffer[8] = 0x54;
            Buffer[9] = 0x52;
            Buffer[10] = 0x4F;
            Buffer[11] = 0x50;
            Buffer[12] = 0x59;
            Buffer[13] = 0x02;
            Buffer[14] = 0x0A;

            return HCI_Command(HCI.Command.HCI_Write_Extended_Inquiry_Response, Buffer);
        }

        private int HCI_Write_Class_of_Device()
        {
            var Buffer = new byte[6];

            Buffer[3] = 0x04;
            Buffer[4] = 0x02;
            Buffer[5] = 0x3E;

            return HCI_Command(HCI.Command.HCI_Write_Class_of_Device, Buffer);
        }

        private int HCI_Write_Inquiry_Scan_Type()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Type, Buffer);
        }

        private int HCI_Write_Inquiry_Scan_Activity()
        {
            var Buffer = new byte[7];

            Buffer[3] = 0x00;
            Buffer[4] = 0x08;
            Buffer[5] = 0x12;
            Buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Activity, Buffer);
        }

        private int HCI_Write_Page_Scan_Type()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Type, Buffer);
        }

        private int HCI_Write_Page_Scan_Activity()
        {
            var Buffer = new byte[7];

            Buffer[3] = 0x00;
            Buffer[4] = 0x04;
            Buffer[5] = 0x12;
            Buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Activity, Buffer);
        }

        private int HCI_Write_Page_Timeout()
        {
            var Buffer = new byte[5];

            Buffer[3] = 0x00;
            Buffer[4] = 0x20;

            return HCI_Command(HCI.Command.HCI_Write_Page_Timeout, Buffer);
        }

        private int HCI_Write_Authentication_Enable()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Authentication_Enable, Buffer);
        }

        private int HCI_Write_Simple_Pairing_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Mode, Buffer);
        }

        private int HCI_Write_Simple_Pairing_Debug_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode, Buffer);
        }

        private int HCI_Write_Inquiry_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Mode, Buffer);
        }

        private int HCI_Write_Inquiry_Transmit_Power_Level()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Transmit_Power_Level, Buffer);
        }

        private int HCI_Inquiry()
        {
            var Buffer = new byte[8];

            Buffer[3] = 0x33;
            Buffer[4] = 0x8B;
            Buffer[5] = 0x9E;
            Buffer[6] = 0x18;
            Buffer[7] = 0x00;

            return HCI_Command(HCI.Command.HCI_Inquiry, Buffer);
        }

        private int HCI_Inquiry_Cancel()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Inquiry_Cancel, Buffer);
        }

        private int HCI_Delete_Stored_Link_Key(byte[] BD_Addr)
        {
            var Buffer = new byte[10];

            for (var Index = 0; Index < 6; Index++) Buffer[Index + 3] = BD_Addr[Index];
            Buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Delete_Stored_Link_Key, Buffer);
        }

        private int HCI_Write_Stored_Link_Key(byte[] BD_Addr, byte[] BD_Link)
        {
            var Buffer = new byte[26];

            Buffer[3] = 0x01;
            for (var Index = 0; Index < 6; Index++) Buffer[Index + 4] = BD_Addr[Index];
            for (var Index = 0; Index < 16; Index++) Buffer[Index + 10] = BD_Link[Index];

            return HCI_Command(HCI.Command.HCI_Write_Stored_Link_Key, Buffer);
        }

        private int HCI_Read_Stored_Link_Key(byte[] BD_Addr)
        {
            var Buffer = new byte[10];

            for (var Index = 0; Index < 6; Index++) Buffer[Index + 3] = BD_Addr[Index];
            Buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Read_Stored_Link_Key, Buffer);
        }

        public int HCI_Disconnect(BthHandle Handle)
        {
            var Buffer = new byte[6];

            Buffer[3] = Handle.Bytes[0];
            Buffer[4] = (byte) (Handle.Bytes[1] ^ 0x20);
            Buffer[5] = 0x13;

            return HCI_Command(HCI.Command.HCI_Disconnect, Buffer);
        }

        #endregion

        #region L2CAP Commands

        private int L2CAP_Command(byte[] Handle, byte[] Data)
        {
            var Transfered = 0;
            var Buffer = new byte[64];

            Buffer[0] = Handle[0];
            Buffer[1] = (byte) (Handle[1] | 0x20);
            Buffer[2] = (byte) (Data.Length + 4);
            Buffer[3] = 0x00;
            Buffer[4] = (byte) (Data.Length);
            Buffer[5] = 0x00;
            Buffer[6] = 0x01;
            Buffer[7] = 0x00;

            for (var i = 0; i < Data.Length; i++) Buffer[i + 8] = Data[i];

            WriteBulkPipe(Buffer, Data.Length + 8, ref Transfered);
            return Transfered;
        }

        private int L2CAP_Connection_Request(byte[] Handle, byte Id, byte[] DCID, L2CAP.PSM Psm)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x02;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = (byte) Psm;
            Buffer[5] = 0x00;
            Buffer[6] = DCID[0];
            Buffer[7] = DCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Connection_Response(byte[] Handle, byte Id, byte[] DCID, byte[] SCID, byte Result)
        {
            var Buffer = new byte[12];

            Buffer[0] = 0x03;
            Buffer[1] = Id;
            Buffer[2] = 0x08;
            Buffer[3] = 0x00;
            Buffer[4] = SCID[0];
            Buffer[5] = SCID[1];
            Buffer[6] = DCID[0];
            Buffer[7] = DCID[1];
            Buffer[8] = Result;
            Buffer[9] = 0x00;
            Buffer[10] = 0x00;
            Buffer[11] = 0x00;

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Configuration_Request(byte[] Handle, byte Id, byte[] DCID, bool MTU = true)
        {
            var Buffer = new byte[MTU ? 12 : 8];

            Buffer[0] = 0x04;
            Buffer[1] = Id;
            Buffer[2] = (byte) (MTU ? 0x08 : 0x04);
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = 0x00;
            Buffer[7] = 0x00;

            if (MTU)
            {
                Buffer[8] = 0x01;
                Buffer[9] = 0x02;
                Buffer[10] = 0x96;
                Buffer[11] = 0x00;
            }

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Configuration_Response(byte[] Handle, byte Id, byte[] SCID)
        {
            var Buffer = new byte[10];

            Buffer[0] = 0x05;
            Buffer[1] = Id;
            Buffer[2] = 0x06;
            Buffer[3] = 0x00;
            Buffer[4] = SCID[0];
            Buffer[5] = SCID[1];
            Buffer[6] = 0x00;
            Buffer[7] = 0x00;
            Buffer[8] = 0x00;
            Buffer[9] = 0x00;

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Disconnection_Request(byte[] Handle, byte Id, byte[] DCID, byte[] SCID)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x06;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = SCID[0];
            Buffer[7] = SCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Disconnection_Response(byte[] Handle, byte Id, byte[] DCID, byte[] SCID)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x07;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = SCID[0];
            Buffer[7] = SCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        #endregion
    }
}