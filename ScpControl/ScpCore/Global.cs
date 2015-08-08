using ScpControl.Properties;

namespace ScpControl.ScpCore
{
    public static class Global
    {
        private static readonly byte[] m_BD_Link =
        {
            0x56, 0xE8, 0x81, 0x38, 0x08, 0x06, 0x51, 0x41, 0xC0, 0x7F, 0x12, 0xAA,
            0xD9, 0x66, 0x3C, 0xCE
        };

        private const int IdleTimeoutMultiplier = 60000;
        private const int LatencyMultiplier = 16;

        public static bool FlipLX
        {
            get { return Settings.Default.FlipAxisLx; }
            set { Settings.Default.FlipAxisLx = value; }
        }

        public static bool FlipLY
        {
            get { return Settings.Default.FlipAxisLy; }
            set { Settings.Default.FlipAxisLy = value; }
        }

        public static bool FlipRX
        {
            get { return Settings.Default.FlipAxisRx; }
            set { Settings.Default.FlipAxisRx = value; }
        }

        public static bool FlipRY
        {
            get { return Settings.Default.FlipAxisRy; }
            set { Settings.Default.FlipAxisRy = value; }
        }

        public static bool DisableLED
        {
            get { return Settings.Default.DisableLed; }
            set { Settings.Default.DisableLed = value; }
        }

        public static bool DisableRumble
        {
            get { return Settings.Default.DisableRumble; }
            set { Settings.Default.DisableRumble = value; }
        }

        public static bool SwapTriggers
        {
            get { return Settings.Default.SwapTriggers; }
            set { Settings.Default.SwapTriggers = value; }
        }

        public static bool DisableLightBar
        {
            get { return Settings.Default.Ds4LightBarBrightness == 0; }
        }

        public static bool IdleDisconnect
        {
            get { return Settings.Default.IdleTimout != 0; }
        }

        public static int IdleTimeout
        {
            get { return Settings.Default.IdleTimout; }
            set { Settings.Default.IdleTimout = value*IdleTimeoutMultiplier; }
        }

        public static int Latency
        {
            get { return Settings.Default.Ds3RumbleLatency; }
            set { Settings.Default.Ds3RumbleLatency = value*LatencyMultiplier; }
        }

        public static byte DeadZoneL
        {
            get { return Settings.Default.DeadZoneL; }
            set { Settings.Default.DeadZoneL = value; }
        }

        public static byte DeadZoneR
        {
            get { return Settings.Default.DeadZoneR; }
            set { Settings.Default.DeadZoneR = value; }
        }

        public static bool DisableNative
        {
            get { return Settings.Default.DisableNativeFeed; }
            set { Settings.Default.DisableNativeFeed = value; }
        }

        public static bool DisableSSP
        {
            get { return Settings.Default.DisableSecureSimplePairing; }
            set { Settings.Default.DisableSecureSimplePairing = value; }
        }

        public static byte Brightness
        {
            get { return Settings.Default.Ds4LightBarBrightness; }
            set { Settings.Default.Ds4LightBarBrightness = value; }
        }

        public static int Bus
        {
            get { return Settings.Default.BusId; }
            set { Settings.Default.BusId = value; }
        }

        public static bool Repair
        {
            get { return Settings.Default.Ds4Repair; }
            set { Settings.Default.Ds4Repair = value; }
        }

        /// <summary>
        ///     Represents the currently active global configurations as byte array.
        /// </summary>
        public static byte[] Packed
        {
            get
            {
                var buffer = new byte[17];

                buffer[1] = 0x03;
                buffer[2] = (byte) (IdleTimeout/IdleTimeoutMultiplier);
                buffer[3] = (byte) (FlipLX ? 0x01 : 0x00);
                buffer[4] = (byte) (FlipLY ? 0x01 : 0x00);
                buffer[5] = (byte) (FlipRX ? 0x01 : 0x00);
                buffer[6] = (byte) (FlipRY ? 0x01 : 0x00);
                buffer[7] = (byte) (DisableLED ? 0x01 : 0x00);
                buffer[8] = (byte) (DisableRumble ? 0x01 : 0x00);
                buffer[9] = (byte) (SwapTriggers ? 0x01 : 0x00);
                buffer[10] = (byte) (Latency/LatencyMultiplier);
                buffer[11] = DeadZoneL;
                buffer[12] = DeadZoneR;
                buffer[13] = (byte) (DisableNative ? 0x01 : 0x00);
                buffer[14] = (byte) (DisableSSP ? 0x01 : 0x00);
                buffer[15] = Brightness;
                buffer[16] = (byte) (Repair ? 0x01 : 0x00);

                return buffer;
            }
            set
            {
                IdleTimeout = value[2];
                FlipLX = value[3] == 0x01;
                FlipLY = value[4] == 0x01;
                FlipRX = value[5] == 0x01;
                FlipRY = value[6] == 0x01;
                DisableLED = value[7] == 0x01;
                DisableRumble = value[8] == 0x01;
                SwapTriggers = value[9] == 0x01;
                Latency = value[10];
                DeadZoneL = value[11];
                DeadZoneR = value[12];
                DisableNative = value[13] == 0x01;
                DisableSSP = value[14] == 0x01;
                Brightness = value[15];
                Repair = value[16] == 0x01;
            }
        }

        public static byte[] BdLink
        {
            get { return m_BD_Link; }
        }

        public static void Load()
        {
            Settings.Default.Reload();
        }

        public static void Save()
        {
            Settings.Default.Save();
        }
    }
}