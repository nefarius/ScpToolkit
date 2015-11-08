using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    public class UsbBadBigBenGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[5] != 0x00) return;

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

            m_ReportArgs.SetCircleDigital(report[6] >> 5);
            m_ReportArgs.SetCrossDigital(report[6] >> 6);
            m_ReportArgs.SetTriangleDigital(report[6] >> 4);
            m_ReportArgs.SetSquareDigital(report[6] >> 7);

            m_ReportArgs.SetSelect(report[7] >> 4);
            m_ReportArgs.SetStart(report[7] >> 5);

            m_ReportArgs.SetDpadUpDigital(report[4] == 0x00);
            m_ReportArgs.SetDpadRightDigital(report[3] == 0xFF);
            m_ReportArgs.SetDpadDownDigital(report[4] == 0xFF);
            m_ReportArgs.SetDpadLeftDigital(report[3] == 0x00);

            m_ReportArgs.SetLeftShoulderDigital(report[7] >> 0);
            m_ReportArgs.SetRightShoulderDigital(report[7] >> 1);
            m_ReportArgs.SetLeftTriggerDigital(report[7] >> 2);
            m_ReportArgs.SetRightTriggerDigital(report[7] >> 3);

            m_ReportArgs.SetLeftThumb(report[7] >> 6);
            m_ReportArgs.SetRightThumb(report[7] >> 7);
            
            // TODO: dafuq?!
            // http://forums.pcsx2.net/attachment.php?aid=57420

            #endregion

            OnHidReportReceived();
        }
    }
}
