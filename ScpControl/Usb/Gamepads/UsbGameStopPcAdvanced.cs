using HidReport.Contract.Enums;
using ScpControl.HidParser;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     GameStop PC Advanced Controller
    /// </summary>
    public class UsbGameStopPcAdvanced : UsbGenericGamepad
    {
        public UsbGameStopPcAdvanced()
        {
            VendorId = 0x11FF;
            ProductId = 0x3331;
        }

        protected override void ParseHidReport(byte[] report)
        {
            if (report[8] != 0xC0 && report[8] != 0x40) return;

            PacketCounter++;

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // reset buttons
            // buttons equaly reported in both modes
            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[6], 5));
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[6], 6));
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[6], 4));
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[6], 7));

            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[7], 4));
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[7], 5));

            inputReport.Set(ButtonsEnum.L1, IsBitSet(report[7], 0));
            inputReport.Set(ButtonsEnum.R1, IsBitSet(report[7], 1));
            inputReport.Set(ButtonsEnum.L2, IsBitSet(report[7], 2));
            inputReport.Set(ButtonsEnum.R2, IsBitSet(report[7], 3));

            inputReport.Set(ButtonsEnum.L3, IsBitSet(report[7], 6));
            inputReport.Set(ButtonsEnum.R3, IsBitSet(report[7], 7));

            // detect mode it's running in
            switch (report[8])
            {
                case 0xC0: // mode 1
                {
                    inputReport.Set(ButtonsEnum.Up, (report[2] == 0x00));
                    inputReport.Set(ButtonsEnum.Right, (report[1] == 0xFF));
                    inputReport.Set(ButtonsEnum.Down, (report[2] == 0xFF));
                    inputReport.Set(ButtonsEnum.Left, (report[1] == 0x00));

                    // mode 1 doesn't report the thumb sticks
                    inputReport.Set(AxesEnum.Lx, 0x80);
                    inputReport.Set(AxesEnum.Ly, 0x80);
                    inputReport.Set(AxesEnum.Rx, 0x80);
                    inputReport.Set(AxesEnum.Ry, 0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[6] & ~0xF0);
                    HidParsers.ParseDPad(dPad, inputReport);
                    inputReport.Set(AxesEnum.Lx, report[1]);
                    inputReport.Set(AxesEnum.Ly, report[2]);

                    inputReport.Set(AxesEnum.Rx, report[4]);
                    inputReport.Set(AxesEnum.Ry, report[5]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}