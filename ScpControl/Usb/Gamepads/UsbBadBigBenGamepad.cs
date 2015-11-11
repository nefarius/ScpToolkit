using ScpControl.ScpCore;

namespace ScpControl.Usb.Gamepads
{
    public class UsbBadBigBenGamepad : UsbGenericGamepad
    {
        protected override void ParseHidReport(byte[] report)
        {
            if (report[5] != 0x00) return;

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

            InputReport.SetCircleDigital(report[6] >> 5);
            InputReport.SetCrossDigital(report[6] >> 6);
            InputReport.SetTriangleDigital(report[6] >> 4);
            InputReport.SetSquareDigital(report[6] >> 7);

            InputReport.SetSelect(report[7] >> 4);
            InputReport.SetStart(report[7] >> 5);

            InputReport.SetDpadUpDigital(report[4] == 0x00);
            InputReport.SetDpadRightDigital(report[3] == 0xFF);
            InputReport.SetDpadDownDigital(report[4] == 0xFF);
            InputReport.SetDpadLeftDigital(report[3] == 0x00);

            InputReport.SetLeftShoulderDigital(report[7] >> 0);
            InputReport.SetRightShoulderDigital(report[7] >> 1);
            InputReport.SetLeftTriggerDigital(report[7] >> 2);
            InputReport.SetRightTriggerDigital(report[7] >> 3);

            InputReport.SetLeftThumb(report[7] >> 6);
            InputReport.SetRightThumb(report[7] >> 7);
            
            // TODO: dafuq?!
            // http://forums.pcsx2.net/attachment.php?aid=57420

            #endregion

            OnHidReportReceived();
        }
    }
}
