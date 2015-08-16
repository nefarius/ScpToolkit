using System;
using System.ComponentModel;

namespace ScpControl
{
    public partial class UsbDevice : ScpDevice, IDsDevice
    {
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
        protected uint m_Packet;
        protected byte m_PlugStatus = 0;
        protected bool m_Publish = false;
        protected ReportEventArgs m_ReportArgs = new ReportEventArgs();
        protected DsState m_State = DsState.Disconnected;

        protected UsbDevice(string Guid) : base(Guid)
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

                m_ReportArgs.Pad = PadId;
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

        public override bool Start()
        {
            if (IsActive)
            {
                Array.Copy(m_Local, 0, m_ReportArgs.Report, (int) DsOffset.Address, m_Local.Length);

                m_ReportArgs.Report[(int) DsOffset.Connection] = (byte) Connection;
                m_ReportArgs.Report[(int) DsOffset.Model] = (byte) Model;

                m_State = DsState.Connected;
                m_Packet = 0;

                HID_Worker.RunWorkerAsync();
                tmUpdate.Enabled = true;

                Rumble(0, 0);
                Log.DebugFormat("-- Started Device Instance [{0}] Local [{1}] Remote [{2}]", m_Instance, Local, Remote);
            }

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

        public event EventHandler<ReportEventArgs> Report;

        protected virtual void Publish()
        {
            m_ReportArgs.Report[0] = m_ControllerId;
            m_ReportArgs.Report[1] = (byte) m_State;

            if (Report != null) Report(this, m_ReportArgs);
        }

        protected virtual void Process(DateTime now)
        {
        }

        protected virtual void Parse(byte[] Report)
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

                Publish();
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

                Publish();
            }

            return !IsActive;
        }

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
                        m_Packet,
                        Battery
                        );
            }

            throw new Exception();
        }

        private void HID_Worker_Thread(object sender, DoWorkEventArgs e)
        {
            var transfered = 0;
            var buffer = new byte[64];

            Log.Debug("-- USB Device : HID_Worker_Thread Starting");

            while (IsActive)
            {
                try
                {
                    if (ReadIntPipe(buffer, buffer.Length, ref transfered) && transfered > 0)
                    {
                        if (transfered == 27)
                        {
                            ConvertAfterglowToValidBytes(ref buffer);
                        }

                        Parse(buffer);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }
            }

            Log.Debug("-- USB Device : HID_Worker_Thread Exiting");
        }

        /// <summary>
        ///     Prototype helper method to convert input byte stream of Afterglow Wireless Controllers to valid PS3 packet.
        /// </summary>
        /// <param name="b">The input byte stream captured from the controller.</param>
        private static void ConvertAfterglowToValidBytes(ref byte[] b)
        {
            // identify the fake controller
            // TODO: add more checks
            if (b[26] != 0x02) return;

            // prepare temporary array for input values
            var input = new byte[28];

            // copy source array and zero out bytes
            for (int i = 0; i < 27; i++)
            {
                input[i] = b[i];
                b[i] = 0x00;
            }

            b[4] |= (byte)((input[1] >> 4) & 1); // PS
            b[2] |= (byte)((input[1] >> 0) & 1); // Select
            b[2] |= (byte)(((input[1] >> 1) & 1) << 3); // Start

            b[3] |= (byte)(((input[0] >> 4) & 1) << 2); // L1 (button)
            b[20] = input[15]; // L1 (analog)

            b[3] |= (byte)(((input[0] >> 5) & 1) << 3); // R1 (button)
            b[21] = input[16]; // R1 (analog)

            b[3] |= (byte)(((input[0] >> 6) & 1) << 0); // L2 (button)
            b[18] = input[17]; // L2 (analog)

            b[3] |= (byte)(((input[0] >> 7) & 1) << 1); // R2 (button)
            b[19] = input[18]; // R2 (analog)

            b[3] |= (byte)(((input[0] >> 3) & 1) << 4); // Triangle (button)
            b[22] = input[11]; // Triangle (analog)

            b[3] |= (byte)(((input[0] >> 2) & 1) << 5); // Circle (button)
            b[23] = input[12]; // Circle (analog)

            b[3] |= (byte)(((input[0] >> 1) & 1) << 6); // Cross (button)
            b[24] = input[13]; // Cross (analog)

            b[3] |= (byte)(((input[0] >> 0) & 1) << 7); // Square (button)
            b[25] = input[14]; // Square (analog)

            if (input[2] != 0x0F)
            {
                b[2] |= (byte)((input[2] == 0x02) ? 0x20 : 0x00); // D-Pad right
                b[15] = input[7]; // D-Pad right

                b[2] |= (byte)((input[2] == 0x06) ? 0x80 : 0x00); // D-Pad left
                b[17] = input[8]; // D-Pad left

                b[2] |= (byte)((input[2] == 0x00) ? 0x10 : 0x00); // D-Pad up
                b[14] = input[9]; // D-Pad up

                b[2] |= (byte)((input[2] == 0x04) ? 0x40 : 0x00); // D-Pad down
                b[16] = input[10]; // D-Pad down
            }

            b[7] = input[4]; // Left Axis Y+
            b[7] = input[4]; // Left Axis Y-
            b[6] = input[3]; // Left Axis X-
            b[6] = input[3]; // Left Axis X+

            b[9] = input[6]; // Right Axis Y+
            b[9] = input[6]; // Right Axis Y-
            b[8] = input[5]; // Right Axis X-
            b[8] = input[5]; // Right Axis X+

            b[2] |= (byte)(((input[1] >> 2) & 1) << 1); // Left Thumb
            b[2] |= (byte)(((input[1] >> 3) & 1) << 2); // Right Thumb

            b[0] = 0x01;
        }

        private void On_Timer(object sender, EventArgs e)
        {
            lock (this)
            {
                Process(DateTime.Now);
            }
        }
    }
}