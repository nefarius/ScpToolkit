using System;
using System.ComponentModel;
using System.Linq;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl.Bluetooth
{
    public partial class BthDs3 : BthDevice
    {
        private byte[] m_Enable = { 0x53, 0xF4, 0x42, 0x03, 0x00, 0x00 };

        private readonly byte[][] _hidInitReport =
        {
            new byte[] {0x02, 0x00, 0x0F, 0x00, 0x08, 0x35, 0x03, 0x19, 0x12, 0x00, 0x00, 0x03, 0x00},
            new byte[]
            {
                0x04, 0x00, 0x10, 0x00, 0x0F, 0x00, 0x01, 0x00, 0x01, 0x00, 0x10, 0x35, 0x06, 0x09, 0x02, 0x01, 0x09, 0x02,
                0x02, 0x00
            },
            new byte[]
            {0x06, 0x00, 0x11, 0x00, 0x0D, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x00},
            new byte[]
            {
                0x06, 0x00, 0x12, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02,
                0x00, 0x7F
            },
            new byte[]
            {
                0x06, 0x00, 0x13, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02,
                0x00, 0x59
            },
            new byte[]
            {
                0x06, 0x00, 0x14, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x80, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02,
                0x00, 0x33
            },
            new byte[]
            {
                0x06, 0x00, 0x15, 0x00, 0x0F, 0x35, 0x03, 0x19, 0x11, 0x24, 0x01, 0x90, 0x35, 0x03, 0x09, 0x02, 0x06, 0x02,
                0x00, 0x0D
            }
        };

        private readonly byte[] _leds = { 0x02, 0x04, 0x08, 0x10 };

        private readonly byte[] _hidReport =
        {
            0x52, 0x01,
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

        public BthDs3()
        {
            InitializeComponent();
        }

        public BthDs3(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthDs3(IBthDevice device, byte[] master, byte lsb, byte msb)
            : base(device, master, lsb, msb)
        {
        }

        public override DsPadId PadId
        {
            get { return (DsPadId)m_ControllerId; }
            set
            {
                m_ControllerId = (byte)value;
                m_ReportArgs.Pad = PadId;

                _hidReport[11] = _leds[m_ControllerId];
            }
        }

        public override bool Start()
        {
            CanStartHid = false;
            m_State = DsState.Connected;

            var bdc = IniConfig.Instance.BthDs3;

            if (bdc.SupportedMacs.Any(m => Local.StartsWith(m))) // Fix up for Fake DS3
            {
                Log.WarnFormat("Fake DS3 detected: {0} [{1}]", RemoteName, Local);

                m_Enable[0] = 0xA3;

                _hidReport[0] = 0xA2;
                _hidReport[3] = 0x00;
                _hidReport[5] = 0x00;
            }

            if (bdc.SupportedNames.Any(n => RemoteName.EndsWith(n))) // Fix up for Fake DS3
            {
                Log.WarnFormat("Fake DS3 detected: {0} [{1}]", RemoteName, Local);

                _hidReport[3] = 0x00;
                _hidReport[5] = 0x00;
            }

            m_Queued = 1;
            m_Blocked = true;
            m_Last = DateTime.Now;
            m_Device.HID_Command(HciHandle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), m_Enable);

            return base.Start();
        }

        public override void Parse(byte[] report)
        {
            if (report[10] == 0xFF) return;

            m_PlugStatus = report[38];
            m_BatteryStatus = report[39];
            m_CableStatus = report[40];

            if (m_Packet == 0) Rumble(0, 0);
            m_Packet++;

            m_ReportArgs.Report[2] = m_BatteryStatus;

            m_ReportArgs.Report[4] = (byte)(m_Packet >> 0 & 0xFF);
            m_ReportArgs.Report[5] = (byte)(m_Packet >> 8 & 0xFF);
            m_ReportArgs.Report[6] = (byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (byte)(m_Packet >> 24 & 0xFF);

            var buttons = (Ds3Button)((report[11] << 0) | (report[12] << 8) | (report[13] << 16) | (report[14] << 24));
            bool trigger = false, active = false;

            // Quick Disconnect
            if ((buttons & Ds3Button.L1) == Ds3Button.L1
                && (buttons & Ds3Button.R1) == Ds3Button.R1
                && (buttons & Ds3Button.PS) == Ds3Button.PS
                )
            {
                trigger = true;
                report[13] ^= 0x1;
            }

            for (var index = 8; index < 57; index++)
            {
                m_ReportArgs.Report[index] = report[index + 1];
            }

            // Buttons
            for (var index = 11; index < 15 && !active; index++)
            {
                if (report[index] != 0) active = true;
            }

            // Axis
            for (var index = 15; index < 19 && !active; index++)
            {
                if (report[index] < 117 || report[index] > 137) active = true;
            }

            // Triggers & Pressure
            for (var index = 23; index < 35 && !active; index++)
            {
                if (report[index] != 0) active = true;
            }

            if (active)
            {
                m_IsIdle = false;
            }
            else if (!m_IsIdle)
            {
                m_IsIdle = true;
                m_Idle = DateTime.Now;
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

        public override bool Rumble(byte large, byte small)
        {
            lock (this)
            {
                if (GlobalConfiguration.Instance.DisableRumble)
                {
                    _hidReport[4] = 0;
                    _hidReport[6] = 0;
                }
                else
                {
                    _hidReport[4] = (byte)(small > 0 ? 0x01 : 0x00);
                    _hidReport[6] = large;
                }

                if (!m_Blocked && GlobalConfiguration.Instance.Latency == 0)
                {
                    m_Last = DateTime.Now;
                    m_Blocked = true;

                    m_Device.HID_Command(HciHandle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), _hidReport);
                }
                else
                {
                    m_Queued = 1;
                }
            }
            return true;
        }

        public override bool InitReport(byte[] report)
        {
            var retVal = false;

            if (m_Init < _hidInitReport.Length)
            {
                m_Device.HID_Command(HciHandle.Bytes, Get_SCID(L2CAP.PSM.HID_Service), _hidInitReport[m_Init++]);
            }
            else if (m_Init == _hidInitReport.Length)
            {
                m_Init++;
                retVal = true;
            }

            return retVal;
        }

        protected override void Process(DateTime now)
        {
            lock (this)
            {
                if (m_State == DsState.Connected)
                {
                    if ((now - m_Tick).TotalMilliseconds >= 500 && m_Packet > 0)
                    {
                        m_Tick = now;

                        if (m_Queued == 0) m_Queued = 1;

                        if (Battery < DsBattery.Medium)
                        {
                            _hidReport[11] ^= _leds[m_ControllerId];
                        }
                        else
                        {
                            _hidReport[11] |= _leds[m_ControllerId];
                        }
                    }

                    if (GlobalConfiguration.Instance.DisableLED) _hidReport[11] = 0;

                    if (!m_Blocked && m_Queued > 0)
                    {
                        if ((now - m_Last).TotalMilliseconds >= GlobalConfiguration.Instance.Latency)
                        {
                            m_Last = now;
                            m_Blocked = true;
                            m_Queued--;

                            m_Device.HID_Command(HciHandle.Bytes, Get_SCID(L2CAP.PSM.HID_Command), _hidReport);
                        }
                    }
                }
            }
        }
    }
}