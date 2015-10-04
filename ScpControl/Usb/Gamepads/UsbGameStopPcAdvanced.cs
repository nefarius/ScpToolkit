using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     GameStop PC Advanced Controller
    /// </summary>
    public class UsbGameStopPcAdvanced : UsbGenericGamepad
    {
        protected override void Parse(byte[] report)
        {
            if (report[7] != 0xC0 && report[7] != 0x40) return;

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
            m_ReportArgs.SetCircleDigital(report[5] >> 5);
            m_ReportArgs.SetCrossDigital(report[5] >> 6);
            m_ReportArgs.SetTriangleDigital(report[5] >> 4);
            m_ReportArgs.SetSquareDigital(report[5] >> 7);

            m_ReportArgs.SetSelect(report[6] >> 4);
            m_ReportArgs.SetStart(report[6] >> 5);

            m_ReportArgs.SetLeftShoulderDigital(report[6] >> 0);
            m_ReportArgs.SetRightShoulderDigital(report[6] >> 1);
            m_ReportArgs.SetLeftTriggerDigital(report[6] >> 2);
            m_ReportArgs.SetRightTriggerDigital(report[6] >> 3);

            m_ReportArgs.SetLeftThumb(report[6] >> 6);
            m_ReportArgs.SetRightThumb(report[6] >> 7);

            // detect mode it's running in
            switch (report[7])
            {
                case 0xC0: // mode 1
                {
                    m_ReportArgs.SetDpadUpDigital(report[1] == 0x00);
                    m_ReportArgs.SetDpadRightDigital(report[0] == 0xFF);
                    m_ReportArgs.SetDpadDownDigital(report[1] == 0xFF);
                    m_ReportArgs.SetDpadLeftDigital(report[0] == 0x00);

                    // mode 1 doesn't report the thumb sticks
                    m_ReportArgs.SetLeftAxisX(0x80);
                    m_ReportArgs.SetLeftAxisY(0x80);
                    m_ReportArgs.SetRightAxisX(0x80);
                    m_ReportArgs.SetRightAxisY(0x80);
                }
                    break;
                case 0x40: // mode 2
                {
                    var dPad = (byte) (report[5] & ~0xF0);

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

                    m_ReportArgs.SetLeftAxisX(report[0]);
                    m_ReportArgs.SetLeftAxisY(report[1]);

                    m_ReportArgs.SetRightAxisX(report[3]);
                    m_ReportArgs.SetRightAxisY(report[4]);
                }
                    break;
            }

            #endregion

            OnHidReportReceived();
        }
    }
}