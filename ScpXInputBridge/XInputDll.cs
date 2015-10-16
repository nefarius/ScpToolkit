using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ScpXInputBridge
{
    using DWORD = UInt32;

    public partial class XInputDll : IDisposable
    {
        #region Private delegates

        private static XInputEnableFunction _originalXInputEnableFunction;
        private static XInputGetStateFunction _originalXInputGetStateFunction;
        private static XInputSetStateFunction _originalXInputSetStateFunction;
        private static XInputGetCapabilitiesFunction _originalXInputGetCapabilitiesFunction;
        private static XInputGetDSoundAudioDeviceGuidsFunction _originalXInputGetDSoundAudioDeviceGuidsFunction;
        private static XInputGetBatteryInformationFunction _originalXInputGetBatteryInformationFunction;
        private static XInputGetKeystrokeFunction _originalXInputGetKeystrokeFunction;

        #endregion

        #region Methods

        public void Dispose()
        {
            if (_dll == IntPtr.Zero) return;

            Kernel32Natives.FreeLibrary(_dll);
            _isInitialized = false;
        }

        private static Delegate GetMethod<T>(IntPtr module, string methodName)
        {
            return Marshal.GetDelegateForFunctionPointer(Kernel32Natives.GetProcAddress(module, methodName), typeof (T));
        }

        private static void Initialize()
        {
            if (_isInitialized)
                return;

            _dll = Kernel32Natives.LoadLibrary(Path.Combine(Environment.SystemDirectory, "xinput1_3.dll"));

            _originalXInputEnableFunction = (XInputEnableFunction) GetMethod<XInputEnableFunction>(_dll, "XInputEnable");
            _originalXInputGetStateFunction =
                (XInputGetStateFunction) GetMethod<XInputGetStateFunction>(_dll, "XInputGetState");
            _originalXInputSetStateFunction =
                (XInputSetStateFunction) GetMethod<XInputSetStateFunction>(_dll, "XInputSetState");
            _originalXInputGetCapabilitiesFunction =
                (XInputGetCapabilitiesFunction) GetMethod<XInputGetCapabilitiesFunction>(_dll, "XInputGetCapabilities");
            _originalXInputGetDSoundAudioDeviceGuidsFunction =
                (XInputGetDSoundAudioDeviceGuidsFunction) GetMethod<XInputGetDSoundAudioDeviceGuidsFunction>(_dll,
                    "XInputGetDSoundAudioDeviceGuids");
            _originalXInputGetBatteryInformationFunction =
                (XInputGetBatteryInformationFunction)
                    GetMethod<XInputGetBatteryInformationFunction>(_dll, "XInputGetBatteryInformation");
            _originalXInputGetKeystrokeFunction =
                (XInputGetKeystrokeFunction) GetMethod<XInputGetKeystrokeFunction>(_dll, "XInputGetKeystroke");

            _isInitialized = true;
        }

        #endregion
        
        #region Private fields

        private static IntPtr _dll = IntPtr.Zero;
        private static volatile bool _isInitialized;

        #endregion

        #region Native structs

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
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
        public struct SCP_EXTN
        {
            public float SCP_UP;
            public float SCP_RIGHT;
            public float SCP_DOWN;
            public float SCP_LEFT;

            public float SCP_LX;
            public float SCP_LY;

            public float SCP_L1;
            public float SCP_L2;
            public float SCP_L3;

            public float SCP_RX;
            public float SCP_RY;

            public float SCP_R1;
            public float SCP_R2;
            public float SCP_R3;

            public float SCP_T;
            public float SCP_C;
            public float SCP_X;
            public float SCP_S;

            public float SCP_SELECT;
            public float SCP_START;

            public float SCP_PS;
        };

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

        #endregion

        #region Delegates for GetProcAddress

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void XInputEnableFunction(bool enable);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetStateFunction(uint dwUserIndex, ref XINPUT_STATE pState);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputSetStateFunction(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetCapabilitiesFunction(uint dwUserIndex, uint dwFlags,
            ref XINPUT_CAPABILITIES pCapabilities);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetDSoundAudioDeviceGuidsFunction(uint dwUserIndex, ref Guid pDSoundRenderGuid,
            ref Guid pDSoundCaptureGuid);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetBatteryInformationFunction(uint dwUserIndex, byte devType,
            ref XINPUT_BATTERY_INFORMATION pBatteryInformation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetKeystrokeFunction(
            uint dwUserIndex, uint dwReserved, ref XINPUT_KEYSTROKE pKeystroke);

        #endregion
    }
}
