using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    public class UsbTwinUsbJoystick : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[0] != 0x01) return;

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            Battery = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = ++PacketCounter;

            // null button states
            inputReport.ZeroPsButtonState();
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            inputReport.Set(Ds3Button.Circle, IsBitSet(report[5], 5));
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[5], 6));
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[5], 4));
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[5], 7));

            // TODO: implement!

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
