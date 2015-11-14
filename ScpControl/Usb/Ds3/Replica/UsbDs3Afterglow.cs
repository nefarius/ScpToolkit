using ScpControl.Profiler;

namespace ScpControl.Usb.Ds3.Replica
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[26] != 0x02) return;

            PacketCounter++;

            #region HID Report translation

            // battery
            m_BatteryStatus = InputReport.BatteryStatus = report[30];

            // packet counter
            InputReport.PacketCounter = PacketCounter;

            // null button states
            InputReport.ZeroPsButtonState();
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            // control buttons
            InputReport.Set(Ds3Button.Ps, IsBitSet(report[1], 4));
            InputReport.Set(Ds3Button.Select, IsBitSet(report[1], 0));
            InputReport.Set(Ds3Button.Start, IsBitSet(report[1], 1));

            // Left shoulder
            InputReport.Set(Ds3Button.L1, IsBitSet(report[0], 4));
            InputReport.Set(Ds3Axis.L1, report[15]);

            // Right shoulder
            InputReport.Set(Ds3Button.R1, IsBitSet(report[0], 5));
            InputReport.Set(Ds3Axis.R1, report[16]);

            // Left trigger
            InputReport.Set(Ds3Button.L2, IsBitSet(report[0], 6));
            InputReport.Set(Ds3Axis.L2, report[17]);

            // Right trigger
            InputReport.Set(Ds3Button.R2, IsBitSet(report[0], 7));
            InputReport.Set(Ds3Axis.R2, report[18]);

            // Triangle
            InputReport.Set(Ds3Button.Triangle, IsBitSet(report[0], 3));
            InputReport.Set(Ds3Axis.Triangle, report[11]);

            // Circle
            InputReport.Set(Ds3Button.Circle, IsBitSet(report[0], 2));
            InputReport.Set(Ds3Axis.Circle, report[12]);

            // Cross
            InputReport.Set(Ds3Button.Cross, IsBitSet(report[0], 1));
            InputReport.Set(Ds3Axis.Cross, report[13]);

            // Square
            InputReport.Set(Ds3Button.Square, IsBitSet(report[0], 0));
            InputReport.Set(Ds3Axis.Square, report[14]);

            // Left thumb
            InputReport.Set(Ds3Button.L3, IsBitSet(report[1], 2));
            // Right thumb
            InputReport.Set(Ds3Button.R3, IsBitSet(report[1], 3));

            // D-Pad
            if (report[2] != 0x0F)
            {
                switch (report[2])
                {
                    case 0:
                        InputReport.Set(Ds3Button.Up);
                        break;
                    case 1:
                        InputReport.Set(Ds3Button.Up);
                        InputReport.Set(Ds3Button.Right);
                        break;
                    case 2:
                        InputReport.Set(Ds3Button.Right);
                        break;
                    case 3:
                        InputReport.Set(Ds3Button.Right);
                        InputReport.Set(Ds3Button.Down);
                        break;
                    case 4:
                        InputReport.Set(Ds3Button.Down);
                        break;
                    case 5:
                        InputReport.Set(Ds3Button.Down);
                        InputReport.Set(Ds3Button.Left);
                        break;
                    case 6:
                        InputReport.Set(Ds3Button.Left);
                        break;
                    case 7:
                        InputReport.Set(Ds3Button.Left);
                        InputReport.Set(Ds3Button.Up);
                        break;
                }
            }

            // Left thumb stick
            InputReport.Set(Ds3Axis.Lx, report[3]);
            InputReport.Set(Ds3Axis.Ly, report[4]);

            // Right thumb stick
            InputReport.Set(Ds3Axis.Rx, report[5]);
            InputReport.Set(Ds3Axis.Ry, report[6]);

            #endregion

            OnHidReportReceived();
        }

        public override bool Pair(byte[] master)
        {
            // controller uses it's own wireless protocol, pairing is not needed
            return true;
        }
    }
}
