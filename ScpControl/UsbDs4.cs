using System;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace ScpControl 
{
    public partial class UsbDs4 : UsbDevice 
    {
        public static String USB_CLASS_GUID = "{2ED90CE1-376F-4982-8F7F-E056CBC3CA71}";

        protected Boolean m_DisableLightBar = false;
        protected Byte    m_Brightness = Global.Brightness;

        protected const Int32 R = 6, G = 7, B = 8;  // Led Offsets

        protected Byte[] m_Report = 
        {
            0x05,
            0xFF, 0x00, 0x00, 0x00, 0x00, 
            0xFF, 0xFF, 0xFF, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 
	        0x00, 
        };

        protected virtual Byte MapBattery(Byte Value) 
        {
            Byte Mapped = (Byte) DsBattery.None;

            switch (Value)
            {
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1A:
                    Mapped = (Byte) DsBattery.Charging;
                    break;
                case 0x1B:
                    Mapped = (Byte) DsBattery.Charged;
                    break;
            }

            return Mapped;
        }

        public override DsPadId PadId 
        {
            get { return (DsPadId) m_ControllerId; }
            set 
            {
                m_ControllerId = (Byte) value;
                m_ReportArgs.Pad = PadId;

                switch (value)
                {
                    case DsPadId.One:      // Blue
                        m_Report[R] = 0x00;
                        m_Report[G] = 0x00;
                        m_Report[B] = m_Brightness;
                        break;
                    case DsPadId.Two:      // Green
                        m_Report[R] = 0x00;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = 0x00;
                        break;
                    case DsPadId.Three:    // Yellow
                        m_Report[R] = m_Brightness;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = 0x00;
                        break;
                    case DsPadId.Four:     // Cyan
                        m_Report[R] = 0x00;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = m_Brightness;
                        break;
                    case DsPadId.None:     // Red
                        m_Report[R] = m_Brightness;
                        m_Report[G] = 0x00;
                        m_Report[B] = 0x00;
                        break;
                }

                if (Global.DisableLightBar)
                {
                    m_Report[R] = m_Report[G] = m_Report[B] = m_Report[12] = m_Report[13] = 0x00;
                }
            }
        }


        public UsbDs4() : base(USB_CLASS_GUID) 
        {
            InitializeComponent();
        }

        public UsbDs4(IContainer container) : base(USB_CLASS_GUID) 
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

                if (SendTransfer(0xA1, 0x01, 0x0312, m_Buffer, ref Transfered))
                {
                    m_Master = new Byte[] { m_Buffer[15], m_Buffer[14], m_Buffer[13], m_Buffer[12], m_Buffer[11], m_Buffer[10] };
                    m_Local  = new Byte[] { m_Buffer[ 6], m_Buffer[ 5], m_Buffer[ 4], m_Buffer[ 3], m_Buffer[ 2], m_Buffer[ 1] };
                }

                m_Mac = String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2], m_Local[3], m_Local[4], m_Local[5]);
            }

            return State == DsState.Reserved;
        }

        public override Boolean Start() 
        {
            m_Model = (Byte) DsModel.DS4;

            if (Global.Repair)
            {
                Int32  Transfered = 0;
                Byte[] Buffer = { 0x13, m_Master[5], m_Master[4], m_Master[3], m_Master[2], m_Master[1], m_Master[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                Array.Copy(Global.BD_Link, 0, Buffer, 7, Global.BD_Link.Length);

                if (SendTransfer(0x21, 0x09, 0x0313, Buffer, ref Transfered))
                {
                    LogDebug(String.Format("++ Repaired DS4 [{0}] Link Key For BTH Dongle [{1}]", Local, Remote));
                }
                else
                {
                    LogDebug(String.Format("++ Repair DS4 [{0}] Link Key For BTH Dongle [{1}] Failed!", Local, Remote));
                }
            }

            return base.Start();
        }


        public override Boolean Rumble(Byte Large, Byte Small) 
        {
            lock (this)
            {
                Int32 Transfered = 0;

                m_Report[4] = (Byte)(Small);
                m_Report[5] = (Byte)(Large);

                return WriteIntPipe(m_Report, m_Report.Length, ref Transfered);
            }
        }

        public override Boolean Pair(Byte[] Master) 
        {
            Int32 Transfered = 0;
            Byte[] Buffer = { 0x13, Master[5], Master[4], Master[3], Master[2], Master[1], Master[0], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            Array.Copy(Global.BD_Link, 0, Buffer, 7, Global.BD_Link.Length);

            if (SendTransfer(0x21, 0x09, 0x0313, Buffer, ref Transfered))
            {
                for (Int32 Index = 0; Index < m_Master.Length; Index++)
                {
                    m_Master[Index] = Master[Index];
                }

                LogDebug(String.Format("++ Paired DS4 [{0}] To BTH Dongle [{1}]", Local, Remote));
                return true;
            }

            LogDebug(String.Format("++ Pair Failed [{0}]", Local));
            return false;
        }


        protected override void Parse(Byte[] Report) 
        {
            if (Report[0] != 0x01) return;

            m_Packet++;

            m_ReportArgs.Report[2] = m_BatteryStatus = MapBattery(Report[30]);

            m_ReportArgs.Report[4] = (Byte)(m_Packet >>  0 & 0xFF);
            m_ReportArgs.Report[5] = (Byte)(m_Packet >>  8 & 0xFF);
            m_ReportArgs.Report[6] = (Byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (Byte)(m_Packet >> 24 & 0xFF);

            Ds4Button Buttons = (Ds4Button)((Report[5] << 0) | (Report[6] << 8) | (Report[7] << 16));

            //++ Convert HAT to DPAD
            Report[5] &= 0xF0;

            switch ((UInt32) Buttons & 0xF) 
            {
                case 0:
                    Report[5] |= (Byte)(Ds4Button.Up);
                    break;
                case 1:
                    Report[5] |= (Byte)(Ds4Button.Up | Ds4Button.Right);
                    break;
                case 2:
                    Report[5] |= (Byte)(Ds4Button.Right);
                    break;
                case 3:
                    Report[5] |= (Byte)(Ds4Button.Right | Ds4Button.Down);
                    break;
                case 4:
                    Report[5] |= (Byte)(Ds4Button.Down);
                    break;
                case 5:
                    Report[5] |= (Byte)(Ds4Button.Down | Ds4Button.Left);
                    break;
                case 6:
                    Report[5] |= (Byte)(Ds4Button.Left);
                    break;
                case 7:
                    Report[5] |= (Byte)(Ds4Button.Left | Ds4Button.Up);
                    break;
            }
            //--

            for (int Index = 8; Index < 72; Index++)
            {
                m_ReportArgs.Report[Index] = Report[Index - 8];
            }

            Publish();
        }

        protected override void Process(DateTime Now) 
        {
            lock (this)
            {
                if ((Now - m_Last).TotalMilliseconds >= 500)
                {
                    Int32 Transfered = 0;

                    m_Last = Now;

                    if (!Global.DisableLightBar)
                    {
                        if (Battery != DsBattery.Charged)
                        {
                            m_Report[9] = m_Report[10] = 0x80;
                        }
                        else
                        {
                            m_Report[9] = m_Report[10] = 0x00;
                        }
                    }

                    if (Global.Brightness != m_Brightness)
                    {
                        m_Brightness = Global.Brightness;
                        PadId = PadId;
                    }

                    if (Global.DisableLightBar != m_DisableLightBar)
                    {
                        m_DisableLightBar = Global.DisableLightBar;
                        PadId = PadId;
                    }

                    WriteIntPipe(m_Report, m_Report.Length, ref Transfered);
                }
            }
        }
   }
}
