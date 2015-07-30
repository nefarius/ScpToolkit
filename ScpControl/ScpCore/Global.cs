namespace ScpControl.ScpCore
{
    public static class Global
    {
        private static BackingStore m_Config = new BackingStore();

        private static byte[] m_BD_Link =
        {
            0x56, 0xE8, 0x81, 0x38, 0x08, 0x06, 0x51, 0x41, 0xC0, 0x7F, 0x12, 0xAA,
            0xD9, 0x66, 0x3C, 0xCE
        };

        private static int m_IdleTimeout = 60000;
        private static int m_Latency = 16;

        public static bool FlipLX
        {
            get { return m_Config.LX; }
            set { m_Config.LX = value; }
        }

        public static bool FlipLY
        {
            get { return m_Config.LY; }
            set { m_Config.LY = value; }
        }

        public static bool FlipRX
        {
            get { return m_Config.RX; }
            set { m_Config.RX = value; }
        }

        public static bool FlipRY
        {
            get { return m_Config.RY; }
            set { m_Config.RY = value; }
        }

        public static bool DisableLED
        {
            get { return m_Config.LED; }
            set { m_Config.LED = value; }
        }

        public static bool DisableRumble
        {
            get { return m_Config.Rumble; }
            set { m_Config.Rumble = value; }
        }

        public static bool SwapTriggers
        {
            get { return m_Config.Triggers; }
            set { m_Config.Triggers = value; }
        }

        public static bool DisableLightBar
        {
            get { return m_Config.Brightness == 0; }
        }

        public static bool IdleDisconnect
        {
            get { return m_Config.Idle != 0; }
        }

        public static int IdleTimeout
        {
            get { return m_Config.Idle; }
            set { m_Config.Idle = value * m_IdleTimeout; }
        }

        public static int Latency
        {
            get { return m_Config.Latency; }
            set { m_Config.Latency = value * m_Latency; }
        }

        public static byte DeadZoneL
        {
            get { return m_Config.DeadL; }
            set { m_Config.DeadL = value; }
        }

        public static byte DeadZoneR
        {
            get { return m_Config.DeadR; }
            set { m_Config.DeadR = value; }
        }

        public static bool DisableNative
        {
            get { return m_Config.Native; }
            set { m_Config.Native = value; }
        }

        public static bool DisableSSP
        {
            get { return m_Config.SSP; }
            set { m_Config.SSP = value; }
        }

        public static byte Brightness
        {
            get { return m_Config.Brightness; }
            set { m_Config.Brightness = value; }
        }

        public static int Bus
        {
            get { return m_Config.Bus; }
            set { m_Config.Bus = value; }
        }

        public static bool Repair
        {
            get { return m_Config.Repair; }
            set { m_Config.Repair = value; }
        }

        public static byte[] Packed
        {
            get
            {
                var Buffer = new byte[17];

                Buffer[1] = 0x03;
                Buffer[2] = (byte)(IdleTimeout / m_IdleTimeout);
                Buffer[3] = (byte)(FlipLX ? 0x01 : 0x00);
                Buffer[4] = (byte)(FlipLY ? 0x01 : 0x00);
                Buffer[5] = (byte)(FlipRX ? 0x01 : 0x00);
                Buffer[6] = (byte)(FlipRY ? 0x01 : 0x00);
                Buffer[7] = (byte)(DisableLED ? 0x01 : 0x00);
                Buffer[8] = (byte)(DisableRumble ? 0x01 : 0x00);
                Buffer[9] = (byte)(SwapTriggers ? 0x01 : 0x00);
                Buffer[10] = (byte)(Latency / m_Latency);
                Buffer[11] = DeadZoneL;
                Buffer[12] = DeadZoneR;
                Buffer[13] = (byte)(DisableNative ? 0x01 : 0x00);
                Buffer[14] = (byte)(DisableSSP ? 0x01 : 0x00);
                Buffer[15] = Brightness;
                Buffer[16] = (byte)(Repair ? 0x01 : 0x00);
                ;

                return Buffer;
            }
            set
            {
                try
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
                catch
                {
                }
            }
        }

        public static byte[] BD_Link
        {
            get { return m_BD_Link; }
        }

        public static void Load()
        {
            m_Config.Load();
        }

        public static void Save()
        {
            m_Config.Save();
        }
    }
}
