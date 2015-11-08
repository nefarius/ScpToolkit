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

            // null button states
            m_ReportArgs.ZeroPsButtonsState();
            m_ReportArgs.ZeroSelectStartButtonsState();
            m_ReportArgs.ZeroShoulderButtonsState();

            // control buttons
            m_ReportArgs.SetSelect(report[6] >> 4);
            m_ReportArgs.SetStart(report[6] >> 5);

            // Left shoulder
            m_ReportArgs.SetLeftShoulderDigital(report[6] >> 0);

            // Right shoulder
            m_ReportArgs.SetRightShoulderDigital(report[6] >> 1);

            // Left trigger
            m_ReportArgs.SetLeftTriggerDigital(report[6] >> 2);

            // Right trigger
            m_ReportArgs.SetRightTriggerDigital(report[6] >> 3);

            // Triangle
            m_ReportArgs.SetTriangleDigital(report[5] >> 4);

            // Circle
            m_ReportArgs.SetCircleDigital(report[5] >> 5);

            // Cross
            m_ReportArgs.SetCrossDigital(report[5] >> 6);

            // Square
            m_ReportArgs.SetSquareDigital(report[5] >> 7);

            // Left thumb
            m_ReportArgs.SetLeftThumb(report[6] >> 6);
            // Right thumb
            m_ReportArgs.SetRightThumb(report[6] >> 7);

            var dPad = (byte)(report[5] & ~0xF0);

            // D-Pad
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

            // Left thumb stick
            m_ReportArgs.SetLeftAxisY(report[1]);
            m_ReportArgs.SetLeftAxisX(report[0]);

            // Right thumb stick
            m_ReportArgs.SetRightAxisY(report[4]);
            m_ReportArgs.SetRightAxisX(report[3]);

            #endregion

            OnHidReportReceived();
        }
    }
}
