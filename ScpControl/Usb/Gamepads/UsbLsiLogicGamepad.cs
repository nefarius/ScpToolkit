using System.Net.NetworkInformation;
using ScpControl.Profiler;
using ScpControl.ScpCore;
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

            if (PacketCounter++ + 1 < PacketCounter)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", PacketCounter);
                PacketCounter = 0;
            }

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.PacketCounter = PacketCounter;

            // set fake MAC address
            InputReport.PadMacAddress = PhysicalAddress.Parse(m_Mac.Replace(":", string.Empty));

            // null button states
            InputReport.ZeroPsButtonState();
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            // control buttons
            InputReport.Set(Ds3Button.Select, IsBitSet(report[6], 4));
            InputReport.Set(Ds3Button.Start, IsBitSet(report[6], 5));

            // Left shoulder
            InputReport.Set(Ds3Button.L1, IsBitSet(report[6], 0));

            // Right shoulder
            InputReport.Set(Ds3Button.R1, IsBitSet(report[6], 1));

            // Left trigger
            InputReport.Set(Ds3Button.L2, IsBitSet(report[6], 2));

            // Right trigger
            InputReport.Set(Ds3Button.R2, IsBitSet(report[6], 3));

            // Triangle
            InputReport.Set(Ds3Button.Triangle, IsBitSet(report[5], 4));

            // Circle
            InputReport.Set(Ds3Button.Circle, IsBitSet(report[5], 5));

            // Cross
            InputReport.Set(Ds3Button.Cross, IsBitSet(report[5], 6));

            // Square
            InputReport.Set(Ds3Button.Square, IsBitSet(report[5], 7));

            // Left thumb
            InputReport.Set(Ds3Button.L3, IsBitSet(report[6], 6));
            // Right thumb
            InputReport.Set(Ds3Button.R3, IsBitSet(report[6], 7));

            var dPad = (byte)(report[5] & ~0xF0);

            // D-Pad
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

            // Left thumb stick
            InputReport.Set(Ds3Axis.Lx, report[0]);
            InputReport.Set(Ds3Axis.Ly, report[1]);
            
            // Right thumb stick
            InputReport.Set(Ds3Axis.Rx, report[3]);
            InputReport.Set(Ds3Axis.Ry, report[4]);
            
            #endregion

            OnHidReportReceived();
        }
    }
}
