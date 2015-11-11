using ScpControl.ScpCore;

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
            InputReport.SetPacketCounter(PacketCounter);

            // null button states
            InputReport.ZeroPsButtonState();
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            // control buttons
            InputReport.SetSelect(report[6] >> 4);
            InputReport.SetStart(report[6] >> 5);

            // Left shoulder
            InputReport.SetLeftShoulderDigital(report[6] >> 0);

            // Right shoulder
            InputReport.SetRightShoulderDigital(report[6] >> 1);

            // Left trigger
            InputReport.SetLeftTriggerDigital(report[6] >> 2);

            // Right trigger
            InputReport.SetRightTriggerDigital(report[6] >> 3);

            // Triangle
            InputReport.SetTriangleDigital(report[5] >> 4);

            // Circle
            InputReport.SetCircleDigital(report[5] >> 5);

            // Cross
            InputReport.SetCrossDigital(report[5] >> 6);

            // Square
            InputReport.SetSquareDigital(report[5] >> 7);

            // Left thumb
            InputReport.SetLeftThumb(report[6] >> 6);
            // Right thumb
            InputReport.SetRightThumb(report[6] >> 7);

            var dPad = (byte)(report[5] & ~0xF0);

            // D-Pad
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

            // Left thumb stick
            InputReport.SetLeftAxisY(report[1]);
            InputReport.SetLeftAxisX(report[0]);

            // Right thumb stick
            InputReport.SetRightAxisY(report[4]);
            InputReport.SetRightAxisX(report[3]);

            #endregion

            OnHidReportReceived();
        }
    }
}
