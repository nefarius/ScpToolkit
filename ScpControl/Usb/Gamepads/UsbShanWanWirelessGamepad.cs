using System.Net.NetworkInformation;
using HidReport.Contract.Enums;
using ScpControl.HidParser;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     ShanWan Wireless Gamepad
    /// </summary>
    public class UsbShanWanWirelessGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[7] != 0x00) return;

            if (PacketCounter++ + 1 < PacketCounter)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", PacketCounter);
                PacketCounter = 0;
            }

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            inputReport.BatteryStatus = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[5], 5));
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[5], 6));

            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[6], 4));
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[6], 5));

            var dPad = (byte)(report[5] & ~0xF0);
            HidParsers.ParseDPad(dPad, inputReport);

            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[5], 4));
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[5], 7));

            inputReport.Set(ButtonsEnum.L1, IsBitSet(report[6], 0));
            inputReport.Set(ButtonsEnum.R1, IsBitSet(report[6], 1));

            inputReport.Set(ButtonsEnum.L2, IsBitSet(report[6], 2));
            inputReport.Set(ButtonsEnum.R2, IsBitSet(report[6], 3));

            inputReport.Set(ButtonsEnum.L3, IsBitSet(report[6], 6));
            inputReport.Set(ButtonsEnum.R3, IsBitSet(report[6], 7));

            inputReport.Set(AxesEnum.Lx, report[3]);
            inputReport.Set(AxesEnum.Ly, report[4]);

            inputReport.Set(AxesEnum.Rx, report[1]);
            inputReport.Set(AxesEnum.Ry, report[2]);
            
            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
