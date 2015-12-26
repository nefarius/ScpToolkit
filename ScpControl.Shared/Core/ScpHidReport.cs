using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ScpControl.Shared.Core
{
    /// <summary>
    ///     Represents an extended HID Input Report ready to be sent to the virtual bus device.
    /// </summary>
    public class ScpHidReport : EventArgs
    {
        /// <summary>
        ///     Report bytes count.
        /// </summary>
        public static int Length
        {
            get { return 96; }
        }

        #region Private fields

        private static readonly PropertyInfo[] Ds3Buttons =
            typeof (Ds3Button).GetProperties(BindingFlags.Public | BindingFlags.Static);

        private static readonly PropertyInfo[] Ds3Axes =
            typeof (Ds3Axis).GetProperties(BindingFlags.Public | BindingFlags.Static);

        private static readonly PropertyInfo[] Ds4Buttons =
            typeof (Ds4Button).GetProperties(BindingFlags.Public | BindingFlags.Static);

        private static readonly PropertyInfo[] Ds4Axes =
            typeof (Ds4Axis).GetProperties(BindingFlags.Public | BindingFlags.Static);

        #endregion

        #region Public methods

        /// <summary>
        ///     Sets the status of a digital (two-state) button.
        /// </summary>
        /// <param name="button">The button of the current report to manipulate.</param>
        /// <param name="value">True to set the button as pressed, false to set the button as released.</param>
        public void Set(IDsButton button, bool value = true)
        {
            button.ToggleBit(ref RawBytes[button.ArrayIndex], value);
        }

        /// <summary>
        ///     Sets the status of a digital (two-state) button to 'released'.
        /// </summary>
        /// <param name="button">The button of the current report to unset.</param>
        public void Unset(IDsButton button)
        {
            Set(button, false);
        }

        /// <summary>
        ///     Sets the value of an analog axis.
        /// </summary>
        /// <param name="axis">The axis of the current report to manipulate.</param>
        /// <param name="value">The value to set the axis to.</param>
        public void Set(IDsAxis axis, byte value)
        {
            RawBytes[axis.Offset] = value;
        }

        /// <summary>
        ///     Sets the value of an analog axis to it's default value.
        /// </summary>
        /// <param name="axis">The axis of the current report to manipulate.</param>
        public void Unset(IDsAxis axis)
        {
            RawBytes[axis.Offset] = axis.DefaultValue;
        }

        public byte SetBatteryStatus(DsBattery battery)
        {
            return RawBytes[(int) DsOffset.Battery] = (byte) battery;
        }

        public void ZeroShoulderButtonsState()
        {
            RawBytes[11] = 0x00;
        }

        public void ZeroSelectStartButtonsState()
        {
            RawBytes[10] = 0x00;
        }

        public void ZeroPsButtonState()
        {
            RawBytes[12] = 0x00;
        }

        #endregion

        #region Ctors

        public ScpHidReport()
        {
            RawBytes = new byte[Length];
            ReportId = 0x01;
        }

        public ScpHidReport(byte[] report)
        {
            RawBytes = report;
        }

        #endregion

        #region Public properties

        public byte[] RawBytes { get; private set; }

        public PhysicalAddress PadMacAddress
        {
            get
            {
                // last 6 bytes contain the PADs MAC address
                return new PhysicalAddress(RawBytes.Skip(Math.Max(0, RawBytes.Length - 6)).ToArray());
            }
            set
            {
                if (value != null)
                    Buffer.BlockCopy(value.GetAddressBytes(), 0, RawBytes, 90, 6);
            }
        }

        public uint PacketCounter
        {
            get { return (uint) ((RawBytes[7] << 24) | (RawBytes[6] << 16) | (RawBytes[5] << 8) | (RawBytes[4] << 0)); }
            set
            {
                RawBytes[4] = (byte) (value >> 0 & 0xFF);
                RawBytes[5] = (byte) (value >> 8 & 0xFF);
                RawBytes[6] = (byte) (value >> 16 & 0xFF);
                RawBytes[7] = (byte) (value >> 24 & 0xFF);
            }
        }

        public DsModel Model
        {
            get { return (DsModel) RawBytes[(int) DsOffset.Model]; }
            set { RawBytes[(int) DsOffset.Model] = (byte) value; }
        }

        public DsPadId PadId
        {
            get { return (DsPadId) RawBytes[(int) DsOffset.Pad]; }
            set { RawBytes[(int) DsOffset.Pad] = (byte) value; }
        }

        public DsState PadState
        {
            get { return (DsState) RawBytes[(int) DsOffset.State]; }
            set { RawBytes[(int)DsOffset.State] = (byte)value; }
        }

        public byte ReportId
        {
            get { return RawBytes[0]; }
            set { RawBytes[0] = value; }
        }

        public DsConnection ConnectionType
        {
            get { return (DsConnection) RawBytes[(int) DsOffset.Connection]; }
            set { RawBytes[(int) DsOffset.Connection] = (byte) value; }
        }

        public byte BatteryStatus
        {
            get { return RawBytes[(int)DsOffset.Battery]; }
            set { RawBytes[(int)DsOffset.Battery] = value; }
        }

        public bool IsPadActive
        {
            get
            {
                switch (Model)
                {
                    case DsModel.DS3:
                        if (
                            Ds3Buttons.Any(
                                button => this[button.GetValue(typeof (Ds3Button), null) as IDsButton].IsPressed)
                            || Ds3Axes.Any(axis => this[axis.GetValue(typeof (Ds3Axis), null) as IDsAxis].IsEngaged))
                        {
                            return true;
                        }
                        break;
                    case DsModel.DS4:
                        if (
                            Ds4Buttons.Any(
                                button => this[button.GetValue(typeof (Ds3Button), null) as IDsButton].IsPressed)
                            || Ds4Axes.Any(axis => this[axis.GetValue(typeof (Ds4Axis), null) as IDsAxis].IsEngaged))
                        {
                            return true;
                        }
                        break;
                    default:
                        return false;
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets the motion data from the DualShock accelerometer sensor.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4/blob/master/index.js</remarks>
        public DsAccelerometer Motion
        {
            get
            {
                switch (Model)
                {
                    case DsModel.DS3:
                        throw new NotImplementedException("DualShock 3 accelerometer readout not implemented yet.");
                    case DsModel.DS4:
                        return new DsAccelerometer
                        {
                            Y = (short) ((RawBytes[22] << 8) | RawBytes[21]),
                            X = (short) -((RawBytes[24] << 8) | RawBytes[23]),
                            Z = (short) -((RawBytes[26] << 8) | RawBytes[25])
                        };
                }

                return new DsAccelerometer();
            }
        }

        /// <summary>
        ///     Gets the orientation data from the DualShock gyroscope sensor.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4/blob/master/index.js</remarks>
        public DsGyroscope Orientation
        {
            get
            {
                switch (Model)
                {
                    case DsModel.DS3:
                        throw new NotImplementedException("DualShock 3 gyroscope readout not implemented yet.");
                    case DsModel.DS4:
                        return new DsGyroscope
                        {
                            Roll = (short) -((RawBytes[28] << 8) | RawBytes[27]),
                            Yaw = (short) ((RawBytes[30] << 8) | RawBytes[29]),
                            Pitch = (short) ((RawBytes[32] << 8) | RawBytes[31])
                        };
                }

                return new DsGyroscope();
            }
        }

        #endregion

        #region DualShock 4 specific properties

        /// <summary>
        ///     The first touch spot on the DualShock 4 track pad.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4</remarks>
        public DsTrackPadTouch TrackPadTouch0
        {
            get
            {
                if (Model != DsModel.DS4) return null;

                return new DsTrackPadTouch
                {
                    Id = RawBytes[43] & 0x7f,
                    IsActive = RawBytes[43] >> 7 == 0,
                    X = ((RawBytes[45] & 0x0f) << 8) | RawBytes[44],
                    Y = RawBytes[46] << 4 | ((RawBytes[45] & 0xf0) >> 4)
                };
            }
        }

        /// <summary>
        ///     The second touch spot on the DualShock 4 track pad.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4</remarks>
        public DsTrackPadTouch TrackPadTouch1
        {
            get
            {
                if (Model != DsModel.DS4) return null;

                return new DsTrackPadTouch
                {
                    Id = RawBytes[47] & 0x7f,
                    IsActive = RawBytes[47] >> 7 == 0,
                    X = ((RawBytes[49] & 0x0f) << 8) | RawBytes[48],
                    Y = RawBytes[50] << 4 | ((RawBytes[49] & 0xf0) >> 4)
                };
            }
        }

        #endregion

        #region Indexers

        private readonly IDsButtonState _currentDsButtonState = new DsButtonState();

        /// <summary>
        ///     Checks if a given button state is engaged in the current packet.
        /// </summary>
        /// <param name="button">The DualShock button to question.</param>
        /// <returns>True if the button is pressed, false if the button is released.</returns>
        public IDsButtonState this[IDsButton button]
        {
            get
            {
                if (button is Ds3Button && Model == DsModel.DS3)
                {
                    var buttons =
                        (uint) ((RawBytes[10] << 0) | (RawBytes[11] << 8) | (RawBytes[12] << 16) | (RawBytes[13] << 24));

                    _currentDsButtonState.IsPressed = !button.Equals(Ds3Button.None) &&
                                                      (buttons & button.Offset) == button.Offset;
                    _currentDsButtonState.Xbox360Button = _currentDsButtonState.IsPressed
                        ? button.Xbox360Button
                        : X360Button.None;

                    return _currentDsButtonState;
                }

                if (button is Ds4Button && Model == DsModel.DS4)
                {
                    var buttons =
                        (uint) ((RawBytes[13] << 0) | (RawBytes[14] << 8) | (RawBytes[15] << 16));

                    _currentDsButtonState.IsPressed = !button.Equals(Ds4Button.None) &&
                                                      (buttons & button.Offset) == button.Offset;
                    _currentDsButtonState.Xbox360Button = _currentDsButtonState.IsPressed
                        ? button.Xbox360Button
                        : X360Button.None;

                    return _currentDsButtonState;
                }

                return _currentDsButtonState;
            }
        }

        private readonly IDsAxisState _currentDsAxisState = new DsAxisState();

        /// <summary>
        ///     Gets the axis state of the current packet.
        /// </summary>
        /// <param name="axis">The DualShock axis to question.</param>
        /// <returns>The value of the axis in question.</returns>
        public IDsAxisState this[IDsAxis axis]
        {
            get
            {
                if ((!(axis is Ds3Axis) || Model != DsModel.DS3) && (!(axis is Ds4Axis) || Model != DsModel.DS4))
                    throw new NotImplementedException();

                if (axis.Equals(Ds3Axis.None) || axis.Equals(Ds4Axis.None))
                {
                    _currentDsAxisState.IsEngaged = false;
                    return _currentDsAxisState;
                }

                _currentDsAxisState.Value = RawBytes[axis.Offset];
                _currentDsAxisState.IsEngaged = axis.DefaultValue == 0x00
                    ? axis.DefaultValue != RawBytes[axis.Offset]
                    /* 
                        * match a range for jitter compensation
                        * if axis value is between 117 and 137 it's not reported as engaged
                        * */
                    : (axis.DefaultValue - 10 > RawBytes[axis.Offset])
                      || (axis.DefaultValue + 10 < RawBytes[axis.Offset]);

                return _currentDsAxisState;
            }
        }

        #endregion
    }
}
