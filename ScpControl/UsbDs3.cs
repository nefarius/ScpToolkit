using System;
using System.ComponentModel;
using System.Threading;

namespace ScpControl 
{
    public partial class UsbDs3 : UsbDevice 
    {
        public static String USB_CLASS_GUID = "{E2824A09-DBAA-4407-85CA-C8E8FF5F6FFA}";

        protected Byte[] m_Leds   = { 0x02, 0x04, 0x08, 0x10 };
        protected Byte[] m_Enable = { 0x42, 0x0C, 0x00, 0x00 };
        protected Byte[] m_Report = 
        {
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

        public override DsPadId PadId 
        {
            get { return (DsPadId) m_ControllerId; }
            set 
            {
                m_ControllerId = (Byte) value;
                m_ReportArgs.Pad = PadId;

                m_Report[9] = m_Leds[m_ControllerId];
            }
        }


        public UsbDs3() : base(USB_CLASS_GUID) 
        {
            InitializeComponent();
        }

        public UsbDs3(IContainer container) : base(USB_CLASS_GUID) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public override Boolean Open(String DevicePath) 
        {
            if (base.Open(DevicePath))
            {
                m_State = DsState.Reserved;
                GetDeviceInstance(ref m_Instance);

                Int32 Transfered = 0;

                if (SendTransfer(0xA1, 0x01, 0x03F5, m_Buffer, ref Transfered))
                {
                    m_Master = new Byte[] { m_Buffer[2], m_Buffer[3], m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7] };
                }

                if (SendTransfer(0xA1, 0x01, 0x03F2, m_Buffer, ref Transfered))
                {
                    m_Local = new Byte[] { m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7], m_Buffer[8], m_Buffer[9] };
                }

                m_Mac = String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2], m_Local[3], m_Local[4], m_Local[5]);
            }

            return State == DsState.Reserved;
        }

        public override Boolean Start() 
        {
            m_Model = (Byte) DsModel.DS3;

            if (IsActive)
            {
                Int32 Transfered = 0;

                if (SendTransfer(0x21, 0x09, 0x03F4, m_Enable, ref Transfered))
                {
                    base.Start();
                }
            }

            return State == DsState.Connected;
        }


        public override Boolean Rumble(Byte Large, Byte Small) 
        {
            lock (this)
            {
                Int32 Transfered = 0;

                if (Global.DisableRumble)
                {
                    m_Report[2] = 0;
                    m_Report[4] = 0;
                }
                else
                {
                    m_Report[2] = (Byte)(Small > 0 ? 0x01 : 0x00);
                    m_Report[4] = (Byte)(Large);
                }

                m_Report[9] = (Byte)(Global.DisableLED ? 0 : m_Leds[m_ControllerId]);

                return SendTransfer(0x21, 0x09, 0x0201, m_Report, ref Transfered);
            }
        }

        public override Boolean Pair(Byte[] Master) 
        {
            Int32 Transfered = 0; Byte[] Buffer = { 0x00, 0x00, Master[0], Master[1], Master[2], Master[3], Master[4], Master[5] };

            if (SendTransfer(0x21, 0x09, 0x03F5, Buffer, ref Transfered))
            {
                for (Int32 Index = 0; Index < m_Master.Length; Index++)
                {
                    m_Master[Index] = Master[Index];
                }

                LogDebug(String.Format("++ Paired DS3 [{0}] To BTH Dongle [{1}]", Local, Remote));
                return true;
            }

            LogDebug(String.Format("++ Pair Failed [{0}]", Local));
            return false;
        }


        protected override void Parse(Byte[] Report) 
        {
            if (Report[0] != 0x01) return;

            m_Packet++;

            m_ReportArgs.Report[2] = m_BatteryStatus = Report[30];

            m_ReportArgs.Report[4] = (Byte)(m_Packet >>  0 & 0xFF);
            m_ReportArgs.Report[5] = (Byte)(m_Packet >>  8 & 0xFF);
            m_ReportArgs.Report[6] = (Byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (Byte)(m_Packet >> 24 & 0xFF);

            Ds3Button Buttons = (Ds3Button)((Report[2] << 0) | (Report[3] << 8) | (Report[4] << 16) | (Report[5] << 24));
            Boolean   Trigger = false;

            if ((Buttons & Ds3Button.L1) == Ds3Button.L1 
             && (Buttons & Ds3Button.R1) == Ds3Button.R1 
             && (Buttons & Ds3Button.PS) == Ds3Button.PS
            )
            {
                Trigger = true; Report[4] ^= 0x1;
            }

            for (int Index = 8; Index < 57; Index++)
            {
                m_ReportArgs.Report[Index] = Report[Index - 8];
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

        protected override void Process(DateTime Now) 
        {
            lock (this)
            {
               if (m_IsDisconnect)
                {
                    if ((Now - m_Disconnect).TotalMilliseconds >= 2000)
                    {
                        LogDebug("++ Quick Disconnect Triggered");

                        Shutdown();
                        return;
                    }
                }

               if ((Now - m_Last).TotalMilliseconds >= 1500 && m_Packet > 0)
                {
                    Int32 Transfered = 0;

                    m_Last = Now;

                    if (Battery == DsBattery.Charging)
                    {
                        m_Report[9] ^= m_Leds[m_ControllerId];
                    }
                    else
                    {
                        m_Report[9] |= m_Leds[m_ControllerId];
                    }

                    if (Global.DisableLED) m_Report[9] = 0;

                    SendTransfer(0x21, 0x09, 0x0201, m_Report, ref Transfered);
                }
            }
        }
    }
}
