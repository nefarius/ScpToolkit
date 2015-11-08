using System;
using System.ComponentModel;

namespace ScpControl.ScpCore
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

    [Obsolete]
    [Flags]
    public enum Ds3Button : uint
    {
        None = 0,

        Select = 1 << 0,
        L3 = 1 << 1,
        R3 = 1 << 2,
        Start = 1 << 3,

        Up = 1 << 4,
        Right = 1 << 5,
        Down = 1 << 6,
        Left = 1 << 7,

        L2 = 1 << 8,
        R2 = 1 << 9,
        L1 = 1 << 10,
        R1 = 1 << 11,

        Triangle = 1 << 12,
        Circle = 1 << 13,
        Cross = 1 << 14,
        Square = 1 << 15,

        PS = 1 << 16
    }

    [Obsolete]
    public enum Ds3Axis
    {
        None = 0,

        LX = 14,
        LY = 15,
        RX = 16,
        RY = 17,

        Up = 22,
        Right = 23,
        Down = 24,
        Left = 25,

        L2 = 26,
        R2 = 27,
        L1 = 28,
        R1 = 29,

        Triangle = 30,
        Circle = 31,
        Cross = 32,
        Square = 33
    }

    [Obsolete]
    [Flags]
    public enum Ds4Button : uint
    {
        None = 0,

        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,

        Square = 1 << 4,
        Cross = 1 << 5,
        Circle = 1 << 6,
        Triangle = 1 << 7,

        L1 = 1 << 8,
        R1 = 1 << 9,
        L2 = 1 << 10,
        R2 = 1 << 11,

        Share = 1 << 12,
        Options = 1 << 13,
        L3 = 1 << 14,
        R3 = 1 << 15,

        PS = 1 << 16,
        TouchPad = 1 << 17
    }

    [Obsolete]
    public enum Ds4Axis
    {
        None = 0,

        LX = 9,
        LY = 10,
        RX = 11,
        RY = 12,

        L2 = 16,
        R2 = 17
    }
}
