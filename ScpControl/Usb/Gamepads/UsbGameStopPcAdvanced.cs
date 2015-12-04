using System.Net.NetworkInformation;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using Ds3Axis = ScpControl.Shared.Core.Ds3Axis;
using Ds3Button = ScpControl.Shared.Core.Ds3Button;

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

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is Usb-powered
            Battery = DsBattery.None;

            // packet counter
            inputReport.PacketCounter = PacketCounter;

            // set fake MAC address
            inputReport.PadMacAddress = PhysicalAddress.Parse(m_Mac.Replace(":", string.Empty));

            // reset buttons
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            // buttons equaly reported in both modes
            inputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5));
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6));
            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4));
            inputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7));

            inputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4));
            inputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5));

            inputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0));
            inputReport.Set(Ds3Button.R1, IsBitSet(report[7], 1));
            inputReport.Set(Ds3Button.L2, IsBitSet(report[7], 2));
            inputReport.Set(Ds3Button.R2, IsBitSet(report[7], 3));

            inputReport.Set(Ds3Button.L3, IsBitSet(report[7], 6));
            inputReport.Set(Ds3Button.R3, IsBitSet(report[7], 7));

            // detect mode it's running in
            switch (report[8])
            {
                case 0xC0: // mode 1
                {
                    inputReport.Set(Ds3Button.Up, (report[2] == 0x00));
                    inputReport.Set(Ds3Button.Right, (report[1] == 0xFF));
                    inputReport.Set(Ds3Button.Down, (report[2] == 0xFF));
                    inputReport.Set(Ds3Button.Left, (report[1] == 0x00));

                    // mode 1 doesn't report the thumb sticks
                    inputReport.Set(Ds3Axis.Lx, 0x80);
                    inputReport.Set(Ds3Axis.Ly, 0x80);
                    inputReport.Set(Ds3Axis.Rx, 0x80);
                    inputReport.Set(Ds3Axis.Ry, 0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[6] & ~0xF0);

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

                    inputReport.Set(Ds3Axis.Lx, report[1]);
                    inputReport.Set(Ds3Axis.Ly, report[2]);

                    inputReport.Set(Ds3Axis.Rx, report[4]);
                    inputReport.Set(Ds3Axis.Ry, report[5]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}