namespace ScpControl.Usb
{
    public class UsbDs3Afterglow : UsbDs3
    {
        protected override void Parse(byte[] report)
        {
            ConvertAfterglowToValidBytes(ref report);

            base.Parse(report);
        }

        /// <summary>
        ///     Prototype helper method to convert input byte stream of Afterglow Wireless Controllers to valid PS3 packet.
        /// </summary>
        /// <param name="b">The input byte stream captured from the controller.</param>
        private static void ConvertAfterglowToValidBytes(ref byte[] b)
        {
            // identify the fake controller
            // TODO: add more checks
            if (b[26] != 0x02) return;

            // prepare temporary array for input values
            var input = new byte[28];

            // copy source array and zero out bytes
            for (int i = 0; i < 27; i++)
            {
                input[i] = b[i];
                b[i] = 0x00;
            }

            b[4] |= (byte)((input[1] >> 4) & 1); // PS
            b[2] |= (byte)((input[1] >> 0) & 1); // Select
            b[2] |= (byte)(((input[1] >> 1) & 1) << 3); // Start

            b[3] |= (byte)(((input[0] >> 4) & 1) << 2); // L1 (button)
            b[20] = input[15]; // L1 (analog)

            b[3] |= (byte)(((input[0] >> 5) & 1) << 3); // R1 (button)
            b[21] = input[16]; // R1 (analog)

            b[3] |= (byte)(((input[0] >> 6) & 1) << 0); // L2 (button)
            b[18] = input[17]; // L2 (analog)

            b[3] |= (byte)(((input[0] >> 7) & 1) << 1); // R2 (button)
            b[19] = input[18]; // R2 (analog)

            b[3] |= (byte)(((input[0] >> 3) & 1) << 4); // Triangle (button)
            b[22] = input[11]; // Triangle (analog)

            b[3] |= (byte)(((input[0] >> 2) & 1) << 5); // Circle (button)
            b[23] = input[12]; // Circle (analog)

            b[3] |= (byte)(((input[0] >> 1) & 1) << 6); // Cross (button)
            b[24] = input[13]; // Cross (analog)

            b[3] |= (byte)(((input[0] >> 0) & 1) << 7); // Square (button)
            b[25] = input[14]; // Square (analog)

            if (input[2] != 0x0F)
            {
                b[2] |= (byte)((input[2] == 0x02) ? 0x20 : 0x00); // D-Pad right
                b[15] = input[7]; // D-Pad right

                b[2] |= (byte)((input[2] == 0x06) ? 0x80 : 0x00); // D-Pad left
                b[17] = input[8]; // D-Pad left

                b[2] |= (byte)((input[2] == 0x00) ? 0x10 : 0x00); // D-Pad up
                b[14] = input[9]; // D-Pad up

                b[2] |= (byte)((input[2] == 0x04) ? 0x40 : 0x00); // D-Pad down
                b[16] = input[10]; // D-Pad down
            }

            b[7] = input[4]; // Left Axis Y+
            b[7] = input[4]; // Left Axis Y-
            b[6] = input[3]; // Left Axis X-
            b[6] = input[3]; // Left Axis X+

            b[9] = input[6]; // Right Axis Y+
            b[9] = input[6]; // Right Axis Y-
            b[8] = input[5]; // Right Axis X-
            b[8] = input[5]; // Right Axis X+

            b[2] |= (byte)(((input[1] >> 2) & 1) << 1); // Left Thumb
            b[2] |= (byte)(((input[1] >> 3) & 1) << 2); // Right Thumb

            b[0] = 0x01;
        }
    }
}
