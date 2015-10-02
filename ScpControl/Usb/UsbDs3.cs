using System;
using System.Collections.Generic;
using System.ComponentModel;
using ScpControl.ScpCore;

namespace ScpControl.Usb
{
    /// <summary>
    ///     Represents a DualShock 3 controller connected via USB.
    /// </summary>
    public partial class UsbDs3 : UsbDevice
    {
        public static string USB_CLASS_GUID = "{E2824A09-DBAA-4407-85CA-C8E8FF5F6FFA}";

        #region HID Reports

        private readonly byte[] _hidCommandEnable = { 0x42, 0x0C, 0x00, 0x00 };
        private readonly byte[] _ledOffsets = { 0x02, 0x04, 0x08, 0x10 };

        private readonly byte[] _hidReport =
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

        #endregion

        #region Ctors

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

        #endregion

        private byte counterForLeds = 0;
        private byte ledStatus = 0;

        public override DsPadId PadId
        {
            get { return (DsPadId)m_ControllerId; }
            set
            {
                m_ControllerId = (byte)value;
                m_ReportArgs.Pad = PadId;

                _hidReport[9] = ledStatus;
            }
        }

        public override bool Open(string devicePath)
        {
            if (base.Open(devicePath))
            {
                m_State = DsState.Reserved;
                GetDeviceInstance(ref m_Instance);

                var transfered = 0;

                if (SendTransfer(UsbHidRequestType.DeviceToHost, UsbHidRequest.GetReport, 0x03F5, m_Buffer, ref transfered))
                {
                    m_Master = new[] { m_Buffer[2], m_Buffer[3], m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7] };
                }

                if (SendTransfer(UsbHidRequestType.DeviceToHost, UsbHidRequest.GetReport, 0x03F2, m_Buffer, ref transfered))
                {
                    m_Local = new[] { m_Buffer[4], m_Buffer[5], m_Buffer[6], m_Buffer[7], m_Buffer[8], m_Buffer[9] };
                }

                m_Mac = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);

                Log.InfoFormat("Successfully opened device with MAC address {0}", m_Mac);
            }

            return State == DsState.Reserved;
        }

        public override bool Start()
        {
            m_Model = (byte)DsModel.DS3;

            if (IsActive)
            {
                var transfered = 0;

                if (SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport, 0x03F4, _hidCommandEnable, ref transfered))
                {
                    base.Start();
                }
            }

            return State == DsState.Connected;
        }

        /// <summary>
        ///     Send Rumble request to controller.
        /// </summary>
        /// <param name="large">Larg motor.</param>
        /// <param name="small">Small motor.</param>
        /// <returns>Always true.</returns>
        public override bool Rumble(byte large, byte small)
        {
            lock (this)
            {
                var transfered = 0;

                if (GlobalConfiguration.Instance.DisableRumble)
                {
                    _hidReport[2] = 0;
                    _hidReport[4] = 0;
                }
                else
                {
                    _hidReport[2] = (byte)(small > 0 ? 0x01 : 0x00);
                    _hidReport[4] = large;
                }

                _hidReport[9] = ledStatus;

                return SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport,
                    ToValue(UsbHidReportRequestType.Output, UsbHidReportRequestId.One),
                    _hidReport, ref transfered);
            }
        }

        public override bool Pair(byte[] master)
        {
            var transfered = 0;
            byte[] buffer = { 0x00, 0x00, master[0], master[1], master[2], master[3], master[4], master[5] };

            if (SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport, 0x03F5, buffer, ref transfered))
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

        protected override void Parse(byte[] report)
        {
            if (report[0] != 0x01) return;

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            m_ReportArgs.Report[2] = m_BatteryStatus = report[30];

            m_ReportArgs.Report[4] = (byte)(m_Packet >> 0 & 0xFF);
            m_ReportArgs.Report[5] = (byte)(m_Packet >> 8 & 0xFF);
            m_ReportArgs.Report[6] = (byte)(m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (byte)(m_Packet >> 24 & 0xFF);

            var buttons = (Ds3Button)((report[2] << 0) | (report[3] << 8) | (report[4] << 16) | (report[5] << 24));
            var trigger = false;

            // detect Quick Disconnect combo (L1, R1 and PS buttons pressed at the same time)
            if ((buttons & Ds3Button.L1) == Ds3Button.L1
                && (buttons & Ds3Button.R1) == Ds3Button.R1
                && (buttons & Ds3Button.PS) == Ds3Button.PS
                )
            {
                trigger = true;
                report[4] ^= 0x1;
            }

            for (var index = 8; index < 57; index++)
            {
                m_ReportArgs.Report[index] = report[index - 8];
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

            OnHidReportReceived();
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

                if ((now - m_Last).TotalMilliseconds >= GlobalConfiguration.Instance.Ds3LEDsPeriod && m_Packet > 0)
                {
                    var transfered = 0;

                    m_Last = now;

                    ledStatus = 0;

                    switch (GlobalConfiguration.Instance.Ds3LEDsFunc)
                    {
                        case 0:
                            ledStatus = 0;
                            break;
                        case 1:
                            if (GlobalConfiguration.Instance.Ds3PadIDLEDsFlashCharging)
                            {
                                counterForLeds++;
                                counterForLeds %= 2;
                                if (counterForLeds == 1)
                                    ledStatus = _ledOffsets[m_ControllerId];
                            }
                            else ledStatus = _ledOffsets[m_ControllerId];
                            break;
                        case 2:
                            switch (Battery)
                            {
                                case DsBattery.None:
                                    ledStatus = 0;
                                    break;
                                case DsBattery.Charging:
                                    counterForLeds++;
                                    counterForLeds %= (byte)_ledOffsets.Length;
                                    for (byte i = 0; i <= counterForLeds; i++)
                                        ledStatus |= _ledOffsets[i];
                                    break;
                                case DsBattery.Full:
                                    ledStatus = (byte)(_ledOffsets[0] | _ledOffsets[1] | _ledOffsets[2] | _ledOffsets[3]);
                                    break;
                                default: ;
                                    break;
                            }
                            break;
                        case 3:
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom1) ledStatus |= _ledOffsets[0];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom2) ledStatus |= _ledOffsets[1];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom3) ledStatus |= _ledOffsets[2];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom4) ledStatus |= _ledOffsets[3];
                            break;
                        default:
                            ledStatus = 0;
                            break;
                    }

                    _hidReport[9] = ledStatus;

                    SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport,
                        ToValue(UsbHidReportRequestType.Output, UsbHidReportRequestId.One), 
                        _hidReport, ref transfered);
                }
            }
        }
    }

    public static class ReportEventArgsExtensions
    {
        public static void SetPacketCounter(this ReportEventArgs args, uint packet)
        {
            args.Report[4] = (byte)(packet >> 0 & 0xFF);
            args.Report[5] = (byte)(packet >> 8 & 0xFF);
            args.Report[6] = (byte)(packet >> 16 & 0xFF);
            args.Report[7] = (byte)(packet >> 24 & 0xFF);
        }

        public static byte SetBatteryStatus(this ReportEventArgs args, byte[] report)
        {
            return args.Report[2] = report[30];
        }

        public static byte SetBatteryStatus(this ReportEventArgs args, DsBattery battery)
        {
            return args.Report[2] = (byte)battery;
        }

        public static void SetTriangleDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 4);
        }

        public static void SetCircleDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 5);
        }

        public static void SetCrossDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 6);
        }

        public static void SetSquareDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte) ((input & 1) << 7); 
        }

        public static void SetDpadRightDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x20 : 0x00);
        }

        public static void SetDpadLeftDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x80 : 0x00);
        }

        public static void SetDpadUpDgital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x10 : 0x00);
        }

        public static void SetDpadDownDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x40 : 0x00);
        }

        public static void ZeroShoulderButtonsState(this ReportEventArgs args)
        {
            args.Report[11] = 0x00;
        }

        public static void ZeroSelectStartButtonsState(this ReportEventArgs args)
        {
            args.Report[10] = 0x00;
        }

        public static void SetSelect(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte) (input & 1);
        }

        public static void SetStart(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte)((input & 1) << 3);
        }

        public static void SetLeftShoulderDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte) ((input & 1) << 2);
        }

        public static void SetRightShoulderDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 3);
        }
    }
}
