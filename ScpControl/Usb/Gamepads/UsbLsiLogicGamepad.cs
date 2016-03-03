using ScpControl.Shared.Core;
using Ds3Axis = ScpControl.Shared.Core.Ds3Axis;
using Ds3Button = ScpControl.Shared.Core.Ds3Button;

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

            // control buttons
            inputReport.Set(Ds3Button.Select, IsBitSet(report[6], 4));
            inputReport.Set(Ds3Button.Start, IsBitSet(report[6], 5));

            // Left shoulder
            inputReport.Set(Ds3Button.L1, IsBitSet(report[6], 0));

            // Right shoulder
            inputReport.Set(Ds3Button.R1, IsBitSet(report[6], 1));

            // Left trigger
            inputReport.Set(Ds3Button.L2, IsBitSet(report[6], 2));

            // Right trigger
            inputReport.Set(Ds3Button.R2, IsBitSet(report[6], 3));

            // Triangle
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[5], 4));

            // Circle
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[5], 5));

            // Cross
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[5], 6));

            // Square
            inputReport.Set(Ds3Button.Square, IsBitSet(report[5], 7));

            // Left thumb
            inputReport.Set(Ds3Button.L3, IsBitSet(report[6], 6));
            // Right thumb
            inputReport.Set(Ds3Button.R3, IsBitSet(report[6], 7));

            var dPad = (byte)(report[5] & ~0xF0);

            // D-Pad
            switch (dPad)
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

            // Left thumb stick
            inputReport.Set(Ds3Axis.Lx, report[0]);
            inputReport.Set(Ds3Axis.Ly, report[1]);
            
            // Right thumb stick
            inputReport.Set(Ds3Axis.Rx, report[3]);
            inputReport.Set(Ds3Axis.Ry, report[4]);
            
            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
