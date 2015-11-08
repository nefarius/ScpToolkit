using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    public class UsbGameStopDigitalController : UsbGenericGamepad
    {
        public UsbGameStopDigitalController()
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

            var buttons = (byte)(report[6] & ~0xF0);

            #endregion

            OnHidReportReceived();
        }
    }
}
