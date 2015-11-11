using ScpControl.ScpCore;

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

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.SetPacketCounter(PacketCounter);

            // reset buttons
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            InputReport.SetCircleDigital(report[5] >> 5);
            InputReport.SetCrossDigital(report[5] >> 6);

            InputReport.SetSelect(report[6] >> 4);
            InputReport.SetStart(report[6] >> 5);

            var dPad = (byte)(report[5] & ~0xF0);

            switch (dPad)
            {
                case 0:
                    InputReport.SetDpadUpDigital(true);
                    break;
                case 1:
                    InputReport.SetDpadUpDigital(true);
                    InputReport.SetDpadRightDigital(true);
                    break;
                case 2:
                    InputReport.SetDpadRightDigital(true);
                    break;
                case 3:
                    InputReport.SetDpadRightDigital(true);
                    InputReport.SetDpadDownDigital(true);
                    break;
                case 4:
                    InputReport.SetDpadDownDigital(true);
                    break;
                case 5:
                    InputReport.SetDpadDownDigital(true);
                    InputReport.SetDpadLeftDigital(true);
                    break;
                case 6:
                    InputReport.SetDpadLeftDigital(true);
                    break;
                case 7:
                    InputReport.SetDpadLeftDigital(true);
                    InputReport.SetDpadUpDigital(true);
                    break;
            }

            InputReport.SetTriangleDigital(report[5] >> 4);
            InputReport.SetSquareDigital(report[5] >> 7);

            InputReport.SetLeftShoulderDigital(report[6] >> 0);
            InputReport.SetRightShoulderDigital(report[6] >> 1);

            InputReport.SetLeftTriggerDigital(report[6] >> 2);
            InputReport.SetRightTriggerDigital(report[6] >> 3);

            InputReport.SetLeftThumb(report[6] >> 6);
            InputReport.SetRightThumb(report[6] >> 7);

            InputReport.SetLeftAxisX(report[3]);
            InputReport.SetLeftAxisY(report[4]);

            InputReport.SetRightAxisX(report[1]);
            InputReport.SetRightAxisY(report[2]);

            #endregion

            OnHidReportReceived();
        }
    }
}
