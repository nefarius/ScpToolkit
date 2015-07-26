using System;
using System.ComponentModel;

namespace ScpControl 
{
    public partial class BthDs3 : BthDevice 
    {
        protected Byte[] m_Report = new Byte[]
        {
            0x52, 0x01, 
            0x00, 0xFF, 0x00, 0xFF, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32, 
	        0x00, 0x00, 0x00, 0x00, 0x00, 
	        0x00, 0x00, 0x00, 0x00,	0x00, 
	        0x00, 0x00, 0x00, 0x00, 0x00, 
	        0x00, 0x00, 0x00,
        };

        protected Byte[][] m_InitReport = new Byte[][]
        {
            new Byte[] { 0x02, 0x00, 0x0F, 0x00, 0x08, 0x35, 0x03, 0x19, 0x12, 0x00, 0x00, 0x03, 0x00 },
            new Byte[] { 0x04, 0x00, 0x10, 0x00, 0x0F, 0x00, 0x01, 0x00, 0x01, 0x00, 0x10, 0x35, 0x06, 0x09, 0x02, 0x01, 0x09, 0x02, 0x02, 0x00 },
            new Byte[] { 0x06, 0x00, 0x11, 0x00, 0x0D, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x00 },
            new Byte[] { 0x06, 0x00, 0x12, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02, 0x00, 0x7F },
            new Byte[] { 0x06, 0x00, 0x13, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02, 0x00, 0x59 },
            new Byte[] { 0x06, 0x00, 0x14, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x80, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02, 0x00, 0x33 },
            new Byte[] { 0x06, 0x00, 0x15, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02, 0x00, 0x0D },
        };

        protected Byte[] m_Leds   = { 0x02, 0x04, 0x08, 0x10, };
        protected Byte[] m_Enable = { 0x53, 0xF4, 0x42, 0x03, 0x00, 0x00, };

        public override DsPadId PadId 
        {
            get { return (DsPadId) m_ControllerId; }
            set 
            {
                m_ControllerId = (Byte) value;
                m_ReportArgs.Pad = PadId;

                m_Report[11] = m_Leds[m_ControllerId];
            }
        }


        public BthDs3() 
        {
            InitializeComponent();
        }

        public BthDs3(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthDs3(IBthDevice Device, Byte[] Master, Byte Lsb, Byte Msb) : base(Device, Master, Lsb, Msb) 
        {
        }


        public override Boolean Start() 
        {
            CanStartHid = false;
            m_State = DsState.Connected;

            if (Local.StartsWith("00:26:5C"))   // Fix up for Fake DS3
            {
                m_Enable[0] = 0xA3;

                m_Report[0] = 0xA2;
                m_Report[3] = 0x00;
                m_Report[5] = 0x00;
            }

            if (Remote_Name.EndsWith("-ghic"))   // Fix up for Fake DS3
            {
                m_Report[3] = 0x00;
                m_Report[5] = 0x00;
            }

            m_Queued = 1; m_Blocked = true; m_Last = DateTime.Now;
            m_Device.HID_Command(HCI_Handle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), m_Enable);

            return base.Start();
        }


        public override void Parse(Byte[] Report) 
        {
            if (Report[10] == 0xFF) return;

            m_PlugStatus    = Report[38];
            m_BatteryStatus = Report[39];
            m_CableStatus   = Report[40];

            if (m_Packet == 0) Rumble(0, 0); m_Packet++;

            m_ReportArgs.Report[2] = m_BatteryStatus;

            m_ReportArgs.Report[4] = (Byte)(m_Packet >>  0 & 0xFF);
            m_ReportArgs.Report[5] = (Byte)(m_Packet >>  8 & 0xFF);
            m_ReportArgs.Report[6] = (Byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (Byte)(m_Packet >> 24 & 0xFF);

            Ds3Button Buttons = (Ds3Button)((Report[11] << 0) | (Report[12] << 8) | (Report[13] << 16) | (Report[14] << 24));
            Boolean   Trigger = false, Active = false;

            // Quick Disconnect
            if ((Buttons & Ds3Button.L1) == Ds3Button.L1
             && (Buttons & Ds3Button.R1) == Ds3Button.R1
             && (Buttons & Ds3Button.PS) == Ds3Button.PS
            )
            {
                Trigger = true; Report[13] ^= 0x1;
            }

            for (Int32 Index = 8; Index < 57; Index++)
            {
                m_ReportArgs.Report[Index] = Report[Index + 1];
            }

            // Buttons
            for (Int32 Index = 11; Index < 15 && !Active; Index++)
            {
                if (Report[Index] != 0) Active = true;
            }

            // Axis
            for (Int32 Index = 15; Index < 19 && !Active; Index++)
            {
                if (Report[Index] < 117 || Report[Index] > 137) Active = true;
            }

            // Triggers & Pressure
            for (Int32 Index = 23; Index < 35 && !Active; Index++)
            {
                if (Report[Index] != 0) Active = true;
            }

            if (Active)
            {
                m_IsIdle = false;
            }
            else if (!m_IsIdle)
            {
                m_IsIdle = true; m_Idle = DateTime.Now;
            }

            if (Trigger && !m_IsDisconnect)
            {
                m_IsDisconnect = true; m_Disconnect = DateTime.Now;
            }
            else if (!Trigger && m_IsDisconnect)
            {
                m_IsDisconnect = false;
            }

            Publish();
        }

        public override Boolean Rumble(Byte large, Byte small) 
        {
            lock (this)
            {
                if (Global.DisableRumble)
                {
                    m_Report[4] = 0;
                    m_Report[6] = 0;
                }
                else
                {
                    m_Report[4] = (Byte)(small > 0 ? 0x01 : 0x00);
                    m_Report[6] = large;
                }

                if (!m_Blocked && Global.Latency == 0)
                {
                    m_Last = DateTime.Now; m_Blocked = true;

                    m_Device.HID_Command(HCI_Handle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), m_Report);
                }
                else
                {
                    m_Queued = 1;
                }
            }
            return true;
        }

        public override Boolean InitReport(Byte[] Report) 
        {
            Boolean retVal = false;

            if (m_Init < m_InitReport.Length)
            {
                m_Device.HID_Command(HCI_Handle.Bytes, Get_SCID(L2CAP.PSM.HID_Service), m_InitReport[m_Init++]);
            }
            else if (m_Init == m_InitReport.Length)
            {
                m_Init++; retVal = true;
            }

            return retVal;
        }


        protected override void Process(DateTime Now) 
        {
            lock (this)
            {
                if (m_State == DsState.Connected)
                {
                    if ((Now - m_Tick).TotalMilliseconds >= 500 && m_Packet > 0)
                    {
                        m_Tick = Now;

                        if (m_Queued == 0) m_Queued = 1;

                        if (Battery < DsBattery.Medium)
                        {
                            m_Report[11] ^= m_Leds[m_ControllerId];
                        }
                        else
                        {
                            m_Report[11] |= m_Leds[m_ControllerId];
                        }
                    }

                    if (Global.DisableLED) m_Report[11] = 0;

                    if (!m_Blocked && m_Queued > 0)
                    {
                        if ((Now - m_Last).TotalMilliseconds >= Global.Latency)
                        {
                            m_Last = Now; m_Blocked = true; m_Queued--;

                            m_Device.HID_Command(HCI_Handle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), m_Report);
                        }
                    }
                }
            }
        }
    }
}
