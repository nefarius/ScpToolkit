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
            m_ReportArgs.Report[2] = m_BatteryStatus = (byte) DsBattery.None;

            // packet counter
            m_ReportArgs.Report[4] = (byte) (m_Packet >> 0 & 0xFF);
            m_ReportArgs.Report[5] = (byte) (m_Packet >> 8 & 0xFF);
            m_ReportArgs.Report[6] = (byte) (m_Packet >> 16 & 0xFF);
            m_ReportArgs.Report[7] = (byte) (m_Packet >> 24 & 0xFF);

            m_ReportArgs.Report[10] = 0x00;
            m_ReportArgs.Report[10] |= (byte) ((report[6] >> 4) & 1); // Select
            m_ReportArgs.Report[10] |= (byte) (((report[6] >> 5) & 1) << 3); // Start

            m_ReportArgs.Report[11] = 0x00;
            m_ReportArgs.Report[11] |= (byte) (((report[6] >> 0) & 1) << 2); // L1 (button)
            //m_ReportArgs.Report[20] = input[15]; // L1 (analog)

            m_ReportArgs.Report[11] |= (byte) (((report[6] >> 2) & 1) << 3); // R1 (button)
            //m_ReportArgs.Report[21] = input[16]; // R1 (analog)

            m_ReportArgs.Report[11] |= (byte) (((report[5] >> 4) & 1) << 4); // Triangle (button)
            //m_ReportArgs.Report[22] = report[11]; // Triangle (analog)

            m_ReportArgs.Report[11] |= (byte) (((report[5] >> 5) & 1) << 5); // Circle (button)
            //m_ReportArgs.Report[23] = report[12]; // Circle (analog)

            m_ReportArgs.Report[11] |= (byte) (((report[5] >> 6) & 1) << 6); // Cross (button)
            //m_ReportArgs.Report[24] = report[13]; // Cross (analog)

            m_ReportArgs.Report[11] |= (byte) (((report[5] >> 7) & 1) << 7); // Square (button)
            //m_ReportArgs.Report[25] = report[14]; // Square (analog)

            m_ReportArgs.Report[10] |= (byte) ((report[3] == 0xFF) ? 0x20 : 0x00); // D-Pad right
            m_ReportArgs.Report[23] = (byte)((report[3] == 0xFF) ? 0xFF : 0x00); // D-Pad right

            m_ReportArgs.Report[10] |= (byte) ((report[3] == 0x00) ? 0x80 : 0x00); // D-Pad left
            m_ReportArgs.Report[25] = (byte)((report[3] == 0x00) ? 0xFF : 0x00); // D-Pad left

            m_ReportArgs.Report[10] |= (byte) ((report[4] == 0x00) ? 0x10 : 0x00); // D-Pad up
            m_ReportArgs.Report[22] = (byte)((report[4] == 0x00) ? 0xFF : 0x00); // D-Pad up

            m_ReportArgs.Report[10] |= (byte) ((report[4] == 0xFF) ? 0x40 : 0x00); // D-Pad down
            m_ReportArgs.Report[24] = (byte)((report[4] == 0xFF) ? 0xFF : 0x00); // D-Pad down

            //m_ReportArgs.Report[12] ^= 0x1;

            OnHidReportReceived();
        }
    }
}