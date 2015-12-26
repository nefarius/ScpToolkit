using System.Runtime.InteropServices;
using ScpControl.Shared.Core;

namespace ScpXInputBridge
{
    public partial class XInputDll
    {
        #region Native structs

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            [MarshalAs(UnmanagedType.U2)]
            public X360Button wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_VIBRATION
        {
            public uint wLeftMotorSpeed;
            public uint wRightMotorSpeed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_CAPABILITIES
        {
            public byte Type;
            public byte SubType;
            public ushort Flags;
            public XINPUT_GAMEPAD Gamepad;
            public XINPUT_VIBRATION Vibration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_BATTERY_INFORMATION
        {
            public byte BatteryType;
            public byte BatteryLevel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_KEYSTROKE
        {
            public ushort VirtualKey;
            public char Unicode;
            public ushort Flags;
            public byte UserIndex;
            public byte HidCode;
        }

        public static class ResultWin32
        {
            /// <summary>
            ///     The operation completed successfully.
            /// </summary>
            public const int ERROR_SUCCESS = 0;

            /// <summary>
            ///     The device is not connected.
            /// </summary>
            public const int ERROR_DEVICE_NOT_CONNECTED = 1167;
        }

        #endregion
    }
}
