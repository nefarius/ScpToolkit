using System;
using System.Collections.Generic;
using System.Linq;
using HidReport.Contract.Core;
using HidReport.Contract.DsActors;
using HidReport.Contract.Enums;
using HidReport.DsActors;

namespace HidReport.Core
{
    /// <summary>
    ///     Represents an extended HID Input Report ready to be sent to the virtual bus device.
    /// </summary>
    [Serializable]
    public sealed class HidReport : EventArgs, IScpHidReport
    {
        public byte ReportId { get; set; } = 0;

        public uint PacketCounter { get; set; } = 0;

        public DsBattery BatteryStatus { get; set; } = DsBattery.None;


        private readonly Dictionary<ButtonsEnum, DsButtonState> _buttonStates = new Dictionary<ButtonsEnum, DsButtonState>();
        private readonly Dictionary<AxesEnum, DsAxisState> _axesStates = new Dictionary<AxesEnum, DsAxisState>();

        /// <summary>
        ///     Sets the status of a digital (two-state) button.
        /// </summary>
        /// <param name="button">The button of the current report to manipulate.</param>
        /// <param name="value">True to set the button as pressed, false to set the button as released.</param>
        public void Set(ButtonsEnum button, bool value = true)
        {
            if(!_buttonStates.ContainsKey(button))
                _buttonStates.Add(button, new DsButtonState());
            _buttonStates[button].IsPressed = value;
        }

        /// <summary>
        ///     Sets the status of a digital (two-state) button to 'released'.
        /// </summary>
        /// <param name="button">The button of the current report to unset.</param>
        public void Unset(ButtonsEnum button)
        {
            Set(button, false);
        }

        /// <summary>
        ///     Sets the value of an analog axis.
        /// </summary>
        /// <param name="axis">The axis of the current report to manipulate.</param>
        /// <param name="value">The value to set the axis to.</param>
        public void Set(AxesEnum axis, byte value)
        {
            if (!_axesStates.ContainsKey(axis))
            {
                byte defaultValue = 0;
                switch (axis)
                {
                    case AxesEnum.Lx:
                    case AxesEnum.Ly:
                    case AxesEnum.Rx:
                    case AxesEnum.Ry:
                        defaultValue = 127;
                        break;
                    default:
                        defaultValue = 0;
                        break;
                }
                _axesStates.Add(axis, new DsAxisState(defaultValue));
            }

            _axesStates[axis].Value = value;
        }

        /// <summary>
        ///     Sets the value of an analog axis to it's default value.
        /// </summary>
        /// <param name="axis">The axis of the current report to manipulate.</param>
        public void Unset(AxesEnum axis)
        {
            Set(axis, _axesStates[axis].DefaultValue);
        }

        /// <summary>
        ///     Gets the motion data from the DualShock accelerometer sensor.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4/blob/master/index.js</remarks>
        public DsAccelerometer MotionMutable { get; set; } = new DsAccelerometer();

        public IDsAccelerometerImmutable Motion => MotionMutable;

        /// <summary>
        ///     Gets the orientation data from the DualShock gyroscope sensor.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4/blob/master/index.js</remarks>
        public DsGyroscope OrientationMutable { get; set; } = new DsGyroscope();
        public IDsGyroscopeImmutable Orientation => OrientationMutable;
        /// <summary>
        ///     The first touch spot on the DualShock 4 track pad.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4</remarks>
        public DsTrackPadTouch TrackPadTouch0Mutable { get; set; } = new DsTrackPadTouch();

        public IDsTrackPadTouchImmutable TrackPadTouch0 => TrackPadTouch0Mutable;
        /// <summary>
        ///     The second touch spot on the DualShock 4 track pad.
        /// </summary>
        /// <remarks>https://github.com/ehd/node-ds4</remarks>
        public DsTrackPadTouch TrackPadTouch1Mutable { get; set; } = new DsTrackPadTouch();
        public IDsTrackPadTouchImmutable TrackPadTouch1 => TrackPadTouch1Mutable;

        /// <summary>
        ///     Checks if a given button state is engaged in the current packet.
        /// </summary>
        /// <param name="button">The DualShock button to question.</param>
        /// <returns>True if the button is pressed, false if the button is released.</returns>
        public IDsButtonState this[ButtonsEnum button] => _buttonStates[button];

        /// <summary>
        ///     Gets the axis state of the current packet.
        /// </summary>
        /// <param name="axis">The DualShock axis to question.</param>
        /// <returns>The value of the axis in question.</returns>
        public IDsAxisStateImmutable this[AxesEnum axis] => _axesStates[axis];

        public bool IsPadActive
        {
            get
            {
                return _buttonStates.Any(p => p.Value.IsPressed)
                    || _axesStates.Any(p => p.Value.Value != p.Value.DefaultValue);
            }
        }

    }
}