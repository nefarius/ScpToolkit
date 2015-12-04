using System.Net.NetworkInformation;
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

            var inputReport = NewHidReport();

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = inputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            inputReport.PacketCounter = PacketCounter;
            
            // set fake MAC address
            inputReport.PadMacAddress = PhysicalAddress.Parse(m_Mac.Replace(":", string.Empty));

            // reset buttons
            inputReport.ZeroSelectStartButtonsState();
            inputReport.ZeroShoulderButtonsState();

            inputReport.Set(Ds3Button.Circle, IsBitSet(report[5], 5));
            inputReport.Set(Ds3Button.Cross, IsBitSet(report[5], 6));

            inputReport.Set(Ds3Button.Select, IsBitSet(report[6], 4));
            inputReport.Set(Ds3Button.Start, IsBitSet(report[6], 5));

            var dPad = (byte)(report[5] & ~0xF0);

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

            inputReport.Set(Ds3Button.Triangle, IsBitSet(report[5], 4));
            inputReport.Set(Ds3Button.Square, IsBitSet(report[5], 7));

            inputReport.Set(Ds3Button.L1, IsBitSet(report[6], 0));
            inputReport.Set(Ds3Button.R1, IsBitSet(report[6], 1));

            inputReport.Set(Ds3Button.L2, IsBitSet(report[6], 2));
            inputReport.Set(Ds3Button.R2, IsBitSet(report[6], 3));

            inputReport.Set(Ds3Button.L3, IsBitSet(report[6], 6));
            inputReport.Set(Ds3Button.R3, IsBitSet(report[6], 7));

            inputReport.Set(Ds3Axis.Lx, report[3]);
            inputReport.Set(Ds3Axis.Ly, report[4]);

            inputReport.Set(Ds3Axis.Rx, report[1]);
            inputReport.Set(Ds3Axis.Ry, report[2]);
            
            #endregion

            OnHidReportReceived(inputReport);
        }
    }
}
