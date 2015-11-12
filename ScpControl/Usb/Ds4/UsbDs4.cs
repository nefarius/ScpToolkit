using System;
using System.ComponentModel;
using ScpControl.ScpCore;
using Ds4Button = ScpControl.Profiler.Ds4Button;

namespace ScpControl.Usb.Ds4
{
    /// <summary>
    ///     Represents a DualShock 4 controller connected via USB.
    /// </summary>
    public sealed partial class UsbDs4 : UsbDevice
    {
        #region HID Report

        private readonly byte[] _hidReport =
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

        #endregion

        #region Private vars

        private const int R = 6; // Led Offsets
        private const int G = 7; // Led Offsets
        private const int B = 8; // Led Offsets

        private byte _brightness = GlobalConfiguration.Instance.Brightness;
        private bool _isLightBarDisabled;

        #endregion

        #region Ctors

        public UsbDs4() : base(DeviceClassGuid)
        {
            InitializeComponent();
        }

        public UsbDs4(IContainer container) : base(DeviceClassGuid)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region Properties

        public static Guid DeviceClassGuid
        {
            get { return Guid.Parse("{2ED90CE1-376F-4982-8F7F-E056CBC3CA71}"); }
        }

        public override DsPadId PadId
        {
            get { return (DsPadId) m_ControllerId; }
            set
            {
                if (GlobalConfiguration.Instance.IsLightBarDisabled)
                {
                    _hidReport[R] = _hidReport[G] = _hidReport[B] = _hidReport[12] = _hidReport[13] = 0x00;
                    return;
                }

                m_ControllerId = (byte) value;
                InputReport.PadId = PadId;

                switch (value)
                {
                    case DsPadId.One: // Blue
                        _hidReport[R] = 0x00;
                        _hidReport[G] = 0x00;
                        _hidReport[B] = _brightness;
                        break;
                    case DsPadId.Two: // Green
                        _hidReport[R] = 0x00;
                        _hidReport[G] = _brightness;
                        _hidReport[B] = 0x00;
                        break;
                    case DsPadId.Three: // Yellow
                        _hidReport[R] = _brightness;
                        _hidReport[G] = _brightness;
                        _hidReport[B] = 0x00;
                        break;
                    case DsPadId.Four: // Cyan
                        _hidReport[R] = 0x00;
                        _hidReport[G] = _brightness;
                        _hidReport[B] = _brightness;
                        break;
                    case DsPadId.None: // Red
                        _hidReport[R] = _brightness;
                        _hidReport[G] = 0x00;
                        _hidReport[B] = 0x00;
                        break;
                }
            }
        }

        #endregion

        #region Actions

        private static byte MapBattery(byte value)
        {
            var mapped = (byte) DsBattery.None;

            switch (value)
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

                var transfered = 0;

                if (SendTransfer(UsbHidRequestType.DeviceToHost, UsbHidRequest.GetReport, 0x0312, m_Buffer,
                    ref transfered))
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

            // skip repairing if disabled in global configuration
            if (!GlobalConfiguration.Instance.Repair) return base.Start();

            var transfered = 0;
            byte[] buffer =
            {
                0x13, m_Master[5], m_Master[4], m_Master[3], m_Master[2], m_Master[1], m_Master[0],
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            Buffer.BlockCopy(GlobalConfiguration.Instance.BdLink, 0, buffer, 7,
                GlobalConfiguration.Instance.BdLink.Length);

            if (SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport, 0x0313, buffer, ref transfered))
            {
                Log.DebugFormat("++ Repaired DS4 [{0}] Link Key For BTH Dongle [{1}]", Local, Remote);
            }
            else
            {
                Log.DebugFormat("++ Repair DS4 [{0}] Link Key For BTH Dongle [{1}] Failed!", Local, Remote);
            }

            return base.Start();
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

                _hidReport[4] = small;
                _hidReport[5] = large;

                return WriteIntPipe(_hidReport, _hidReport.Length, ref transfered);
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

            Buffer.BlockCopy(GlobalConfiguration.Instance.BdLink, 0, buffer, 7,
                GlobalConfiguration.Instance.BdLink.Length);

            if (SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport, 0x0313, buffer, ref transfered))
            {
                for (var index = 0; index < m_Master.Length; index++)
                {
                    m_Master[index] = master[index];
                }

                Log.DebugFormat("++ Paired DS4 [{0}] To BTH Dongle [{1}]", Local, Remote);
                return true;
            }

            Log.DebugFormat("++ Pair Failed [{0}]", Local);
            return false;
        }

        /// <summary>
        ///     Interprets a HID report sent by a DualShock 4 device.
        /// </summary>
        /// <param name="report">The HID report as byte array.</param>
        protected override void ParseHidReport(byte[] report)
        {
            if (report[0] != 0x01) return;

            PacketCounter++;

            m_BatteryStatus = InputReport.SetBatteryStatus((DsBattery) MapBattery(report[30]));

            InputReport.PacketCounter = PacketCounter;

            var buttons = ((report[5] << 0) | (report[6] << 8) | (report[7] << 16));

            #region Convert HAT to DPAD

            report[5] &= 0xF0;

            switch ((uint) buttons & 0xF)
            {
                case 0:
                    report[5] |= (byte) Ds4Button.Up.Offset;
                    break;
                case 1:
                    report[5] |= (byte) (Ds4Button.Up.Offset | Ds4Button.Right.Offset);
                    break;
                case 2:
                    report[5] |= (byte) Ds4Button.Right.Offset;
                    break;
                case 3:
                    report[5] |= (byte) (Ds4Button.Right.Offset | Ds4Button.Down.Offset);
                    break;
                case 4:
                    report[5] |= (byte) (Ds4Button.Down.Offset);
                    break;
                case 5:
                    report[5] |= (byte) (Ds4Button.Down.Offset | Ds4Button.Left.Offset);
                    break;
                case 6:
                    report[5] |= (byte) Ds4Button.Left.Offset;
                    break;
                case 7:
                    report[5] |= (byte) (Ds4Button.Left.Offset | Ds4Button.Up.Offset);
                    break;
            }

            #endregion

            // copy controller data to report packet
            Buffer.BlockCopy(report, 0, InputReport.RawBytes, 8, 64);

            OnHidReportReceived();
        }

        protected override void Process(DateTime now)
        {
            lock (this)
            {
                if (!((now - m_Last).TotalMilliseconds >= 500)) return;

                var transfered = 0;

                m_Last = now;

                if (!GlobalConfiguration.Instance.IsLightBarDisabled)
                {
                    if (Battery != DsBattery.Charged)
                    {
                        _hidReport[9] = _hidReport[10] = 0x80;
                    }
                    else
                    {
                        _hidReport[9] = _hidReport[10] = 0x00;
                    }
                }

                if (GlobalConfiguration.Instance.Brightness != _brightness)
                {
                    _brightness = GlobalConfiguration.Instance.Brightness;
                    PadId = PadId;
                }

                if (GlobalConfiguration.Instance.IsLightBarDisabled != _isLightBarDisabled)
                {
                    _isLightBarDisabled = GlobalConfiguration.Instance.IsLightBarDisabled;
                    PadId = PadId;
                }

                WriteIntPipe(_hidReport, _hidReport.Length, ref transfered);
            }
        }

        #endregion
    }
}