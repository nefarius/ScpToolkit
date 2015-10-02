using System;
using ScpControl.ScpCore;

namespace ScpControl.Usb
{
    /// <summary>
    ///     DragonRise Inc. USB Gamepad SNES
    /// </summary>
    public class UsbDs3SnesGamepad : UsbDs3
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
            if (report[0] != 0x01) return;

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = m_ReportArgs.SetBatteryStatus(DsBattery.None);

            // packet counter
            m_ReportArgs.SetPacketCounter(m_Packet);

            m_ReportArgs.ZeroSelectStartButtonsState();
            m_ReportArgs.SetSelect(report[6] >> 4); // Select
            m_ReportArgs.SetStart(report[6] >> 5); // Start

            m_ReportArgs.ZeroShoulderButtonsState();
            m_ReportArgs.SetLeftShoulderDigital(report[6] >> 0); // L1 (button)
            //m_ReportArgs.Report[20] = input[15]; // L1 (analog)

            m_ReportArgs.SetRightShoulderDigital(report[6] >> 2); // R1 (button)
            //m_ReportArgs.Report[21] = input[16]; // R1 (analog)

            m_ReportArgs.SetTriangleDigital(report[5] >> 4); // Triangle (button)
            //m_ReportArgs.Report[22] = report[11]; // Triangle (analog)

            m_ReportArgs.SetCircleDigital(report[5] >> 5); // Circle (button)
            //m_ReportArgs.Report[23] = report[12]; // Circle (analog)

            m_ReportArgs.SetCrossDigital(report[5] >> 6); // Cross (button)
            //m_ReportArgs.Report[24] = report[13]; // Cross (analog)

            m_ReportArgs.SetSquareDigital(report[5] >> 7); // Square (button)
            //m_ReportArgs.Report[25] = report[14]; // Square (analog)

            m_ReportArgs.SetDpadRightDigital(report[3] == 0xFF); // D-Pad right
            m_ReportArgs.Report[23] = (byte)((report[3] == 0xFF) ? 0xFF : 0x00); // D-Pad right

            m_ReportArgs.SetDpadLeftDigital(report[3] == 0x00); // D-Pad left
            m_ReportArgs.Report[25] = (byte)((report[3] == 0x00) ? 0xFF : 0x00); // D-Pad left

            m_ReportArgs.SetDpadUpDgital(report[4] == 0x00); // D-Pad up
            m_ReportArgs.Report[22] = (byte)((report[4] == 0x00) ? 0xFF : 0x00); // D-Pad up

            m_ReportArgs.SetDpadDownDigital(report[4] == 0xFF); // D-Pad down
            m_ReportArgs.Report[24] = (byte)((report[4] == 0xFF) ? 0xFF : 0x00); // D-Pad down

            //m_ReportArgs.Report[12] ^= 0x1;

            OnHidReportReceived();
        }
    }
}