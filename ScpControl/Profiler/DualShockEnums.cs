using System;
using System.ComponentModel;

namespace ScpControl.Profiler
{
    public enum DsOffset
    {
        Pad = 0,
        State = 1,
        Battery = 2,
        Connection = 3,
        Model = 89,
        Address = 90
    };

    public enum DsState
    {
        [Description("Disconnected")]
        Disconnected = 0x00,
        [Description("Reserved")]
        Reserved = 0x01,
        [Description("Connected")]
        Connected = 0x02
    };

    /// <summary>
    ///     DualShock connection types.
    /// </summary>
    public enum DsConnection
    {
        [Description("None")]
        None = 0x00,
        [Description("USB")]
        USB = 0x01,
        [Description("Bluetooth")]
        BTH = 0x02
    };

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

    public enum DsPadId : byte
    {
        None = 0xFF,
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        All = 0x04
    };

    /// <summary>
    ///     DualShock models.
    /// </summary>
    public enum DsModel : byte
    {
        [Description("None")]
        None = 0,
        [Description("DualShock 3")]
        DS3 = 1,
        [Description("DualShock 4")]
        DS4 = 2,
        [Description("Generic Gamepad")]
        Generic = 3
    }

    public enum DsMatch
    {
        None = 0,
        Global = 1,
        Pad = 2,
        Mac = 3
    }

    [Flags]
    public enum X360Button : uint
    {
        None = 0,

        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,

        Start = 1 << 4,
        Back = 1 << 5,
        LS = 1 << 6,
        RS = 1 << 7,

        LB = 1 << 8,
        RB = 1 << 9,

        Guide = 1 << 10,

        A = 1 << 12,
        B = 1 << 13,
        X = 1 << 14,
        Y = 1 << 15
    }

    public enum X360Axis
    {
        BT_Lo = 10,
        BT_Hi = 11,

        LT = 12,
        RT = 13,

        LX_Lo = 14,
        LX_Hi = 15,
        LY_Lo = 16,
        LY_Hi = 17,

        RX_Lo = 18,
        RX_Hi = 19,
        RY_Lo = 20,
        RY_Hi = 21
    }
}
