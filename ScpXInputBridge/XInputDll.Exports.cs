using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace ScpXInputBridge
{
    public partial class XInputDll
    {
        #region XInput proxy functions

        /// <summary>
        ///     Sets the reporting state of XInput.
        /// </summary>
        /// <param name="enable">
        ///     If enable is FALSE, XInput will only send neutral data in response to
        ///     <see cref="XInputGetState" /> (all buttons up, axes centered, and triggers at 0). <see cref="XInputSetState" />
        ///     calls will be registered but not sent to the device. Sending any value other than FALSE will restore reading and
        ///     writing functionality to normal.
        /// </param>
        [DllExport("XInputEnable", CallingConvention.StdCall)]
        public static void XInputEnable(bool enable)
        {
            OriginalXInputEnableFunction.Value(enable);
        }

        /// <summary>
        ///     Retrieves the current state of the specified controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="pState">
        ///     Pointer to an <see cref="XINPUT_STATE" /> structure that receives the current state of the
        ///     controller.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS. If the controller is not connected, the return
        ///     value is ERROR_DEVICE_NOT_CONNECTED.
        /// </returns>
        [DllExport("XInputGetState", CallingConvention.StdCall)]
        public static uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState)
        {
            return OriginalXInputGetStateFunction.Value(dwUserIndex, ref pState);
        }

        /// <summary>
        ///     Sends data to a connected controller. This function is used to activate the vibration function of a controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="pVibration">
        ///     Pointer to an <see cref="XINPUT_VIBRATION" /> structure containing the vibration information
        ///     to send to the controller.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS. If the controller is not connected, the return
        ///     value is ERROR_DEVICE_NOT_CONNECTED.
        /// </returns>
        [DllExport("XInputSetState", CallingConvention.StdCall)]
        public static uint XInputSetState(uint dwUserIndex, ref XINPUT_VIBRATION pVibration)
        {
            return OriginalXInputSetStateFunction.Value(dwUserIndex, ref pVibration);
        }

        /// <summary>
        ///     Retrieves the capabilities and features of a connected controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="dwFlags">
        ///     Input flags that identify the controller type. If this value is 0, then the capabilities of all
        ///     controllers connected to the system are returned.
        /// </param>
        /// <param name="pCapabilities">
        ///     Pointer to an <see cref="XINPUT_CAPABILITIES" /> structure that receives the controller
        ///     capabilities.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS. If the controller is not connected, the return
        ///     value is ERROR_DEVICE_NOT_CONNECTED.
        /// </returns>
        [DllExport("XInputGetCapabilities", CallingConvention.StdCall)]
        public static uint XInputGetCapabilities(uint dwUserIndex, uint dwFlags,
            ref XINPUT_CAPABILITIES pCapabilities)
        {
            return OriginalXInputGetCapabilitiesFunction.Value(dwUserIndex, dwFlags, ref pCapabilities);
        }

        /// <summary>
        ///     Gets the sound rendering and sound capture device GUIDs that are associated with the headset connected to the
        ///     specified controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="pDSoundRenderGuid">Pointer that receives the GUID of the headset sound rendering device.</param>
        /// <param name="pDSoundCaptureGuid">Pointer that receives the GUID of the headset sound capture device.</param>
        /// <returns>
        ///     If the function successfully retrieves the device IDs for render and capture, the return code is
        ///     ERROR_SUCCESS. If there is no headset connected to the controller, the function also retrieves ERROR_SUCCESS with
        ///     GUID_NULL as the values for pDSoundRenderGuid and pDSoundCaptureGuid. If the controller port device is not
        ///     physically connected, the function returns ERROR_DEVICE_NOT_CONNECTED. If the function fails, it returns a valid
        ///     Win32 error code.
        /// </returns>
        /// <remarks>XInputGetDSoundAudioDeviceGuids is deprecated because it isn't supported by Windows 8 (XInput 1.4).</remarks>
        [DllExport("XInputGetDSoundAudioDeviceGuids", CallingConvention.StdCall)]
        public static uint XInputGetDSoundAudioDeviceGuids(uint dwUserIndex, ref Guid pDSoundRenderGuid,
            ref Guid pDSoundCaptureGuid)
        {
            return OriginalXInputGetDSoundAudioDeviceGuidsFunction.Value(dwUserIndex, ref pDSoundRenderGuid,
                ref pDSoundCaptureGuid);
        }

        /// <summary>
        ///     Retrieves the battery type and charge status of a wireless controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="devType">
        ///     Specifies which device associated with this user index should be queried. Must be
        ///     BATTERY_DEVTYPE_GAMEPAD or BATTERY_DEVTYPE_HEADSET.
        /// </param>
        /// <param name="pBatteryInformation">
        ///     Pointer to an <see cref="XINPUT_BATTERY_INFORMATION" /> structure that receives the
        ///     battery information.
        /// </param>
        /// <returns>If the function succeeds, the return value is ERROR_SUCCESS.</returns>
        [DllExport("XInputGetBatteryInformation", CallingConvention.StdCall)]
        public static uint XInputGetBatteryInformation(uint dwUserIndex, byte devType,
            ref XINPUT_BATTERY_INFORMATION pBatteryInformation)
        {
            return OriginalXInputGetBatteryInformationFunction.Value(dwUserIndex, devType, ref pBatteryInformation);
        }

        /// <summary>
        ///     Retrieves a gamepad input event.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="dwReserved">Reserved</param>
        /// <param name="pKeystroke">Pointer to an <see cref="XINPUT_KEYSTROKE" /> structure that receives an input event.</param>
        /// <returns>
        ///     If the function succeeds, the return value is ERROR_SUCCESS. If no new keys have been pressed, the return
        ///     value is ERROR_EMPTY. If the controller is not connected or the user has not activated it, the return value is
        ///     ERROR_DEVICE_NOT_CONNECTED. See the Remarks section below. If the function fails, the return value is an error code
        ///     defined in Winerror.h. The function does not use SetLastError to set the calling thread's last-error code.
        /// </returns>
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