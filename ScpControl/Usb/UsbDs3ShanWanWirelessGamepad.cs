using System;
using ScpControl.ScpCore;

namespace ScpControl.Usb
{
    public class UsbDs3ShanWanWirelessGamepad : UsbDs3
    {
        protected override void Process(DateTime now)
        {
            // ignore
        }

        public override bool Pair(byte[] master)
        {
            return false; // ignore
        }

        public override bool Rumble(byte large, byte small)
        {
            return false; // ignore
        }

        protected override void Parse(byte[] report)
        {
            if (report[6] != 0x00) return;

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

            m_ReportArgs.SetCircleDigital(report[4] >> 5);
            m_ReportArgs.SetCrossDigital(report[4] >> 6);

            m_ReportArgs.SetSelect(report[5] >> 4);
            m_ReportArgs.SetStart(report[5] >> 5);

            var dPad = (byte)(report[4] & ~0xF0);

            switch (dPad)
            {
                case 0:
                    m_ReportArgs.SetDpadUpDigital(true);
                    break;
                case 1:
                    m_ReportArgs.SetDpadRightDigital(true);
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    m_ReportArgs.SetDpadDownDigital(true);
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    m_ReportArgs.SetDpadLeftDigital(true);
                    break;
            }

            m_ReportArgs.SetTriangleDigital(report[4] >> 4);
            m_ReportArgs.SetSquareDigital(report[4] >> 7);

            m_ReportArgs.SetLeftShoulderDigital(report[5] >> 0);
            m_ReportArgs.SetRightShoulderDigital(report[5] >> 1);

            m_ReportArgs.SetLeftThumb(report[5] >> 6);
            m_ReportArgs.SetRightThumb(report[5] >> 7);

            m_ReportArgs.SetLeftAxisX(report[2]);
            m_ReportArgs.SetLeftAxisY(report[3]);

            m_ReportArgs.SetRightAxisX(report[0]);
            m_ReportArgs.SetRightAxisY(report[1]);

            #endregion

            OnHidReportReceived();
        }
    }
}
