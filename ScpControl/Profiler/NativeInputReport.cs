using System;
using ScpControl.ScpCore;

namespace ScpControl.Profiler
{
    public class NativeInputReport : EventArgs
    {
        public NativeInputReport(byte[] report)
        {
            RawBytes = report;
        }

        public byte[] RawBytes { get; private set; }

        public DsModel Model
        {
            get { return (DsModel) RawBytes[(int) DsOffset.Model]; }
        }

        public int PadId
        {
            get { return RawBytes[(int) DsOffset.Pad]; }
        }

        /// <summary>
        ///     Checks if a given button state is engaged in the current packet.
        /// </summary>
        /// <param name="button">The DualShock button to question.</param>
        /// <returns>True if the button is pressed, false if the button is released.</returns>
        public bool this[IDsButton button]
        {
            get
            {
                if (button is Ds3Button && Model == DsModel.DS3)
                {
                    var buttons =
                        (uint) ((RawBytes[10] << 0) | (RawBytes[11] << 8) | (RawBytes[12] << 16) | (RawBytes[13] << 24));

                    return (buttons & button.Offset) == button.Offset;
                }

                if (button is Ds4Button && Model == DsModel.DS4)
                {
                    var buttons =
                        (uint)((RawBytes[13] << 0) | (RawBytes[14] << 8) | (RawBytes[15] << 16));

                    return (buttons & button.Offset) == button.Offset;
                }

                return false;
            }
        }

        /// <summary>
        ///      Gets the axis state of the current packet.
        /// </summary>
        /// <param name="axis">The DualShock axis to question.</param>
        /// <returns>The value of the axis in question.</returns>
        public byte this[IDsAxis axis]
        {
            get
            {
                if (axis is Ds3Axis && Model == DsModel.DS3)
                {
                    return RawBytes[axis.Offset];
                }

                if (axis is Ds4Axis && Model == DsModel.DS4)
                {
                    return RawBytes[axis.Offset];
                }

                // default is centered
                return 0x80;
            }
        }
    }
}
