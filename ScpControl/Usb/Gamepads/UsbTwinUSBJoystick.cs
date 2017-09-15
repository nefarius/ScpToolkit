
using HidReport.Contract.Core;
using HidReport.Contract.Enums;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    public class UsbTwinUsbJoystick : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[0] != 0x01) return;

            HidReport.Core.HidReport inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = ++PacketCounter;

            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[5], 5));
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[5], 6));
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[5], 4));
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[5], 7));

            // TODO: implement!

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
