using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    public class UsbBadBigBenGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[4] != 0x00) return;

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            #region HID Report translation

            // overwrite Report ID
            m_ReportArgs.Report[0] = 0x01;

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = m_ReportArgs.SetBatteryStatus(DsBattery.None);

            // packet counter
            m_ReportArgs.SetPacketCounter(m_Packet);

            m_ReportArgs.SetCircleDigital(report[5] >> 5);
            m_ReportArgs.SetCrossDigital(report[5] >> 6);
            m_ReportArgs.SetTriangleDigital(report[5] >> 4);
            m_ReportArgs.SetSquareDigital(report[5] >> 7);

            m_ReportArgs.SetSelect(report[6] >> 4);
            m_ReportArgs.SetStart(report[6] >> 5);

            m_ReportArgs.SetDpadUpDigital(report[3] == 0x00);
            m_ReportArgs.SetDpadRightDigital(report[2] == 0xFF);
            m_ReportArgs.SetDpadDownDigital(report[3] == 0xFF);
            m_ReportArgs.SetDpadLeftDigital(report[2] == 0x00);

            m_ReportArgs.SetLeftShoulderDigital(report[6] >> 0);
            m_ReportArgs.SetRightShoulderDigital(report[6] >> 1);
            m_ReportArgs.SetLeftTriggerDigital(report[6] >> 2);
            m_ReportArgs.SetRightTriggerDigital(report[6] >> 3);

            m_ReportArgs.SetLeftThumb(report[6] >> 6);
            m_ReportArgs.SetRightThumb(report[6] >> 7);
            
            // TODO: dafuq?!
            // http://forums.pcsx2.net/attachment.php?aid=57420

            #endregion

            OnHidReportReceived();
        }
    }
}
