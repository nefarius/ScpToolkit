#include "StdAfx.h"
#define XINPUT_FUNCTIONS 11

static BOOL l_bInited = false;

static HMODULE l_hXInputDll = NULL;
static FARPROC l_hXInputFunc[] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL }; 

typedef DWORD (WINAPI *XInputGetStateFunction)(__in DWORD dwUserIndex, __out XINPUT_STATE* pState);
typedef DWORD (WINAPI *XInputSetStateFunction)(__in DWORD dwUserIndex, __in  XINPUT_VIBRATION* pVibration);
typedef DWORD (WINAPI *XInputGetCapabilitiesFunction)(__in DWORD dwUserIndex, __in DWORD dwFlags, __out XINPUT_CAPABILITIES* pCapabilities);
typedef void  (WINAPI *XInputEnableFunction)(__in BOOL enable);
typedef DWORD (WINAPI *XInputGetDSoundAudioDeviceGuidsFunction)(__in DWORD dwUserIndex, __out GUID* pDSoundRenderGuid, __out GUID* pDSoundCaptureGuid);
typedef DWORD (WINAPI *XInputGetBatteryInformationFunction)(__in DWORD dwUserIndex, __in BYTE devType, __out XINPUT_BATTERY_INFORMATION* pBatteryInformation);
typedef DWORD (WINAPI *XInputGetKeystrokeFunction)(__in DWORD dwUserIndex, __reserved DWORD dwReserved, __out PXINPUT_KEYSTROKE pKeystroke);

// UNDOCUMENTED
typedef DWORD (WINAPI *XInputGetStateExFunction)(__in DWORD dwUserIndex, __out XINPUT_STATE* pState);
typedef DWORD (WINAPI *InputWaitForGuideButtonFunction)(__in DWORD dwUserIndex, __in  DWORD dwFlag, __out LPVOID pVoid);
typedef DWORD (WINAPI *XInputCancelGuideButtonWaitFunction)(__in DWORD dwUserIndex);
typedef DWORD (WINAPI *XInputPowerOffControllerFunction)(__in DWORD dwUserIndex);

BOOL WINAPI WRAP_LoadXInput
(
	__in BOOL enable
)
{
	if (enable && !l_bInited)
	{
		TCHAR libdir[MAX_PATH];

		GetSystemDirectory(libdir, MAX_PATH); _stprintf_s(libdir, _T("%s\\XInput1_3.dll"), libdir); 

		if ((l_hXInputDll = LoadLibrary(libdir)) != NULL)
		{
			l_hXInputFunc[ 0] = GetProcAddress(l_hXInputDll, "XInputGetState");
			l_hXInputFunc[ 1] = GetProcAddress(l_hXInputDll, "XInputSetState");
			l_hXInputFunc[ 2] = GetProcAddress(l_hXInputDll, "XInputGetCapabilities");
			l_hXInputFunc[ 3] = GetProcAddress(l_hXInputDll, "XInputEnable");
			l_hXInputFunc[ 4] = GetProcAddress(l_hXInputDll, "XInputGetDSoundAudioDeviceGuids");
			l_hXInputFunc[ 5] = GetProcAddress(l_hXInputDll, "XInputGetBatteryInformation");
			l_hXInputFunc[ 6] = GetProcAddress(l_hXInputDll, "XInputGetKeystroke");

			l_hXInputFunc[ 7] = GetProcAddress(l_hXInputDll, (LPCSTR) 100); // XInputGetStateEx
			l_hXInputFunc[ 8] = GetProcAddress(l_hXInputDll, (LPCSTR) 101); // XInputWaitForGuideButton
			l_hXInputFunc[ 9] = GetProcAddress(l_hXInputDll, (LPCSTR) 102); // XInputCancelGuideButtonWait
			l_hXInputFunc[10] = GetProcAddress(l_hXInputDll, (LPCSTR) 103); // XInputPowerOffController

			l_bInited = true;
		}
	}
	else if (!enable && l_bInited)
	{
		for (int i = 0; i < XINPUT_FUNCTIONS; i++) l_hXInputFunc[i] = NULL;

		if (l_hXInputDll)
		{
			FreeLibrary(l_hXInputDll);
			l_hXInputDll = NULL;
		}

		l_bInited = false;
	}

	return true;
}

DWORD WINAPI WRAP_XInputGetState
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state
)
{
	if (!l_bInited || !l_hXInputFunc[0]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetStateFunction)(l_hXInputFunc[0]))(dwUserIndex, pState);
}

DWORD WINAPI WRAP_XInputSetState
(
    __in DWORD             dwUserIndex,						// Index of the gamer associated with the device
    __in XINPUT_VIBRATION* pVibration						// The vibration information to send to the controller
)
{
	if (!l_bInited || !l_hXInputFunc[1]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputSetStateFunction)(l_hXInputFunc[1]))(dwUserIndex, pVibration);
}

DWORD WINAPI WRAP_XInputGetCapabilities
(
    __in  DWORD                dwUserIndex,					// Index of the gamer associated with the device
    __in  DWORD                dwFlags,						// Input flags that identify the device type
    __out XINPUT_CAPABILITIES* pCapabilities				// Receives the capabilities
)
{
	if (!l_bInited || !l_hXInputFunc[2]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetCapabilitiesFunction)(l_hXInputFunc[2]))(dwUserIndex, dwFlags, pCapabilities);
}

void WINAPI WRAP_XInputEnable
(
    __in BOOL enable										// [in] Indicates whether xinput is enabled or disabled. 
)
{
	if (!l_bInited || !l_hXInputFunc[3]) return;

	return ((XInputEnableFunction)(l_hXInputFunc[3]))(enable);
}

DWORD WINAPI WRAP_XInputGetDSoundAudioDeviceGuids
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __out GUID* pDSoundRenderGuid,							// DSound device ID for render
    __out GUID* pDSoundCaptureGuid							// DSound device ID for capture
)
{
	if (!l_bInited || !l_hXInputFunc[4]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetDSoundAudioDeviceGuidsFunction)(l_hXInputFunc[4]))(dwUserIndex, pDSoundRenderGuid, pDSoundCaptureGuid);
}

DWORD WINAPI WRAP_XInputGetBatteryInformation
(
    __in  DWORD                       dwUserIndex,			// Index of the gamer associated with the device
    __in  BYTE                        devType,				// Which device on this user index
    __out XINPUT_BATTERY_INFORMATION* pBatteryInformation	// Contains the level and types of batteries
)
{
	if (!l_bInited || !l_hXInputFunc[5]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetBatteryInformationFunction)(l_hXInputFunc[5]))(dwUserIndex, devType, pBatteryInformation);
}

DWORD WINAPI WRAP_XInputGetKeystroke
(
    __in       DWORD dwUserIndex,							// Index of the gamer associated with the device
    __reserved DWORD dwReserved,							// Reserved for future use
    __out      PXINPUT_KEYSTROKE pKeystroke					// Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
)
{
	if (!l_bInited || !l_hXInputFunc[6]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetKeystrokeFunction)(l_hXInputFunc[6]))(dwUserIndex, dwReserved, pKeystroke);
}

// UNDOCUMENTED

DWORD WINAPI WRAP_XInputGetStateEx
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state + the Guide/Home button
)
{
	if (!l_bInited || !l_hXInputFunc[7]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputGetStateExFunction)(l_hXInputFunc[7]))(dwUserIndex, pState);
}

DWORD WINAPI WRAP_XInputWaitForGuideButton
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __in  DWORD dwFlag,										// ???
    __out LPVOID pVoid										// ???
)
{
	if (!l_bInited || !l_hXInputFunc[8]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((InputWaitForGuideButtonFunction)(l_hXInputFunc[8]))(dwUserIndex, dwFlag, pVoid);
}

DWORD WINAPI WRAP_XInputCancelGuideButtonWait
(
    __in  DWORD dwUserIndex									// Index of the gamer associated with the device
)
{
	if (!l_bInited || !l_hXInputFunc[9]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputCancelGuideButtonWaitFunction)(l_hXInputFunc[9]))(dwUserIndex);
}

DWORD WINAPI WRAP_XInputPowerOffController
(
    __in  DWORD dwUserIndex									// Index of the gamer associated with the device
)
{
	if (!l_bInited || !l_hXInputFunc[10]) return ERROR_DEVICE_NOT_CONNECTED;

	return ((XInputPowerOffControllerFunction)(l_hXInputFunc[10]))(dwUserIndex);
}
