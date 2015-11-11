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

            if (PacketCounter++ + 1 < PacketCounter)
            {
                Log.WarnFormat("Packet counter rolled over ({0}), resetting to 0", PacketCounter);
                PacketCounter = 0;
            }

            #region HID Report translation

            // no battery state since the Gamepad is USB-powered
            m_BatteryStatus = InputReport.SetBatteryStatus(DsBattery.None);

            // packet counter
            InputReport.SetPacketCounter(PacketCounter);

            // reset buttons
            InputReport.ZeroSelectStartButtonsState();
            InputReport.ZeroShoulderButtonsState();

            InputReport.SetSelect(report[7] >> 4); // Select
            InputReport.SetStart(report[7] >> 5); // Start
            
            InputReport.SetLeftShoulderDigital(report[7] >> 0); // L1 (button)
            InputReport.SetRightShoulderDigital(report[7] >> 2); // R1 (button)

            InputReport.SetTriangleDigital(report[6] >> 4); // Triangle (button)
            InputReport.SetCircleDigital(report[6] >> 5); // Circle (button)
            InputReport.SetCrossDigital(report[6] >> 6); // Cross (button)
            InputReport.SetSquareDigital(report[6] >> 7); // Square (button)

            InputReport.SetDpadRightDigital(report[4] == 0xFF); // D-Pad right
            InputReport.SetDpadLeftDigital(report[4] == 0x00); // D-Pad left
            InputReport.SetDpadUpDigital(report[5] == 0x00); // D-Pad up
            InputReport.SetDpadDownDigital(report[5] == 0xFF); // D-Pad down

            // This device has no thumb sticks, center axes
            InputReport.SetLeftAxisY(0x80);
            InputReport.SetLeftAxisX(0x80);
            InputReport.SetRightAxisY(0x80);
            InputReport.SetRightAxisX(0x80);

            #endregion

            OnHidReportReceived();
        }
    }
}
