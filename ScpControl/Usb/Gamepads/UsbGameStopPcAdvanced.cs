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

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = m_ReportArgs.SetBatteryStatus(DsBattery.None);

            // packet counter
            m_ReportArgs.SetPacketCounter(m_Packet);

            // reset buttons
            m_ReportArgs.ZeroSelectStartButtonsState();
            m_ReportArgs.ZeroShoulderButtonsState();

            // buttons equaly reported in both modes
            m_ReportArgs.SetCircleDigital(report[6] >> 5);
            m_ReportArgs.SetCrossDigital(report[6] >> 6);
            m_ReportArgs.SetTriangleDigital(report[6] >> 4);
            m_ReportArgs.SetSquareDigital(report[6] >> 7);

            m_ReportArgs.SetSelect(report[7] >> 4);
            m_ReportArgs.SetStart(report[7] >> 5);

            m_ReportArgs.SetLeftShoulderDigital(report[7] >> 0);
            m_ReportArgs.SetRightShoulderDigital(report[7] >> 1);
            m_ReportArgs.SetLeftTriggerDigital(report[7] >> 2);
            m_ReportArgs.SetRightTriggerDigital(report[7] >> 3);

            m_ReportArgs.SetLeftThumb(report[7] >> 6);
            m_ReportArgs.SetRightThumb(report[7] >> 7);

            // detect mode it's running in
            switch (report[8])
            {
                case 0xC0: // mode 1
                {
                    m_ReportArgs.SetDpadUpDigital(report[2] == 0x00);
                    m_ReportArgs.SetDpadRightDigital(report[1] == 0xFF);
                    m_ReportArgs.SetDpadDownDigital(report[2] == 0xFF);
                    m_ReportArgs.SetDpadLeftDigital(report[1] == 0x00);

                    // mode 1 doesn't report the thumb sticks
                    m_ReportArgs.SetLeftAxisX(0x80);
                    m_ReportArgs.SetLeftAxisY(0x80);
                    m_ReportArgs.SetRightAxisX(0x80);
                    m_ReportArgs.SetRightAxisY(0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[6] & ~0xF0);

                    switch (dPad)
                    {
                        case 0:
                            m_ReportArgs.SetDpadUpDigital(true);
                            break;
                        case 1:
                            m_ReportArgs.SetDpadUpDigital(true);
                            m_ReportArgs.SetDpadRightDigital(true);
                            break;
                        case 2:
                            m_ReportArgs.SetDpadRightDigital(true);
                            break;
                        case 3:
                            m_ReportArgs.SetDpadRightDigital(true);
                            m_ReportArgs.SetDpadDownDigital(true);
                            break;
                        case 4:
                            m_ReportArgs.SetDpadDownDigital(true);
                            break;
                        case 5:
                            m_ReportArgs.SetDpadDownDigital(true);
                            m_ReportArgs.SetDpadLeftDigital(true);
                            break;
                        case 6:
                            m_ReportArgs.SetDpadLeftDigital(true);
                            break;
                        case 7:
                            m_ReportArgs.SetDpadLeftDigital(true);
                            m_ReportArgs.SetDpadUpDigital(true);
                            break;
                    }

                    m_ReportArgs.SetLeftAxisX(report[1]);
                    m_ReportArgs.SetLeftAxisY(report[2]);

                    m_ReportArgs.SetRightAxisX(report[4]);
                    m_ReportArgs.SetRightAxisY(report[5]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived();
        }
    }
}