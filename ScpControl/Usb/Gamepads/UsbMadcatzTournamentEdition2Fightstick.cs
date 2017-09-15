using HidReport.Contract.Enums;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    public class UsbMadcatzTournamentEdition2Fightstick : UsbGenericGamepad
    {
        public UsbMadcatzTournamentEdition2Fightstick()
        {
            VendorId = 0738;
            ProductId = 3480;
        }

        protected override void ParseHidReport(byte[] report)
        {
            PacketCounter++;

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // circle
            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[1], 2));
            inputReport.Set(AxesEnum.Circle, report[13]);

            // cross
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[1], 1));
            inputReport.Set(AxesEnum.Cross, report[14]);

            // triangle
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[1], 3));
            inputReport.Set(AxesEnum.Triangle, report[12]);

            // square
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[1], 0));
            inputReport.Set(AxesEnum.Square, report[15]);

            // select
            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[2], 0));

            // start
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[2], 1));



            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
