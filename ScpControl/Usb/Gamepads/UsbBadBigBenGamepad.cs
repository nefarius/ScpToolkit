using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    public class UsbBadBigBenGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[5] != 0x00) return;

            PacketCounter++;

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = inputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4));
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5));
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6));
            inputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7));

            inputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4));
            inputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5));

            inputReport.Set(Ds3Button.Up, (report[4] == 0x00));
            inputReport.Set(Ds3Button.Right, (report[3] == 0xFF));
            inputReport.Set(Ds3Button.Down, (report[4] == 0xFF));
            inputReport.Set(Ds3Button.Left, (report[3] == 0x00));

            inputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0));
            inputReport.Set(Ds3Button.R1, IsBitSet(report[7], 1));
            inputReport.Set(Ds3Button.L2, IsBitSet(report[7], 2));
            inputReport.Set(Ds3Button.R2, IsBitSet(report[7], 3));

            inputReport.Set(Ds3Button.L3, IsBitSet(report[7], 6));
            inputReport.Set(Ds3Button.R3, IsBitSet(report[7], 7));
            
            // TODO: the PS-button is dead according to the report:
            // http://forums.pcsx2.net/attachment.php?aid=57420

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
