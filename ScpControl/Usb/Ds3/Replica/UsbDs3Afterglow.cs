using System.Net.NetworkInformation;
using HidReport.Contract.Enums;
using ScpControl.HidParser;
using ScpControl.Shared.Core;

namespace ScpControl.Usb.Ds3.Replica
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[26] != 0x02) return;

            PacketCounter++;

            var inputReport = new HidReport.Core.HidReport();

            #region HID Report translation

            // battery
            inputReport.BatteryStatus = (DsBattery) report[30];

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // control buttons
            inputReport.Set(ButtonsEnum.Ps, IsBitSet(report[1], 4));
            inputReport.Set(ButtonsEnum.Select, IsBitSet(report[1], 0));
            inputReport.Set(ButtonsEnum.Start, IsBitSet(report[1], 1));

            // Left shoulder
            inputReport.Set(ButtonsEnum.L1, IsBitSet(report[0], 4));
            inputReport.Set(AxesEnum.L1, report[15]);

            // Right shoulder
            inputReport.Set(ButtonsEnum.R1, IsBitSet(report[0], 5));
            inputReport.Set(AxesEnum.R1, report[16]);

            // Left trigger
            inputReport.Set(ButtonsEnum.L2, IsBitSet(report[0], 6));
            inputReport.Set(AxesEnum.L2, report[17]);

            // Right trigger
            inputReport.Set(ButtonsEnum.R2, IsBitSet(report[0], 7));
            inputReport.Set(AxesEnum.R2, report[18]);

            // Triangle
            inputReport.Set(ButtonsEnum.Triangle, IsBitSet(report[0], 3));
            inputReport.Set(AxesEnum.Triangle, report[11]);

            // Circle
            inputReport.Set(ButtonsEnum.Circle, IsBitSet(report[0], 2));
            inputReport.Set(AxesEnum.Circle, report[12]);

            // Cross
            inputReport.Set(ButtonsEnum.Cross, IsBitSet(report[0], 1));
            inputReport.Set(AxesEnum.Cross, report[13]);

            // Square
            inputReport.Set(ButtonsEnum.Square, IsBitSet(report[0], 0));
            inputReport.Set(AxesEnum.Square, report[14]);

            // Left thumb
            inputReport.Set(ButtonsEnum.L3, IsBitSet(report[1], 2));
            // Right thumb
            inputReport.Set(ButtonsEnum.R3, IsBitSet(report[1], 3));

            // D-Pad
            if (report[2] != 0x0F)
            {
                HidParsers.ParseDPad(report[2], inputReport);
            }

            // Left thumb stick
            inputReport.Set(AxesEnum.Lx, report[3]);
            inputReport.Set(AxesEnum.Ly, report[4]);

            // Right thumb stick
            inputReport.Set(AxesEnum.Rx, report[5]);
            inputReport.Set(AxesEnum.Ry, report[6]);

            #endregion

            OnHidReportReceived(inputReport);
        }

        public override bool Pair(PhysicalAddress master)
        {
            // controller uses it's own wireless protocol, pairing is not needed
            return true;
        }
    }
}
