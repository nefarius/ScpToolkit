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

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            Battery = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // reset buttons
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            // circle
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[1], 2));
            inputReport.Set(Ds3Axis.Circle, report[13]);

            // cross
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[1], 1));
            inputReport.Set(Ds3Axis.Cross, report[14]);

            // triangle
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[1], 3));
            inputReport.Set(Ds3Axis.Triangle, report[12]);

            // square
            inputReport.Set(Ds3Button.Square, IsBitSet(report[1], 0));
            inputReport.Set(Ds3Axis.Square, report[15]);

            // select
            inputReport.Set(Ds3Button.Select, IsBitSet(report[2], 0));

            // start
            inputReport.Set(Ds3Button.Start, IsBitSet(report[2], 1));



            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
