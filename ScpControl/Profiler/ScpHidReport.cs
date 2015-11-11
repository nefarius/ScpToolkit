using System;
using System.Linq;
using System.Reflection;
using ScpControl.ScpCore;

namespace ScpControl.Profiler
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

        [Obsolete]
        public void SetPacketCounter(uint packet)
        {
            RawBytes[4] = (byte) (packet >> 0 & 0xFF);
            RawBytes[5] = (byte) (packet >> 8 & 0xFF);
            RawBytes[6] = (byte) (packet >> 16 & 0xFF);
            RawBytes[7] = (byte) (packet >> 24 & 0xFF);
        }

        public byte SetBatteryStatus(DsBattery battery)
        {
            return RawBytes[2] = (byte) battery;
        }

        public void SetTriangleDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 4);
        }

        public void SetCircleDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 5);
        }

        public void SetCrossDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 6);
        }

        public void SetSquareDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 7);
        }

        public void SetDpadRightDigital(bool input)
        {
            RawBytes[10] |= (byte) (input ? 0x20 : 0x00);
        }

        public void SetDpadLeftDigital(bool input)
        {
            RawBytes[10] |= (byte) (input ? 0x80 : 0x00);
        }

        public void SetDpadUpDigital(bool input)
        {
            RawBytes[10] |= (byte) (input ? 0x10 : 0x00);
        }

        public void SetDpadDownDigital(bool input)
        {
            RawBytes[10] |= (byte) (input ? 0x40 : 0x00);
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

        public void SetSelect(int input)
        {
            RawBytes[10] |= (byte) (input & 1);
        }

        public void SetStart(int input)
        {
            RawBytes[10] |= (byte) ((input & 1) << 3);
        }

        public void SetPs(int input)
        {
            RawBytes[12] |= (byte) (input & 1);
        }

        public void SetLeftShoulderDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 2);
        }

        public void SetRightShoulderDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 3);
        }

        public void SetLeftTriggerDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 0);
        }

        public void SetRightTriggerDigital(int input)
        {
            RawBytes[11] |= (byte) ((input & 1) << 1);
        }

        public void SetLeftShoulderAnalog(byte input)
        {
            RawBytes[28] = input;
        }

        public void SetRightShoulderAnalog(byte input)
        {
            RawBytes[29] = input;
        }

        public void SetLeftTriggerAnalog(byte input)
        {
            RawBytes[26] = input;
        }

        public void SetRightTriggerAnalog(byte input)
        {
            RawBytes[27] = input;
        }

        public void SetTriangleAnalog(byte input)
        {
            RawBytes[30] = input;
        }

        public void SetCircleAnalog(byte input)
        {
            RawBytes[31] = input;
        }

        public void SetCrossAnalog(byte input)
        {
            RawBytes[32] = input;
        }

        public void SetSquareAnalog(byte input)
        {
            RawBytes[33] = input;
        }

        public void SetLeftThumb(int input)
        {
            RawBytes[10] |= (byte) ((input & 1) << 1);
        }

        public void SetRightThumb(int input)
        {
            RawBytes[10] |= (byte) ((input & 1) << 2);
        }

        public void SetLeftAxisY(byte input)
        {
            RawBytes[15] = input;
        }

        public void SetLeftAxisX(byte input)
        {
            RawBytes[14] = input;
        }

        public void SetRightAxisY(byte input)
        {
            RawBytes[17] = input;
        }

        public void SetRightAxisX(byte input)
        {
            RawBytes[16] = input;
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

        public uint PacketCounter
        {
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
            get { return (DsState) RawBytes[1]; }
            set { RawBytes[1] = (byte) value; }
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
            get { return RawBytes[2]; }
            set { RawBytes[2] = value; }
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
                                button => this[button.GetValue(typeof(Ds3Button), null) as IDsButton].IsPressed)
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
                    IsActive = (RawBytes[43] >> 7) == 0,
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
                    IsActive = (RawBytes[47] >> 7) == 0,
                    X = ((RawBytes[49] & 0x0f) << 8) | RawBytes[48],
                    Y = RawBytes[50] << 4 | ((RawBytes[49] & 0xf0) >> 4)
                };
            }
        }

        #endregion

        #region Indexers

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

                    return new DsButtonState
                    {
                        IsPressed = (!button.Equals(Ds3Button.None)) && (buttons & button.Offset) == button.Offset
                    };
                }

                if (button is Ds4Button && Model == DsModel.DS4)
                {
                    var buttons =
                        (uint) ((RawBytes[13] << 0) | (RawBytes[14] << 8) | (RawBytes[15] << 16));

                    return new DsButtonState
                    {
                        IsPressed = (!button.Equals(Ds4Button.None)) && (buttons & button.Offset) == button.Offset
                    };
                }

                return new DsButtonState();
            }
        }

        /// <summary>
        ///     Gets the axis state of the current packet.
        /// </summary>
        /// <param name="axis">The DualShock axis to question.</param>
        /// <returns>The value of the axis in question.</returns>
        public IDsAxisState this[IDsAxis axis]
        {
            get
            {
                if ((axis is Ds3Axis && Model == DsModel.DS3)
                    || (axis is Ds4Axis && Model == DsModel.DS4))
                {
                    if (axis.Equals(Ds3Axis.None) || axis.Equals(Ds4Axis.None))
                        return new DsAxisState() {IsEngaged = false};

                    return new DsAxisState
                    {
                        Value = RawBytes[axis.Offset],
                        IsEngaged = (axis.DefaultValue == 0x00)
                            ? (axis.DefaultValue != RawBytes[axis.Offset])
                            /* 
                             * match a range for jitter compensation
                             * if axis value is between 117 and 137 it's not reported as engaged
                             * */
                            : (((axis.DefaultValue - 10) > RawBytes[axis.Offset])
                               || ((axis.DefaultValue + 10) < RawBytes[axis.Offset]))
                    };
                }

                // default is centered
                return new DsAxisState();
            }
        }

        #endregion
    }
}