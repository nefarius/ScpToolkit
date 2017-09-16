using System;
using HidReport.Contract.DsActors;

namespace HidReport.DsActors
{
    /// <summary>
    ///     Implements the possible states for a DualShock button.
    /// </summary>
    [Serializable]
    public class DsButtonState : IDsButtonState
    {
        #region Properties

        /// <summary>
        ///     True if the button in question is currently pressed, false if it's released.
        /// </summary>
        public bool IsPressed { get; set; }

        /// <summary>
        ///     Gets the pressure value of the current button compatible with PCSX2s XInput/LilyPad mod.
        /// </summary>
        /// <remarks>This is just a boolean to float conversion.</remarks>
        public float Pressure => IsPressed ? 1.0f : 0.0f;

        /// <summary>
        ///     Gets the button press state as byte value.
        /// </summary>
        /// <remarks>255 equals pressed, 0 equals released.</remarks>
        public byte Value => (byte) (IsPressed ? 0xFF : 0x00);

        #endregion
    }
}