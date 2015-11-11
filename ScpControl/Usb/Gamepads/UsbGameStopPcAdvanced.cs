using ScpControl.ScpCore;

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

            // buttons equaly reported in both modes
            InputReport.SetCircleDigital(report[6] >> 5);
            InputReport.SetCrossDigital(report[6] >> 6);
            InputReport.SetTriangleDigital(report[6] >> 4);
            InputReport.SetSquareDigital(report[6] >> 7);

            InputReport.SetSelect(report[7] >> 4);
            InputReport.SetStart(report[7] >> 5);

            InputReport.SetLeftShoulderDigital(report[7] >> 0);
            InputReport.SetRightShoulderDigital(report[7] >> 1);
            InputReport.SetLeftTriggerDigital(report[7] >> 2);
            InputReport.SetRightTriggerDigital(report[7] >> 3);

            InputReport.SetLeftThumb(report[7] >> 6);
            InputReport.SetRightThumb(report[7] >> 7);

            // detect mode it's running in
            switch (report[8])
            {
                case 0xC0: // mode 1
                {
                    InputReport.SetDpadUpDigital(report[2] == 0x00);
                    InputReport.SetDpadRightDigital(report[1] == 0xFF);
                    InputReport.SetDpadDownDigital(report[2] == 0xFF);
                    InputReport.SetDpadLeftDigital(report[1] == 0x00);

                    // mode 1 doesn't report the thumb sticks
                    InputReport.SetLeftAxisX(0x80);
                    InputReport.SetLeftAxisY(0x80);
                    InputReport.SetRightAxisX(0x80);
                    InputReport.SetRightAxisY(0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[6] & ~0xF0);

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

                    InputReport.SetLeftAxisX(report[1]);
                    InputReport.SetLeftAxisY(report[2]);

                    InputReport.SetRightAxisX(report[4]);
                    InputReport.SetRightAxisY(report[5]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived();
        }
    }
}