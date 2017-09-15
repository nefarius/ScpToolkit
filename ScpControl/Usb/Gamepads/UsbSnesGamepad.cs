using HidReport.Contract.Enums;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     DragonRise Inc. Usb Gamepad SNES
    /// </summary>
    public class UsbSnesGamepad : UsbGenericGamepad
    {
        public UsbSnesGamepad()
        {
            VendorId = 0x0079;
            ProductId = 0x0011;
        }

        protected override void ParseHidReport(byte[] report)
        {
            if (report[1] != 0x01) return;

            PacketCounter++;

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[7], 4)); // Select
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[7], 5)); // Start
            
            inputReport.Set(ButtonsEnum.L1, IsBitSet(report[7], 0)); // L1 (button)
            inputReport.Set(ButtonsEnum.R1, IsBitSet(report[7], 2)); // R1 (button)

            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[6], 4)); // Triangle (button)
            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[6], 5)); // Circle (button)
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[6], 6)); // Cross (button)
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[6], 7)); // Square (button)

            inputReport.Set(ButtonsEnum.Right, (report[4] == 0xFF)); // D-Pad right
            inputReport.Set(ButtonsEnum.Left, (report[4] == 0x00)); // D-Pad left
            inputReport.Set(ButtonsEnum.Up, (report[5] == 0x00)); // D-Pad up
            inputReport.Set(ButtonsEnum.Down, (report[5] == 0xFF)); // D-Pad down

            // This device has no thumb sticks, center axes
            inputReport.Set(AxesEnum.Lx, 0x80);
            inputReport.Set(AxesEnum.Ly, 0x80);
            inputReport.Set(AxesEnum.Rx, 0x80);
            inputReport.Set(AxesEnum.Ry, 0x80);

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
