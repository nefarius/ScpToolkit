using ScpControl.Shared.Core;

namespace ScpControl.Usb.Ds3.Replica
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[26] != 0x02) return;

            PacketCounter++;

            var inputReport = NewHidReport();

            #region HID Report translation

            // battery
            Battery = (DsBattery) report[30];

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // null button states
            inputReport.ZeroPsButtonState();
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            // control buttons
            inputReport.Set(Ds3Button.Ps, IsBitSet(report[1], 4));
            inputReport.Set(Ds3Button.Select, IsBitSet(report[1], 0));
            inputReport.Set(Ds3Button.Start, IsBitSet(report[1], 1));

            // Left shoulder
            inputReport.Set(Ds3Button.L1, IsBitSet(report[0], 4));
            inputReport.Set(Ds3Axis.L1, report[15]);

            // Right shoulder
            inputReport.Set(Ds3Button.R1, IsBitSet(report[0], 5));
            inputReport.Set(Ds3Axis.R1, report[16]);

            // Left trigger
            inputReport.Set(Ds3Button.L2, IsBitSet(report[0], 6));
            inputReport.Set(Ds3Axis.L2, report[17]);

            // Right trigger
            inputReport.Set(Ds3Button.R2, IsBitSet(report[0], 7));
            inputReport.Set(Ds3Axis.R2, report[18]);

            // Triangle
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[0], 3));
            inputReport.Set(Ds3Axis.Triangle, report[11]);

            // Circle
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[0], 2));
            inputReport.Set(Ds3Axis.Circle, report[12]);

            // Cross
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[0], 1));
            inputReport.Set(Ds3Axis.Cross, report[13]);

            // Square
            inputReport.Set(Ds3Button.Square, IsBitSet(report[0], 0));
            inputReport.Set(Ds3Axis.Square, report[14]);

            // Left thumb
            inputReport.Set(Ds3Button.L3, IsBitSet(report[1], 2));
            // Right thumb
            inputReport.Set(Ds3Button.R3, IsBitSet(report[1], 3));

            // D-Pad
            if (report[2] != 0x0F)
            {
                switch (report[2])
                {
                    case 0:
                        inputReport.Set(Ds3Button.Up);
                        break;
                    case 1:
                        inputReport.Set(Ds3Button.Up);
                        inputReport.Set(Ds3Button.Right);
                        break;
                    case 2:
                        inputReport.Set(Ds3Button.Right);
                        break;
                    case 3:
                        inputReport.Set(Ds3Button.Right);
                        inputReport.Set(Ds3Button.Down);
                        break;
                    case 4:
                        inputReport.Set(Ds3Button.Down);
                        break;
                    case 5:
                        inputReport.Set(Ds3Button.Down);
                        inputReport.Set(Ds3Button.Left);
                        break;
                    case 6:
                        inputReport.Set(Ds3Button.Left);
                        break;
                    case 7:
                        inputReport.Set(Ds3Button.Left);
                        inputReport.Set(Ds3Button.Up);
                        break;
                }
            }

            // Left thumb stick
            inputReport.Set(Ds3Axis.Lx, report[3]);
            inputReport.Set(Ds3Axis.Ly, report[4]);

            // Right thumb stick
            inputReport.Set(Ds3Axis.Rx, report[5]);
            inputReport.Set(Ds3Axis.Ry, report[6]);

            #endregion

            OnHidReportReceived(inputReport);
        }

        public override bool Pair(byte[] master)
        {
            // controller uses it's own wireless protocol, pairing is not needed
            return true;
        }
    }
}
