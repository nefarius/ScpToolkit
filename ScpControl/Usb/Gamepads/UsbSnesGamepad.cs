using ScpControl.ScpCore;
using Ds3Button = ScpControl.Profiler.Ds3Button;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     DragonRise Inc. USB Gamepad SNES
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

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.PacketCounter = PacketCounter;

            // reset buttons
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            InputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4)); // Select
            InputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5)); // Start
            
            InputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0)); // L1 (button)
            InputReport.Set(Ds3Button.R1, IsBitSet(report[7], 2)); // R1 (button)

            InputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4)); // Triangle (button)
            InputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5)); // Circle (button)
            InputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6)); // Cross (button)
            InputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7)); // Square (button)

            InputReport.Set(Ds3Button.Right, (report[4] == 0xFF)); // D-Pad right
            InputReport.Set(Ds3Button.Left, (report[4] == 0x00)); // D-Pad left
            InputReport.Set(Ds3Button.Up, (report[5] == 0x00)); // D-Pad up
            InputReport.Set(Ds3Button.Down, (report[5] == 0xFF)); // D-Pad down

            // This device has no thumb sticks, center axes
            InputReport.SetLeftAxisY(0x80);
            InputReport.SetLeftAxisX(0x80);
            InputReport.SetRightAxisY(0x80);
            InputReport.SetRightAxisX(0x80);

            #endregion

            OnHidReportReceived();
        }
    }
}
