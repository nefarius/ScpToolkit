using HidReport.Contract.Enums;
using ScpControl.HidParser;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     LSI Logic Gamepad
    /// </summary>
    public class UsbLsiLogicGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[2] != 0x00) return;

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = ++PacketCounter;

            // control buttons
            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[6], 4));
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[6], 5));

            // Left shoulder
            inputReport.Set(ButtonsEnum.L1, IsBitSet(report[6], 0));

            // Right shoulder
            inputReport.Set(ButtonsEnum.R1, IsBitSet(report[6], 1));

            // Left trigger
            inputReport.Set(ButtonsEnum.L2, IsBitSet(report[6], 2));

            // Right trigger
            inputReport.Set(ButtonsEnum.R2, IsBitSet(report[6], 3));

            // Triangle
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[5], 4));

            // Circle
            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[5], 5));

            // Cross
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[5], 6));

            // Square
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[5], 7));

            // Left thumb
            inputReport.Set(ButtonsEnum.L3, IsBitSet(report[6], 6));
            // Right thumb
            inputReport.Set(ButtonsEnum.R3, IsBitSet(report[6], 7));

            var dPad = (byte)(report[5] & ~0xF0);

            // D-Pad
            HidParsers.ParseDPad(dPad, inputReport);

            // Left thumb stick
            inputReport.Set(AxesEnum.Lx, report[0]);
            inputReport.Set(AxesEnum.Ly, report[1]);
            
            // Right thumb stick
            inputReport.Set(AxesEnum.Rx, report[3]);
            inputReport.Set(AxesEnum.Ry, report[4]);
            
            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
