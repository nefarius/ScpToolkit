using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ScpXInputBridge
{
    using DWORD = UInt32;

    public partial class XInputDll : IDisposable
    {
        #region Private delegates

        private static readonly Lazy<XInputEnableFunction> OriginalXInputEnableFunction =
            new Lazy<XInputEnableFunction>(() =>
            {
                Initialize();
                return (XInputEnableFunction) GetMethod<XInputEnableFunction>(_dll, "XInputEnable");
            });

        private static readonly Lazy<XInputGetStateFunction> OriginalXInputGetStateFunction = new Lazy
            <XInputGetStateFunction>(
            () =>
            {
                Initialize();
                return (XInputGetStateFunction) GetMethod<XInputGetStateFunction>(_dll, "XInputGetState");
            });

        private static readonly Lazy<XInputSetStateFunction> OriginalXInputSetStateFunction = new Lazy
            <XInputSetStateFunction>(
            () =>
            {
                Initialize();
                return (XInputSetStateFunction) GetMethod<XInputSetStateFunction>(_dll, "XInputSetState");
            });

        private static readonly Lazy<XInputGetCapabilitiesFunction> OriginalXInputGetCapabilitiesFunction = new Lazy
            <XInputGetCapabilitiesFunction>(
            () =>
            {
                Initialize();
                return
                    (XInputGetCapabilitiesFunction)
                        GetMethod<XInputGetCapabilitiesFunction>(_dll, "XInputGetCapabilities");
            });

        private static readonly Lazy<XInputGetDSoundAudioDeviceGuidsFunction>
            OriginalXInputGetDSoundAudioDeviceGuidsFunction = new Lazy<XInputGetDSoundAudioDeviceGuidsFunction>(
                () =>
                {
                    Initialize();
                    return
                        (XInputGetDSoundAudioDeviceGuidsFunction)
                            GetMethod<XInputGetDSoundAudioDeviceGuidsFunction>(_dll,
                                "XInputGetDSoundAudioDeviceGuids");
                });

        private static readonly Lazy<XInputGetBatteryInformationFunction> OriginalXInputGetBatteryInformationFunction = new Lazy
            <XInputGetBatteryInformationFunction>(
            () =>
            {
                Initialize();
                return (XInputGetBatteryInformationFunction)
                    GetMethod<XInputGetBatteryInformationFunction>(_dll, "XInputGetBatteryInformation");
            });

        private static readonly Lazy<XInputGetKeystrokeFunction> OriginalXInputGetKeystrokeFunction = new Lazy
            <XInputGetKeystrokeFunction>(
            () =>
            {
                Initialize();
                return (XInputGetKeystrokeFunction) GetMethod<XInputGetKeystrokeFunction>(_dll, "XInputGetKeystroke");
            });

        #endregion

        #region Methods

        /// <summary>
        ///     Free resources.
        /// </summary>
        /// TODO: does this even get called?
        public void Dispose()
        {
            if (_dll == IntPtr.Zero) return;

            Kernel32Natives.FreeLibrary(_dll);
            _isInitialized = false;
        }

        /// <summary>
        ///     Translates a native method into a managed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the target delegate.</typeparam>
        /// <param name="module">The module name to search the function in.</param>
        /// <param name="methodName">The native finctions' name.</param>
        /// <returns>Returns the managed delegate.</returns>
        private static Delegate GetMethod<T>(IntPtr module, string methodName)
        {
            return Marshal.GetDelegateForFunctionPointer(Kernel32Natives.GetProcAddress(module, methodName), typeof (T));
        }

        /// <summary>
        ///     Loads native dependencies.
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized)
                return;

            _dll = Kernel32Natives.LoadLibrary(Path.Combine(Environment.SystemDirectory, "xinput1_3.dll"));

            if (_dll != IntPtr.Zero)
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