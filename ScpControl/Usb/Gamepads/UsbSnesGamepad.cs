using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    /// <summary>
    ///     DragonRise Inc. USB Gamepad SNES
    /// </summary>
    public class UsbSnesGamepad : UsbGenericGamepad
    {
        public UsbSnesGamepad()
        {
            VendorId = 0x0079;
            ProductId = 0x0011;
        }

        protected override void ParseHidReport(byte[] report)
        {
            if (report[1] != 0x01) return;

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

            m_ReportArgs.SetSelect(report[7] >> 4); // Select
            m_ReportArgs.SetStart(report[7] >> 5); // Start
            
            m_ReportArgs.SetLeftShoulderDigital(report[7] >> 0); // L1 (button)
            m_ReportArgs.SetRightShoulderDigital(report[7] >> 2); // R1 (button)

            m_ReportArgs.SetTriangleDigital(report[6] >> 4); // Triangle (button)
            m_ReportArgs.SetCircleDigital(report[6] >> 5); // Circle (button)
            m_ReportArgs.SetCrossDigital(report[6] >> 6); // Cross (button)
            m_ReportArgs.SetSquareDigital(report[6] >> 7); // Square (button)

            m_ReportArgs.SetDpadRightDigital(report[4] == 0xFF); // D-Pad right
            m_ReportArgs.SetDpadLeftDigital(report[4] == 0x00); // D-Pad left
            m_ReportArgs.SetDpadUpDigital(report[5] == 0x00); // D-Pad up
            m_ReportArgs.SetDpadDownDigital(report[5] == 0xFF); // D-Pad down

            // This device has no thumb sticks, center axes
            m_ReportArgs.SetLeftAxisY(0x80);
            m_ReportArgs.SetLeftAxisX(0x80);
            m_ReportArgs.SetRightAxisY(0x80);
            m_ReportArgs.SetRightAxisX(0x80);

            #endregion

            OnHidReportReceived();
        }
    }
}
