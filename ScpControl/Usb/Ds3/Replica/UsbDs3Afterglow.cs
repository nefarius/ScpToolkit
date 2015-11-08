namespace ScpControl.Usb.Ds3.Replica
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[26] != 0x02) return;

            if (m_Packet++ + 1 < m_Packet)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", m_Packet);
                m_Packet = 0;
            }

            #region HID Report translation

            // battery
            m_BatteryStatus = m_ReportArgs.BatteryStatus = report[30];

            // packet counter
            m_ReportArgs.SetPacketCounter(m_Packet);

            // null button states
            m_ReportArgs.ZeroPsButtonsState();
            m_ReportArgs.ZeroSelectStartButtonsState();
            m_ReportArgs.ZeroShoulderButtonsState();

            // control buttons
            m_ReportArgs.SetPs(report[1] >> 4);
            m_ReportArgs.SetSelect(report[1] >> 0);
            m_ReportArgs.SetStart(report[1] >> 1);

            // Left shoulder
            m_ReportArgs.SetLeftShoulderDigital(report[0] >> 4);
            m_ReportArgs.SetLeftShoulderAnalog(report[15]);

            // Right shoulder
            m_ReportArgs.SetRightShoulderDigital(report[0] >> 5);
            m_ReportArgs.SetRightShoulderAnalog(report[16]);

            // Left trigger
            m_ReportArgs.SetLeftTriggerDigital(report[0] >> 6);
            m_ReportArgs.SetLeftTriggerAnalog(report[17]);

            // Right trigger
            m_ReportArgs.SetRightTriggerDigital(report[0] >> 7);
            m_ReportArgs.SetRightTriggerAnalog(report[18]);

            // Triangle
            m_ReportArgs.SetTriangleDigital(report[0] >> 3);
            m_ReportArgs.SetTriangleAnalog(report[11]);

            // Circle
            m_ReportArgs.SetCircleDigital(report[0] >> 2);
            m_ReportArgs.SetCircleAnalog(report[12]);

            // Cross
            m_ReportArgs.SetCrossDigital(report[0] >> 1);
            m_ReportArgs.SetCrossAnalog(report[13]);

            // Square
            m_ReportArgs.SetSquareDigital(report[0] >> 0);
            m_ReportArgs.SetSquareAnalog(report[14]);

            // Left thumb
            m_ReportArgs.SetLeftThumb(report[1] >> 2);
            // Right thumb
            m_ReportArgs.SetRightThumb(report[1] >> 3);

            // D-Pad
            if (report[2] != 0x0F)
            {
                switch (report[2])
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
            }

            // Left thumb stick
            m_ReportArgs.SetLeftAxisY(report[4]);
            m_ReportArgs.SetLeftAxisX(report[3]);

            // Right thumb stick
            m_ReportArgs.SetRightAxisY(report[6]);
            m_ReportArgs.SetRightAxisX(report[5]);

            #endregion

            OnHidReportReceived();
        }

        public override bool Pair(byte[] master)
        {
            // controller uses it's own wireless protocol, pairing is not needed
            return true;
        }
    }
}
