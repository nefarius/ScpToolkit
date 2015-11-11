namespace ScpControl.Usb.Ds3.Replica
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[26] != 0x02) return;

            if (PacketCounter++ + 1 < PacketCounter)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", PacketCounter);
                PacketCounter = 0;
            }

            #region HID Report translation

            // battery
            m_BatteryStatus = InputReport.BatteryStatus = report[30];

            // packet counter
            InputReport.SetPacketCounter(PacketCounter);

            // null button states
            InputReport.ZeroPsButtonState();
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            // control buttons
            InputReport.SetPs(report[1] >> 4);
            InputReport.SetSelect(report[1] >> 0);
            InputReport.SetStart(report[1] >> 1);

            // Left shoulder
            InputReport.SetLeftShoulderDigital(report[0] >> 4);
            InputReport.SetLeftShoulderAnalog(report[15]);

            // Right shoulder
            InputReport.SetRightShoulderDigital(report[0] >> 5);
            InputReport.SetRightShoulderAnalog(report[16]);

            // Left trigger
            InputReport.SetLeftTriggerDigital(report[0] >> 6);
            InputReport.SetLeftTriggerAnalog(report[17]);

            // Right trigger
            InputReport.SetRightTriggerDigital(report[0] >> 7);
            InputReport.SetRightTriggerAnalog(report[18]);

            // Triangle
            InputReport.SetTriangleDigital(report[0] >> 3);
            InputReport.SetTriangleAnalog(report[11]);

            // Circle
            InputReport.SetCircleDigital(report[0] >> 2);
            InputReport.SetCircleAnalog(report[12]);

            // Cross
            InputReport.SetCrossDigital(report[0] >> 1);
            InputReport.SetCrossAnalog(report[13]);

            // Square
            InputReport.SetSquareDigital(report[0] >> 0);
            InputReport.SetSquareAnalog(report[14]);

            // Left thumb
            InputReport.SetLeftThumb(report[1] >> 2);
            // Right thumb
            InputReport.SetRightThumb(report[1] >> 3);

            // D-Pad
            if (report[2] != 0x0F)
            {
                switch (report[2])
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
            }

            // Left thumb stick
            InputReport.SetLeftAxisY(report[4]);
            InputReport.SetLeftAxisX(report[3]);

            // Right thumb stick
            InputReport.SetRightAxisY(report[6]);
            InputReport.SetRightAxisX(report[5]);

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
