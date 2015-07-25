#pragma once

BOOL WINAPI WRAP_LoadXInput
(
	__in BOOL enable
);

DWORD WINAPI WRAP_XInputGetState
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state
);

DWORD WINAPI WRAP_XInputSetState
(
    __in DWORD             dwUserIndex,						// Index of the gamer associated with the device
    __in XINPUT_VIBRATION* pVibration						// The vibration information to send to the controller
);

DWORD WINAPI WRAP_XInputGetCapabilities
(
    __in  DWORD                dwUserIndex,					// Index of the gamer associated with the device
    __in  DWORD                dwFlags,						// Input flags that identify the device type
    __out XINPUT_CAPABILITIES* pCapabilities				// Receives the capabilities
);

void WINAPI WRAP_XInputEnable
(
    __in BOOL enable										// [in] Indicates whether xinput is enabled or disabled. 
);

DWORD WINAPI WRAP_XInputGetDSoundAudioDeviceGuids
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __out GUID* pDSoundRenderGuid,							// DSound device ID for render
    __out GUID* pDSoundCaptureGuid							// DSound device ID for capture
);

DWORD WINAPI WRAP_XInputGetBatteryInformation
(
    __in  DWORD                       dwUserIndex,			// Index of the gamer associated with the device
    __in  BYTE                        devType,				// Which device on this user index
    __out XINPUT_BATTERY_INFORMATION* pBatteryInformation	// Contains the level and types of batteries
);

DWORD WINAPI WRAP_XInputGetKeystroke
(
    __in       DWORD dwUserIndex,							// Index of the gamer associated with the device
    __reserved DWORD dwReserved,							// Reserved for future use
    __out      PXINPUT_KEYSTROKE pKeystroke					// Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
);

// UNDOCUMENTED

DWORD WINAPI WRAP_XInputGetStateEx
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state + the Guide/Home button
);

DWORD WINAPI WRAP_XInputWaitForGuideButton
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __in  DWORD dwFlag,										// ???
    __out LPVOID pVoid										// ???
);

DWORD WINAPI WRAP_XInputCancelGuideButtonWait
(
    __in  DWORD dwUserIndex									// Index of the gamer associated with the device
);

DWORD WINAPI WRAP_XInputPowerOffController
(
    __in  DWORD dwUserIndex									// Index of the gamer associated with the device
);
