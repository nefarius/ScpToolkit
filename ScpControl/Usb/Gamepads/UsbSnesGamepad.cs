using System.Net.NetworkInformation;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using Ds3Axis = ScpControl.Shared.Core.Ds3Axis;
using Ds3Button = ScpControl.Shared.Core.Ds3Button;

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

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            Battery = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // set fake MAC address
            inputReport.PadMacAddress = PhysicalAddress.Parse(m_Mac.Replace(":", string.Empty));

            // reset buttons
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            inputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4)); // Select
            inputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5)); // Start
            
            inputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0)); // L1 (button)
            inputReport.Set(Ds3Button.R1, IsBitSet(report[7], 2)); // R1 (button)

            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4)); // Triangle (button)
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5)); // Circle (button)
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6)); // Cross (button)
            inputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7)); // Square (button)

            inputReport.Set(Ds3Button.Right, (report[4] == 0xFF)); // D-Pad right
            inputReport.Set(Ds3Button.Left, (report[4] == 0x00)); // D-Pad left
            inputReport.Set(Ds3Button.Up, (report[5] == 0x00)); // D-Pad up
            inputReport.Set(Ds3Button.Down, (report[5] == 0xFF)); // D-Pad down

            // This device has no thumb sticks, center axes
            inputReport.Set(Ds3Axis.Lx, 0x80);
            inputReport.Set(Ds3Axis.Ly, 0x80);
            inputReport.Set(Ds3Axis.Rx, 0x80);
            inputReport.Set(Ds3Axis.Ry, 0x80);

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
