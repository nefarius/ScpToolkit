using System;
using System.ComponentModel;
using ScpControl.ScpCore;

namespace ScpControl
{
    public sealed partial class UsbDs4 : UsbDevice
    {
        private const int R = 6; // Led Offsets
        private const int G = 7; // Led Offsets
        private const int B = 8; // Led Offsets
        public static string USB_CLASS_GUID = "{2ED90CE1-376F-4982-8F7F-E056CBC3CA71}";
        private byte m_Brightness = GlobalConfiguration.Instance.Brightness;
        private bool m_DisableLightBar;

        private byte[] m_Report =
        {
            0x05,
            0xFF, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFF, 0xFF, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00
        };

        public UsbDs4() : base(USB_CLASS_GUID)
        {
            InitializeComponent();
        }

        public UsbDs4(IContainer container) : base(USB_CLASS_GUID)
        {
            container.Add(this);

            InitializeComponent();
        }

        public override DsPadId PadId
        {
            get { return (DsPadId) m_ControllerId; }
            set
            {
                m_ControllerId = (byte) value;
                m_ReportArgs.Pad = PadId;

                switch (value)
                {
                    case DsPadId.One: // Blue
                        m_Report[R] = 0x00;
                        m_Report[G] = 0x00;
                        m_Report[B] = m_Brightness;
                        break;
                    case DsPadId.Two: // Green
                        m_Report[R] = 0x00;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = 0x00;
                        break;
                    case DsPadId.Three: // Yellow
                        m_Report[R] = m_Brightness;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = 0x00;
                        break;
                    case DsPadId.Four: // Cyan
                        m_Report[R] = 0x00;
                        m_Report[G] = m_Brightness;
                        m_Report[B] = m_Brightness;
                        break;
                    case DsPadId.None: // Red
                        m_Report[R] = m_Brightness;
                        m_Report[G] = 0x00;
                        m_Report[B] = 0x00;
                        break;
                }

                if (GlobalConfiguration.Instance.DisableLightBar)
                {
                    m_Report[R] = m_Report[G] = m_Report[B] = m_Report[12] = m_Report[13] = 0x00;
                }
            }
        }

        private byte MapBattery(byte Value)
        {
            var mapped = (byte) DsBattery.None;

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
                    mapped = (byte) DsBattery.Charging;
                    break;
                case 0x1B:
                    mapped = (byte) DsBattery.Charged;
                    break;
            }

            return mapped;
        }

        public override bool Open(string devicePath)
        {
            if (base.Open(devicePath))
            {
                m_State = DsState.Reserved;
                GetDeviceInstance(ref m_Instance);

                var Transfered = 0;

                if (SendTransfer(0xA1, 0x01, 0x0312, m_Buffer, ref Transfered))
                {
                    m_Master = new[]
                    {m_Buffer[15], m_Buffer[14], m_Buffer[13], m_Buffer[12], m_Buffer[11], m_Buffer[10]};
                    m_Local = new[] {m_Buffer[6], m_Buffer[5], m_Buffer[4], m_Buffer[3], m_Buffer[2], m_Buffer[1]};
                }

                m_Mac = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            m_Model = (byte) DsModel.DS4;

            if (GlobalConfiguration.Instance.Repair)
            {
                var transfered = 0;
                byte[] buffer =
                {
                    0x13, m_Master[5], m_Master[4], m_Master[3], m_Master[2], m_Master[1], m_Master[0],
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                Array.Copy(GlobalConfiguration.Instance.BdLink, 0, buffer, 7, GlobalConfiguration.Instance.BdLink.Length);

                if (SendTransfer(0x21, 0x09, 0x0313, buffer, ref transfered))
                {
                    Log.DebugFormat("++ Repaired DS4 [{0}] Link Key For BTH Dongle [{1}]", Local, Remote);
                }
                else
                {
                    Log.DebugFormat("++ Repair DS4 [{0}] Link Key For BTH Dongle [{1}] Failed!", Local, Remote);
                }
            }

            return base.Start();
        }

        public override bool Rumble(byte large, byte small)
        {
            lock (this)
            {
                var transfered = 0;

                m_Report[4] = small;
                m_Report[5] = large;

                return WriteIntPipe(m_Report, m_Report.Length, ref transfered);
            }
        }

        public override bool Pair(byte[] master)
        {
            var transfered = 0;
            byte[] buffer =
            {
                0x13, master[5], master[4], master[3], master[2], master[1], master[0], 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            Array.Copy(GlobalConfiguration.Instance.BdLink, 0, buffer, 7, GlobalConfiguration.Instance.BdLink.Length);

            if (SendTransfer(0x21, 0x09, 0x0313, buffer, ref transfered))
            {
                for (var Index = 0; Index < m_Master.Length; Index++)
                {
                    m_Master[Index] = master[Index];
                }

                Log.DebugFormat("++ Paired DS4 [{0}] To BTH Dongle [{1}]", Local, Remote);
                return true;
            }

            Log.DebugFormat("++ Pair Failed [{0}]", Local);
            return false;
        }

        protected override void Parse(byte[] Report)
        {
            if (Report[0] != 0x01) return;

            m_Packet++;

            m_ReportArgs.Report[2] = m_BatteryStatus = MapBattery(Report[30]);

            m_ReportArgs.Report[4] = (byte) (m_Packet >> 0 & 0xFF);
            m_ReportArgs.Report[5] = (byte) (m_Packet >> 8 & 0xFF);
            m_ReportArgs.Report[6] = (byte) (m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (byte) (m_Packet >> 24 & 0xFF);

            var buttons = (Ds4Button) ((Report[5] << 0) | (Report[6] << 8) | (Report[7] << 16));

            //++ Convert HAT to DPAD
            Report[5] &= 0xF0;

            switch ((uint) buttons & 0xF)
            {
                case 0:
                    Report[5] |= (byte) (Ds4Button.Up);
                    break;
                case 1:
                    Report[5] |= (byte) (Ds4Button.Up | Ds4Button.Right);
                    break;
                case 2:
                    Report[5] |= (byte) (Ds4Button.Right);
                    break;
                case 3:
                    Report[5] |= (byte) (Ds4Button.Right | Ds4Button.Down);
                    break;
                case 4:
                    Report[5] |= (byte) (Ds4Button.Down);
                    break;
                case 5:
                    Report[5] |= (byte) (Ds4Button.Down | Ds4Button.Left);
                    break;
                case 6:
                    Report[5] |= (byte) (Ds4Button.Left);
                    break;
                case 7:
                    Report[5] |= (byte) (Ds4Button.Left | Ds4Button.Up);
                    break;
            }
            //--

            for (var index = 8; index < 72; index++)
            {
                m_ReportArgs.Report[index] = Report[index - 8];
            }

            Publish();
        }

        protected override void Process(DateTime now)
        {
            lock (this)
            {
                if ((now - m_Last).TotalMilliseconds >= 500)
                {
                    var transfered = 0;

                    m_Last = now;

                    if (!GlobalConfiguration.Instance.DisableLightBar)
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

                    if (GlobalConfiguration.Instance.Brightness != m_Brightness)
                    {
                        m_Brightness = GlobalConfiguration.Instance.Brightness;
                        PadId = PadId;
                    }

                    if (GlobalConfiguration.Instance.DisableLightBar != m_DisableLightBar)
                    {
                        m_DisableLightBar = GlobalConfiguration.Instance.DisableLightBar;
                        PadId = PadId;
                    }

                    WriteIntPipe(m_Report, m_Report.Length, ref transfered);
                }
            }
        }
    }
}