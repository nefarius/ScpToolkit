using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace ScpXInputBridge
{
    public partial class XInputDll
    {
        #region XInput proxy functions

        [DllExport("XInputEnable", CallingConvention.StdCall)]
        public static void XInputEnable(bool enable)
        {
            OriginalXInputEnableFunction.Value(enable);
        }

        [DllExport("XInputGetState", CallingConvention.StdCall)]
        public static uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState)
        {
            return OriginalXInputGetStateFunction.Value(dwUserIndex, ref pState);
        }

        [DllExport("XInputSetState", CallingConvention.StdCall)]
        public static uint XInputSetState(uint dwUserIndex, ref XINPUT_VIBRATION pVibration)
        {
            return OriginalXInputSetStateFunction.Value(dwUserIndex, ref pVibration);
        }

        [DllExport("XInputGetCapabilities", CallingConvention.StdCall)]
        public static uint XInputGetCapabilities(uint dwUserIndex, uint dwFlags,
            ref XINPUT_CAPABILITIES pCapabilities)
        {
            return OriginalXInputGetCapabilitiesFunction.Value(dwUserIndex, dwFlags, ref pCapabilities);
        }

        [DllExport("XInputGetDSoundAudioDeviceGuids", CallingConvention.StdCall)]
        public static uint XInputGetDSoundAudioDeviceGuids(uint dwUserIndex, ref Guid pDSoundRenderGuid,
            ref Guid pDSoundCaptureGuid)
        {
            return OriginalXInputGetDSoundAudioDeviceGuidsFunction.Value(dwUserIndex, ref pDSoundRenderGuid,
                ref pDSoundCaptureGuid);
        }

        [DllExport("XInputGetBatteryInformation", CallingConvention.StdCall)]
        public static uint XInputGetBatteryInformation(uint dwUserIndex, byte devType,
            ref XINPUT_BATTERY_INFORMATION pBatteryInformation)
        {
            return OriginalXInputGetBatteryInformationFunction.Value(dwUserIndex, devType, ref pBatteryInformation);
        }

        [DllExport("XInputGetKeystroke", CallingConvention.StdCall)]
        public static uint XInputGetKeystroke(uint dwUserIndex, uint dwReserved, ref XINPUT_KEYSTROKE pKeystroke)
        {
            return OriginalXInputGetKeystrokeFunction.Value(dwUserIndex, dwReserved, ref pKeystroke);
        }

        #region Undocumented functions

        [DllExport("XInputGetStateEx", CallingConvention.StdCall)]
        public static uint XInputGetStateEx(uint dwUserIndex, ref XINPUT_STATE pState)
        {
            throw new NotImplementedException();
        }

        [DllExport("XInputWaitForGuideButton", CallingConvention.StdCall)]
        public static uint XInputWaitForGuideButton(uint dwUserIndex, uint dwFlag, IntPtr pVoid)
        {
            throw new NotImplementedException();
        }

        [DllExport("XInputCancelGuideButtonWait", CallingConvention.StdCall)]
        public static uint XInputCancelGuideButtonWait(uint dwUserIndex)
        {
            throw new NotImplementedException();
        }

        [DllExport("XInputPowerOffController", CallingConvention.StdCall)]
        public static uint XInputPowerOffController(uint dwUserIndex)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}