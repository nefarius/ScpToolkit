namespace ScpControl.Shared.Utilities
{
    public static class DsMath
    {
        /// <summary>
        ///     Translates DualShock axis value to Xbox 360 compatible value.
        /// </summary>
        /// <param name="value">The DualShock value.</param>
        /// <param name="flip">True to invert the axis, false for 1:1 scaling.</param>
        /// <returns>The Xbox 360 value.</returns>
        public static int Scale(int value, bool flip)
        {
            value -= 0x80;
            if (value == -128) value = -127;

            if (flip) value *= -1;

            return (int)(value * 258.00787401574803149606299212599f);
        }

        /// <summary>
        ///     Checks if X and Y positions are within the provided dead zone.
        /// </summary>
        /// <param name="r">The threshold value.</param>
        /// <param name="x">The value for the X-axis.</param>
        /// <param name="y">The value for the Y-axis.</param>
        /// <returns>True if positions are within the dead zone, false otherwise.</returns>
        public static bool DeadZone(int r, int x, int y)
        {
            x -= 0x80;
            if (x == -128) x = -127;
            y -= 0x80;
            if (y == -128) y = -127;

            return r * r >= x * x + y * y;
        }

        private static float ClampAxis(float value) { if (value > 1.0f) return 1.0f; else if (value < -1.0f) return -1.0f; else return value; }

        public static float ToAxis(byte value) { return ClampAxis(value / 32767.0f); }
    }
}
