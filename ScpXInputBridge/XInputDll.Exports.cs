using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
#if !EXPERIMENTAL
using System.Threading;
using ScpControl.Shared.Core;
using ScpControl.Shared.Utilities;
#endif
using ScpControl.Shared.XInput;

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
#if !EXPERIMENTAL
            return OriginalXInputGetStateFunction.Value(dwUserIndex, ref pState);
#else
            if (OriginalXInputGetStateFunction.Value(dwUserIndex, ref pState) == ResultWin32.ERROR_SUCCESS)
            {
                return ResultWin32.ERROR_SUCCESS;
            }

            try
            {
                ScpHidReport report = null;

                while (dwUserIndex == 0 && (report = Proxy.GetReport(dwUserIndex)) == null)
                {
                    Thread.Sleep(100);
                }

                if (report == null || report.PadState != DsState.Connected)
                {
                    return ResultWin32.ERROR_DEVICE_NOT_CONNECTED;
                }
                
                var xPad = new XINPUT_GAMEPAD();

                pState.dwPacketNumber = report.PacketCounter;

                switch (report.Model)
                {
                    case DsModel.DS3:
                    {
                        // select & start
                        xPad.wButtons |= (ushort) report[Ds3Button.Select].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Start].Xbox360Button;

                        // d-pad
                        xPad.wButtons |= (ushort) report[Ds3Button.Up].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Right].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Down].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Left].Xbox360Button;

                        // shoulders
                        xPad.wButtons |= (ushort) report[Ds3Button.L1].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.R1].Xbox360Button;

                        // face buttons
                        xPad.wButtons |= (ushort) report[Ds3Button.Triangle].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Circle].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Cross].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.Square].Xbox360Button;

                        // PS/Guide
                        xPad.wButtons |= (ushort) report[Ds3Button.Ps].Xbox360Button;

                        // thumbs
                        xPad.wButtons |= (ushort) report[Ds3Button.L3].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds3Button.R3].Xbox360Button;

                        // triggers
                        xPad.bLeftTrigger = report[Ds3Axis.L2].Value;
                        xPad.bRightTrigger = report[Ds3Axis.R2].Value;

                        // thumb axes
                        xPad.sThumbLX = (short) +DsMath.Scale(report[Ds3Axis.Lx].Value, false);
                        xPad.sThumbLY = (short) -DsMath.Scale(report[Ds3Axis.Ly].Value, false);
                        xPad.sThumbRX = (short) +DsMath.Scale(report[Ds3Axis.Rx].Value, false);
                        xPad.sThumbRY = (short) -DsMath.Scale(report[Ds3Axis.Ry].Value, false);
                    }
                        break;
                    case DsModel.DS4:
                    {
                        // select & start
                        xPad.wButtons |= (ushort) report[Ds4Button.Share].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Options].Xbox360Button;

                        // d-pad
                        xPad.wButtons |= (ushort) report[Ds4Button.Up].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Right].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Down].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Left].Xbox360Button;

                        // shoulders
                        xPad.wButtons |= (ushort) report[Ds4Button.L1].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.R1].Xbox360Button;

                        // face buttons
                        xPad.wButtons |= (ushort) report[Ds4Button.Triangle].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Circle].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Cross].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.Square].Xbox360Button;

                        // PS/Guide
                        xPad.wButtons |= (ushort) report[Ds4Button.Ps].Xbox360Button;

                        // thumbs
                        xPad.wButtons |= (ushort) report[Ds4Button.L3].Xbox360Button;
                        xPad.wButtons |= (ushort) report[Ds4Button.R3].Xbox360Button;

                        // triggers
                        xPad.bLeftTrigger = report[Ds4Axis.L2].Value;
                        xPad.bRightTrigger = report[Ds4Axis.R2].Value;

                        // thumb axes
                        xPad.sThumbLX = (short) +DsMath.Scale(report[Ds4Axis.Lx].Value, false);
                        xPad.sThumbLY = (short) -DsMath.Scale(report[Ds4Axis.Ly].Value, false);
                        xPad.sThumbRX = (short) +DsMath.Scale(report[Ds4Axis.Rx].Value, false);
                        xPad.sThumbRY = (short) -DsMath.Scale(report[Ds4Axis.Ry].Value, false);
                    }
                        break;
                }

                pState.Gamepad = xPad;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
                return ResultWin32.ERROR_DEVICE_NOT_CONNECTED;
            }

            return ResultWin32.ERROR_SUCCESS;
#endif
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
#if !EXPERIMENTAL
            return OriginalXInputGetCapabilitiesFunction.Value(dwUserIndex, dwFlags, ref pCapabilities);
#else

            Log.DebugFormat("dwUserIndex = {0}", dwUserIndex);

            if (OriginalXInputGetCapabilitiesFunction.Value(dwUserIndex, dwFlags, ref pCapabilities) ==
                ResultWin32.ERROR_SUCCESS)
            {
                return ResultWin32.ERROR_SUCCESS;
            }

            try
            {
                ScpHidReport report = Proxy.GetReport(dwUserIndex);
                if (report == null || report.PadState != DsState.Connected)
                {
                    return ResultWin32.ERROR_DEVICE_NOT_CONNECTED;
                }

                pCapabilities.Type = XInputConstants.XINPUT_DEVTYPE_GAMEPAD;
                pCapabilities.SubType = XInputConstants.XINPUT_DEVSUBTYPE_GAMEPAD;
                pCapabilities.Flags = (ushort) (XInputConstants.CapabilityFlags.XINPUT_CAPS_FFB_SUPPORTED |
                                                XInputConstants.CapabilityFlags.XINPUT_CAPS_WIRELESS);

                pCapabilities.Gamepad = new XINPUT_GAMEPAD()
                {
                    wButtons = 0xFFFF,
                    bLeftTrigger = 0xFF,
                    bRightTrigger = 0xFF
                };
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
                return ResultWin32.ERROR_DEVICE_NOT_CONNECTED;
            }

            return ResultWin32.ERROR_SUCCESS;
#endif
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