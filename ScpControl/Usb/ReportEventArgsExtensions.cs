using ScpControl.ScpCore;

namespace ScpControl.Usb
{
    public static class ReportEventArgsExtensions
    {
        public static void SetPacketCounter(this ReportEventArgs args, uint packet)
        {
            args.Report[4] = (byte)(packet >> 0 & 0xFF);
            args.Report[5] = (byte)(packet >> 8 & 0xFF);
            args.Report[6] = (byte)(packet >> 16 & 0xFF);
            args.Report[7] = (byte)(packet >> 24 & 0xFF);
        }

        public static byte SetBatteryStatus(this ReportEventArgs args, byte[] report)
        {
            return args.Report[2] = report[30];
        }

        public static byte SetBatteryStatus(this ReportEventArgs args, DsBattery battery)
        {
            return args.Report[2] = (byte)battery;
        }

        public static void SetTriangleDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 4);
        }

        public static void SetCircleDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 5);
        }

        public static void SetCrossDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 6);
        }

        public static void SetSquareDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 7);
        }

        public static void SetDpadRightDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x20 : 0x00);
        }

        public static void SetDpadLeftDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x80 : 0x00);
        }

        public static void SetDpadUpDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x10 : 0x00);
        }

        public static void SetDpadDownDigital(this ReportEventArgs args, bool input)
        {
            args.Report[10] |= (byte)(input ? 0x40 : 0x00);
        }

        public static void ZeroShoulderButtonsState(this ReportEventArgs args)
        {
            args.Report[11] = 0x00;
        }

        public static void ZeroSelectStartButtonsState(this ReportEventArgs args)
        {
            args.Report[10] = 0x00;
        }

        public static void ZeroPsButtonsState(this ReportEventArgs args)
        {
            args.Report[12] = 0x00;
        }

        public static void SetSelect(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte)(input & 1);
        }

        public static void SetStart(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte)((input & 1) << 3);
        }

        public static void SetPs(this ReportEventArgs args, int input)
        {
            args.Report[12] |= (byte)(input & 1);
        }

        public static void SetLeftShoulderDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 2);
        }

        public static void SetRightShoulderDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 3);
        }

        public static void SetLeftTriggerDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 0);
        }

        public static void SetRightTriggerDigital(this ReportEventArgs args, int input)
        {
            args.Report[11] |= (byte)((input & 1) << 1);
        }

        public static void SetLeftShoulderAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[28] = input;
        }

        public static void SetRightShoulderAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[29] = input;
        }

        public static void SetLeftTriggerAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[26] = input;
        }

        public static void SetRightTriggerAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[27] = input;
        }

        public static void SetTriangleAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[30] = input;
        }

        public static void SetCircleAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[31] = input;
        }

        public static void SetCrossAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[32] = input;
        }

        public static void SetSquareAnalog(this ReportEventArgs args, byte input)
        {
            args.Report[33] = input;
        }

        public static void SetLeftThumb(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte)((input & 1) << 1);
        }

        public static void SetRightThumb(this ReportEventArgs args, int input)
        {
            args.Report[10] |= (byte)((input & 1) << 2);
        }

        public static void SetLeftAxisY(this ReportEventArgs args, byte input)
        {
            args.Report[15] = input;
        }

        public static void SetLeftAxisX(this ReportEventArgs args, byte input)
        {
            args.Report[14] = input;
        }

        public static void SetRightAxisY(this ReportEventArgs args, byte input)
        {
            args.Report[17] = input;
        }

        public static void SetRightAxisX(this ReportEventArgs args, byte input)
        {
            args.Report[16] = input;
        }
    }
}
