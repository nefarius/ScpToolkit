using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using Ds3Button = ScpControl.Shared.Core.Ds3Button;

namespace ScpControl.Usb.Gamepads
{
    public class UsbBadBigBenGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[5] != 0x00) return;

            PacketCounter++;

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.PacketCounter = PacketCounter;

            InputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4));
            InputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5));
            InputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6));
            InputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7));

            InputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4));
            InputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5));

            InputReport.Set(Ds3Button.Up, (report[4] == 0x00));
            InputReport.Set(Ds3Button.Right, (report[3] == 0xFF));
            InputReport.Set(Ds3Button.Down, (report[4] == 0xFF));
            InputReport.Set(Ds3Button.Left, (report[3] == 0x00));

            InputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0));
            InputReport.Set(Ds3Button.R1, IsBitSet(report[7], 1));
            InputReport.Set(Ds3Button.L2, IsBitSet(report[7], 2));
            InputReport.Set(Ds3Button.R2, IsBitSet(report[7], 3));

            InputReport.Set(Ds3Button.L3, IsBitSet(report[7], 6));
            InputReport.Set(Ds3Button.R3, IsBitSet(report[7], 7));
            
            // TODO: the PS-button is dead according to the report:
            // http://forums.pcsx2.net/attachment.php?aid=57420

            #endregion

            OnHidReportReceived();
        }
    }
}
