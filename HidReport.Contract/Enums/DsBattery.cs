namespace HidReport.Contract.Enums
{
    /// <summary>
    ///     DualShock rechargeable battery status.
    /// </summary>
    public enum DsBattery : byte
    {
        None = 0x00,
        Dying = 0x01,
        Low = 0x02,
        Medium = 0x03,
        High = 0x04,
        Full = 0x05,
        Charging = 0xEE,
        Charged = 0xEF
    };
}