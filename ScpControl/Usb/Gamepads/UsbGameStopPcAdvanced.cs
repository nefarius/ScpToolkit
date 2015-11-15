using System.Net.NetworkInformation;
using ScpControl.ScpCore;
using Ds3Axis = ScpControl.Profiler.Ds3Axis;
using Ds3Button = ScpControl.Profiler.Ds3Button;

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

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.PacketCounter = PacketCounter;

            // set fake MAC address
            InputReport.PadMacAddress = PhysicalAddress.Parse(m_Mac.Replace(":", string.Empty));

            // reset buttons
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            // buttons equaly reported in both modes
            InputReport.Set(Ds3Button.Circle, IsBitSet(report[6], 5));
            InputReport.Set(Ds3Button.Cross, IsBitSet(report[6], 6));
            InputReport.Set(Ds3Button.Triangle, IsBitSet(report[6], 4));
            InputReport.Set(Ds3Button.Square, IsBitSet(report[6], 7));

            InputReport.Set(Ds3Button.Select, IsBitSet(report[7], 4));
            InputReport.Set(Ds3Button.Start, IsBitSet(report[7], 5));

            InputReport.Set(Ds3Button.L1, IsBitSet(report[7], 0));
            InputReport.Set(Ds3Button.R1, IsBitSet(report[7], 1));
            InputReport.Set(Ds3Button.L2, IsBitSet(report[7], 2));
            InputReport.Set(Ds3Button.R2, IsBitSet(report[7], 3));

            InputReport.Set(Ds3Button.L3, IsBitSet(report[7], 6));
            InputReport.Set(Ds3Button.R3, IsBitSet(report[7], 7));

            // detect mode it's running in
            switch (report[8])
            {
                case 0xC0: // mode 1
                {
                    InputReport.Set(Ds3Button.Up, (report[2] == 0x00));
                    InputReport.Set(Ds3Button.Right, (report[1] == 0xFF));
                    InputReport.Set(Ds3Button.Down, (report[2] == 0xFF));
                    InputReport.Set(Ds3Button.Left, (report[1] == 0x00));

                    // mode 1 doesn't report the thumb sticks
                    InputReport.Set(Ds3Axis.Lx, 0x80);
                    InputReport.Set(Ds3Axis.Ly, 0x80);
                    InputReport.Set(Ds3Axis.Rx, 0x80);
                    InputReport.Set(Ds3Axis.Ry, 0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[6] & ~0xF0);

                    switch (dPad)
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

                    InputReport.Set(Ds3Axis.Lx, report[1]);
                    InputReport.Set(Ds3Axis.Ly, report[2]);

                    InputReport.Set(Ds3Axis.Rx, report[4]);
                    InputReport.Set(Ds3Axis.Ry, report[5]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived();
        }
    }
}