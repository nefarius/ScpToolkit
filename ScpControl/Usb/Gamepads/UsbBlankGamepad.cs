using System;
using ScpControl.Utilities;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     This Gamepad is used only by the Analyzer to receive incoming HID Reports.
    /// </summary>
    public class UsbBlankGamepad : UsbDevice
    {
        // randomly generated GUID so no other devices may be accidentally affected
        public static readonly Guid DeviceClassGuid = Guid.Parse("433FA0C6-2BF1-4675-98C6-7F4FC99796FC");
        private readonly DumpHelper _dumper;

        #region Ctors

        public UsbBlankGamepad()
            : base(DeviceClassGuid.ToString())
        {
        }

        public UsbBlankGamepad(string header, string dumpFileName) : this()
        {
            _dumper = new DumpHelper(header, dumpFileName);
        }

        #endregion

        public CaptureType Capture { private get; set; }

        protected override void Parse(byte[] report)
        {
            if (Capture != CaptureType.Default)
            {
                _dumper.DumpArray(Capture.ToString(), report, report.Length);
                Capture = CaptureType.Default;
            }
        }
    }

    public enum CaptureType
    {
        Default,
        Nothing,
        Circle,
        Cross,
        Triangle,
        Square,
        Select,
        Start,
        DpadUp,
        DpadUpAndRight,
        DpadRight,
        DpadRightAndDown,
        DpadDown,
        DpadDownAndLeft,
        DpadLeft,
        DpadLeftAndUp,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger,
        LeftThumb,
        RightThumb,
        LeftStickRight,
        LeftStickLeft,
        LeftStickUp,
        LeftStickDown,
        RightStickRight,
        RightStickLeft,
        RichtStickUp,
        RightStickDown,
        Ps
    }
}
