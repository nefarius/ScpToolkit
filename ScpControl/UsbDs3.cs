using System;
using System.ComponentModel;
using ScpControl.ScpCore;

namespace ScpControl
{
    public partial class UsbDs3 : UsbDevice
    {
        public static string USB_CLASS_GUID = "{E2824A09-DBAA-4407-85CA-C8E8FF5F6FFA}";
        private byte[] m_Enable = { 0x42, 0x0C, 0x00, 0x00 };
        private byte[] m_Leds = { 0x02, 0x04, 0x08, 0x10 };

        private byte[] m_Report =
        {
            0x00, 0xFF, 0x00, 0xFF, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0xFF, 0x27, 0x10, 0x00, 0x32,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00
        };

        public UsbDs3()
            : base(USB_CLASS_GUID)
        {
            InitializeComponent();
        }

        public UsbDs3(IContainer container)
            : base(USB_CLASS_GUID)
        {
            container.Add(this);

            InitializeComponent();
        }

        public override DsPadId PadId
        {
            get { return (DsPadId)m_ControllerId; }
            set
            {
                m_ControllerId = (byte)value;
                m_ReportArgs.Pad = PadId;

                m_Report[9] = m_Leds[m_ControllerId];
            }
        }

        public override bool Open(string devicePath)
        {
            if (base.Open(devicePath))
            {
                m_State = DsState.Reserved;
                GetDeviceInstance(ref m_Instance);

                var transfered = 0;

                if (SendTransfer(0xA1, 0x01, 0x03F5, m_Buffer, ref transfered))
                {
                    m_Master = new[] { m_Buffer[2], m_Buffer[3], m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7] };
                }

                if (SendTransfer(0xA1, 0x01, 0x03F2, m_Buffer, ref transfered))
                {
                    m_Local = new[] { m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7], m_Buffer[8], m_Buffer[9] };
                }

                m_Mac = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
                Log.DebugFormat("MAC = {0}", m_Mac);
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            m_Model = (byte)DsModel.DS3;

            if (IsActive)
            {
                var transfered = 0;

                if (SendTransfer(0x21, 0x09, 0x03F4, m_Enable, ref transfered))
                {
                    base.Start();
                }
            }

            return State == DsState.Connected;
        }

        public override bool Rumble(byte large, byte small)
        {
            lock (this)
            {
                var transfered = 0;

                if (GlobalConfiguration.Instance.DisableRumble)
                {
                    m_Report[2] = 0;
                    m_Report[4] = 0;
                }
                else
                {
                    m_Report[2] = (byte)(small > 0 ? 0x01 : 0x00);
                    m_Report[4] = large;
                }

                m_Report[9] = (byte)(GlobalConfiguration.Instance.DisableLED ? 0 : m_Leds[m_ControllerId]);

                return SendTransfer(0x21, 0x09, 0x0201, m_Report, ref transfered);
            }
        }

        public override bool Pair(byte[] master)
        {
            var transfered = 0;
            byte[] buffer = { 0x00, 0x00, master[0], master[1], master[2], master[3], master[4], master[5] };

            if (SendTransfer(0x21, 0x09, 0x03F5, buffer, ref transfered))
            {
                for (var index = 0; index < m_Master.Length; index++)
                {
                    m_Master[index] = master[index];
                }

                Log.DebugFormat("++ Paired DS3 [{0}] To BTH Dongle [{1}]", Local, Remote);
                return true;
            }

            Log.DebugFormat("++ Pair Failed [{0}]", Local);
            return false;
        }

        protected override void Parse(byte[] Report)
        {
            if (Report[0] != 0x01) return;

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            m_ReportArgs.Report[2] = m_BatteryStatus = Report[30];

            m_ReportArgs.Report[4] = (byte)(m_Packet >> 0 & 0xFF);
            m_ReportArgs.Report[5] = (byte)(m_Packet >> 8 & 0xFF);
            m_ReportArgs.Report[6] = (byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (byte)(m_Packet >> 24 & 0xFF);

            var buttons = (Ds3Button)((Report[2] << 0) | (Report[3] << 8) | (Report[4] << 16) | (Report[5] << 24));
            var trigger = false;

            if ((buttons & Ds3Button.L1) == Ds3Button.L1
                && (buttons & Ds3Button.R1) == Ds3Button.R1
                && (buttons & Ds3Button.PS) == Ds3Button.PS
                )
            {
                trigger = true;
                Report[4] ^= 0x1;
            }

            for (var index = 8; index < 57; index++)
            {
                m_ReportArgs.Report[index] = Report[index - 8];
            }

            if (trigger && !m_IsDisconnect)
            {
                m_IsDisconnect = true;
                m_Disconnect = DateTime.Now;
            }
            else if (!trigger && m_IsDisconnect)
            {
                m_IsDisconnect = false;
            }

            Publish();
        }

        protected override void Process(DateTime now)
        {
            lock (this)
            {
                if (m_IsDisconnect)
                {
                    if ((now - m_Disconnect).TotalMilliseconds >= 2000)
                    {
                        Log.Debug("++ Quick Disconnect Triggered");

                        Shutdown();
                        return;
                    }
                }

                if ((now - m_Last).TotalMilliseconds >= 1500 && m_Packet > 0)
                {
                    var transfered = 0;

                    m_Last = now;

                    if (Battery == DsBattery.Charging)
                    {
                        m_Report[9] ^= m_Leds[m_ControllerId];
                    }
                    else
                    {
                        m_Report[9] |= m_Leds[m_ControllerId];
                    }

                    if (GlobalConfiguration.Instance.DisableLED) m_Report[9] = 0;

                    SendTransfer(0x21, 0x09, 0x0201, m_Report, ref transfered);
                }
            }
        }
    }
}