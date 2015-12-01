using System;
using System.ComponentModel;
using System.Linq;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl.Usb.Ds3
{
    /// <summary>
    ///     Represents a DualShock 3 controller connected via USB.
    /// </summary>
    public partial class UsbDs3 : UsbDevice
    {
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
            : base(DeviceClassGuid)
        {
            InitializeComponent();
        }

        public UsbDs3(IContainer container)
            : base(DeviceClassGuid)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region Private fields

        private byte _counterForLeds = 0;
        private byte _ledStatus = 0;

        #endregion

        #region Properties

        public static Guid DeviceClassGuid
        {
            get { return Guid.Parse("{E2824A09-DBAA-4407-85CA-C8E8FF5F6FFA}"); }
        }

        private bool IsFake { get; set; }

        public override DsPadId PadId
        {
            get { return (DsPadId)m_ControllerId; }
            set
            {
                m_ControllerId = (byte)value;
                InputReport.PadId = PadId;

                _hidReport[9] = _ledStatus;
            }
        }

        #endregion

        #region Actions

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

                if (!IniConfig.Instance.Hci.GenuineMacAddresses.Any(m => m_Mac.StartsWith(m)))
                {
                    Log.InfoFormat("-- Fake DualShock detected [{0}]", m_Mac);

                    var bthCompany = IniConfig.Instance.BthChipManufacturers.FirstOrDefault(
                        m => m_Mac.ToUpper().StartsWith(m.PartialMacAddress.ToUpper()));

                    if (bthCompany != null && bthCompany.Name.Equals("AirohaTechnologyCorp"))
                    {
                        Log.InfoFormat("Controller uses Bluetooth chip by Airoha Technology Corp., suppressing workaround");
                        IsFake = false;
                    }
                    else
                    {
                        IsFake = true;
                    }
                }
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
        /// <returns>The result of the send request, true if sent successfully, false otherwise.</returns>
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

                _hidReport[9] = _ledStatus;

                return SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport,
                    ToValue(UsbHidReportRequestType.Output, UsbHidReportRequestId.One),
                    _hidReport, ref transfered);
            }
        }

        /// <summary>
        ///     Pairs the current device to the provided Bluetooth host.
        /// </summary>
        /// <param name="master">The MAC address of the host.</param>
        /// <returns>True on success, false otherwise.</returns>
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

        /// <summary>
        ///     Interprets a HID report sent by a DualShock 3 device.
        /// </summary>
        /// <param name="report">The HID report as byte array.</param>
        protected override void ParseHidReport(byte[] report)
        {
            // report ID must be 1
            if (report[0] != 0x01) return;

            PacketCounter++;

            // set battery level
            m_BatteryStatus = InputReport.BatteryStatus = report[30];

            // set packet counter
            InputReport.PacketCounter = PacketCounter;

            // copy controller data to report packet
            Buffer.BlockCopy(report, 0, InputReport.RawBytes, 8, 49);

            var trigger = false;

            // detect Quick Disconnect combo (L1, R1 and PS buttons pressed at the same time)
            if (InputReport[Profiler.Ds3Button.L1].IsPressed
                && InputReport[Profiler.Ds3Button.R1].IsPressed
                && InputReport[Profiler.Ds3Button.Ps].IsPressed)
            {
                trigger = true;
                // unset PS button
                InputReport.RawBytes[12] ^= 0x01;
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

        /// <summary>
        ///     Sends periodic status updates to the controller (HID Output reports).
        /// </summary>
        /// <param name="now">The current timestamp.</param>
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

                #region LED control

                if ((now - m_Last).TotalMilliseconds >= GlobalConfiguration.Instance.Ds3LEDsPeriod && PacketCounter > 0)
                {
                    m_Last = now;
                    _ledStatus = 0;

                    switch (GlobalConfiguration.Instance.Ds3LEDsFunc)
                    {
                        case 0:
                            _ledStatus = 0;
                            break;
                        case 1:
                            if (GlobalConfiguration.Instance.Ds3PadIDLEDsFlashCharging && Battery == DsBattery.Charging)
                            {
                                _counterForLeds++;
                                _counterForLeds %= 2;
                                if (_counterForLeds == 1)
                                    _ledStatus = _ledOffsets[m_ControllerId];
                            }
                            else _ledStatus = _ledOffsets[m_ControllerId];
                            break;
                        case 2:
                            switch (Battery)
                            {
                                case DsBattery.None:
                                    _ledStatus = 0;
                                    break;
                                case DsBattery.Charging:
                                    _counterForLeds++;
                                    _counterForLeds %= (byte)_ledOffsets.Length;
                                    for (byte i = 0; i <= _counterForLeds; i++)
                                        _ledStatus |= _ledOffsets[i];
                                    break;
                                case DsBattery.Charged:
                                    _ledStatus = (byte)(_ledOffsets[0] | _ledOffsets[1] | _ledOffsets[2] | _ledOffsets[3]);
                                    break;
                                default: ;
                                    break;
                            }
                            break;
                        case 3:
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom1) _ledStatus |= _ledOffsets[0];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom2) _ledStatus |= _ledOffsets[1];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom3) _ledStatus |= _ledOffsets[2];
                            if (GlobalConfiguration.Instance.Ds3LEDsCustom4) _ledStatus |= _ledOffsets[3];
                            break;
                        default:
                            _ledStatus = 0;
                            break;
                    }

                    _hidReport[9] = _ledStatus;
                }

                #endregion

                #region send HID Output Report

                var transfered = 0;

                if (!IsFake)
                {
                    SendTransfer(UsbHidRequestType.HostToDevice, UsbHidRequest.SetReport,
                        ToValue(UsbHidReportRequestType.Output, UsbHidReportRequestId.One),
                        _hidReport, ref transfered);
                }
                else
                {
                    var outReport = ReportDescriptor.OutputReports.FirstOrDefault();
                    if (outReport == null)
                        return;

                    var buffer = new byte[outReport.Length + 1];
                    Buffer.BlockCopy(_hidReport, 0, buffer, 1, _hidReport.Length);
                    buffer[0] = outReport.ID;

                    WriteIntPipe(buffer, buffer.Length, ref transfered);
                }

                #endregion
            }
        }

        #endregion
    }
}
