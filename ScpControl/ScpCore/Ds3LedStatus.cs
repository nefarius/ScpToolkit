namespace ScpControl.ScpCore
{
    public enum Ds3LedMode : byte
    {
        Disabled = 0x00,
        PadId = 0x01,
        ChargeStatus = 0x02,
        Custom = 0xFF
    }

    public class Ds3LedStatus
    {
        public Ds3LedMode Mode { get; set; }

        public bool IsFlashing { get; set; }

        public bool IsLed1Enabled { get; set; }

        public bool IsLed2Enabled { get; set; }

        public bool IsLed3Enabled { get; set; }

        public bool IsLed4Enabled { get; set; }
    }
}
