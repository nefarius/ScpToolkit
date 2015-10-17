namespace ScpControl.Shared.Utilities
{
    public static class NativeFeedExtensions
    {
        public static byte GetDpadUpAnalog(this byte[] report)
        {
            return report[22];
        }

        public static byte GetDpadRightAnalog(this byte[] report)
        {
            return report[23];
        }

        public static byte GetDpadDownAnalog(this byte[] report)
        {
            return report[24];
        }

        public static byte GetDpadLeftAnalog(this byte[] report)
        {
            return report[25];
        }

        public static byte GetLeftAxisX(this byte[] report)
        {
            return report[14];
        }

        public static byte GetLeftAxisY(this byte[] report)
        {
            return report[15];
        }

        public static byte GetLeftShoulderAnalog(this byte[] report)
        {
            return report[28];
        }

        public static byte GetLeftTriggerAnalog(this byte[] report)
        {
            return report[26];
        }

        public static bool GetLeftThumb(this byte[] report)
        {
            return (report[10] >> 1 & 1) != 0;
        }

        public static byte GetRightAxisX(this byte[] report)
        {
            return report[16];
        }

        public static byte GetRightAxisY(this byte[] report)
        {
            return report[17];
        }

        public static byte GetRightShoulderAnalog(this byte[] report)
        {
            return report[29];
        }

        public static byte GetRightTriggerAnalog(this byte[] report)
        {
            return report[27];
        }

        public static bool GetRightThumb(this byte[] report)
        {
            return (report[10] >> 2 & 1) != 0;
        }

        public static byte GetTriangleAnalog(this byte[] report)
        {
            return report[30];
        }

        public static byte GetCircleAnalog(this byte[] report)
        {
            return report[31];
        }

        public static byte GetCrossAnalog(this byte[] report)
        {
            return report[32];
        }

        public static byte GetSquareAnalog(this byte[] report)
        {
            return report[33];
        }

        public static bool GetSelect(this byte[] report)
        {
            return (report[10] & 1) != 0;
        }

        public static bool GetStart(this byte[] report)
        {
            return (report[10] >> 3 & 1) != 0;
        }

        public static bool GetPs(this byte[] report)
        {
            return (report[12] & 0x01) != 0;
        }
        
        public static float ToPressure(this byte value)
        {
            return (value & 0xFF)/255.0f;
        }
    }
}
