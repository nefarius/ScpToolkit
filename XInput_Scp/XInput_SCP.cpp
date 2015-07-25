#include "stdafx.h"

static CSCPController* l_Pad[XUSER_MAX_COUNT];
static BOOL  l_bPassThrough = false, l_bStarted = false, l_bUnloaded = false;
static DWORD l_nPads = 0, l_nAttached = 0, l_nBtPads = 0;

BOOL LoadApi(BOOL bEnable)
{
	if (bEnable) l_nAttached++; else l_nAttached--;

	if (bEnable && !l_bStarted && !l_bUnloaded)
	{
		load_lib_usb();
		WRAP_LoadXInput(true);

		// Load Bluetooth DS3s
		CBTConnection* BtPad = new CBTConnection();

		if (BtPad->Open())
		{
			l_nBtPads = BtPad->CollectionSize;

			for(DWORD Index = 0; Index < BtPad->CollectionSize; Index++)
			{
				l_Pad[l_nPads++] = BtPad;
			}
		}
		else delete BtPad;

		CSCPController*	Pad;

		// Load DS3s
		if (l_nBtPads == 0) // Only if not using BTH Server
		{
			for (DWORD Index = 0; Index < XUSER_MAX_COUNT * CDS3Controller::CollectionSize && l_nPads < XUSER_MAX_COUNT; Index++)
			{
				Pad = new CDS3Controller(Index);

				if (Pad->Open()) l_Pad[l_nPads++] = Pad;
				else { delete Pad; break; }
			}
		}

		// Load SL3s
		for (DWORD Index = 0; Index < XUSER_MAX_COUNT * CSL3Controller::CollectionSize && l_nPads < XUSER_MAX_COUNT; Index++)
		{
			Pad = new CSL3Controller(Index);

			if (Pad->Open()) l_Pad[l_nPads++] = Pad;
			else { delete Pad; break; }
		}

		// Load DS2s
		for (DWORD Index = 0; Index < XUSER_MAX_COUNT * CDS2Controller::CollectionSize && l_nPads < XUSER_MAX_COUNT; Index++)
		{
			Pad = new CDS2Controller(Index);

			if (Pad->Open()) l_Pad[l_nPads++] = Pad;
			else delete Pad;
		}
		
		// Load X360s
		if (l_nBtPads == 0) // Only if not using BTH Server
		{
			for (DWORD Index = 0; Index < XUSER_MAX_COUNT * CX360Controller::CollectionSize && l_nPads < XUSER_MAX_COUNT; Index++)
			{
				Pad = new CX360Controller(Index);

				if (Pad->Open()) l_Pad[l_nPads++] = Pad;
				else delete Pad;
			}
		}

		l_bStarted = true;

		if (l_nPads == 0)
		{
			// No Devices found, PassThrough only
			l_bPassThrough = true;
		}
	}
	else if (!bEnable && l_bStarted && !l_bUnloaded && l_nAttached == 0)
	{
		l_bStarted = false;
		l_bUnloaded = true;

		if (l_nBtPads > 0)
		{
			l_Pad[0]->Close();
			delete l_Pad[0];
		}

		for (DWORD Index = l_nBtPads; Index < l_nPads; Index++)
		{
			l_Pad[Index]->Close();
			delete l_Pad[Index];
		}

		return WRAP_LoadXInput(bEnable);
	}

	return true;
}

DWORD WINAPI XInputGetState
(
    __in  DWORD         dwUserIndex,						// Index of the gamer associated with the device
    __out XINPUT_STATE* pState								// Receives the current state
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetState(dwUserIndex, pState);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetState(dwUserIndex, pState);
}

DWORD WINAPI XInputSetState
(
    __in DWORD             dwUserIndex,						// Index of the gamer associated with the device
    __in XINPUT_VIBRATION* pVibration						// The vibration information to send to the controller
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->SetState(dwUserIndex, pVibration);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputSetState(dwUserIndex, pVibration);
}

DWORD WINAPI XInputGetCapabilities
(
    __in  DWORD                dwUserIndex,					// Index of the gamer associated with the device
    __in  DWORD                dwFlags,						// Input flags that identify the device type
    __out XINPUT_CAPABILITIES* pCapabilities				// Receives the capabilities
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetCapabilities(dwUserIndex, dwFlags, pCapabilities);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetCapabilities(dwUserIndex, dwFlags, pCapabilities);
}

void WINAPI XInputEnable
(
    __in BOOL enable										// [in] Indicates whether xinput is enabled or disabled. 
)
{
	if (l_bUnloaded) return;

	if (!l_bStarted) LoadApi(true);

	if (!enable)
	{
		XINPUT_VIBRATION Vibration = { 0, 0 };

		for (DWORD nPad = 0; nPad < l_nPads; nPad++)
		{
			XInputSetState(nPad, &Vibration);
		}
	}

	WRAP_XInputEnable(enable);
}

DWORD WINAPI XInputGetDSoundAudioDeviceGuids
(
    __in  DWORD dwUserIndex,								// Index of the gamer associated with the device
    __out GUID* pDSoundRenderGuid,							// DSound device ID for render
    __out GUID* pDSoundCaptureGuid							// DSound device ID for capture
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetDSoundAudioDeviceGuids(dwUserIndex, pDSoundRenderGuid, pDSoundCaptureGuid);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetDSoundAudioDeviceGuids(dwUserIndex, pDSoundRenderGuid, pDSoundCaptureGuid);
}

DWORD WINAPI XInputGetBatteryInformation
(
    __in  DWORD                       dwUserIndex,			// Index of the gamer associated with the device
    __in  BYTE                        devType,				// Which device on this user index
    __out XINPUT_BATTERY_INFORMATION* pBatteryInformation	// Contains the level and types of batteries
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetBatteryInformation(dwUserIndex, devType, pBatteryInformation);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetBatteryInformation(dwUserIndex, devType, pBatteryInformation);
}

DWORD WINAPI XInputGetKeystroke
(
    __in       DWORD dwUserIndex,							// Index of the gamer associated with the device
    __reserved DWORD dwReserved,							// Reserved for future use
    __out      PXINPUT_KEYSTROKE pKeystroke					// Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetKeystroke(dwUserIndex, dwReserved, pKeystroke);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetKeystroke(dwUserIndex, dwReserved, pKeystroke);
}

DWORD WINAPI XInputGetExtended(DWORD dwUserIndex, SCP_EXTN* pPressure)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetExtended(dwUserIndex, pPressure);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return ERROR_NOT_SUPPORTED;
}

// UNDOCUMENTED

DWORD WINAPI XInputGetStateEx(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->GetStateEx(dwUserIndex, pState);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputGetStateEx(dwUserIndex, pState);
}

DWORD WINAPI XInputWaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->WaitForGuideButton(dwUserIndex, dwFlag, pVoid);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputWaitForGuideButton(dwUserIndex, dwFlag, pVoid);
}

DWORD WINAPI XInputCancelGuideButtonWait(DWORD dwUserIndex)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->CancelGuideButtonWait(dwUserIndex);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputCancelGuideButtonWait(dwUserIndex);
}

DWORD WINAPI XInputPowerOffController(DWORD dwUserIndex)
{
	if (l_bUnloaded) return ERROR_DEVICE_NOT_CONNECTED;

	if (!l_bStarted) LoadApi(true);

	if (!l_bPassThrough)
	{
		if (dwUserIndex < l_nPads)
		{
			return l_Pad[dwUserIndex]->PowerOffController(dwUserIndex);
		}
		else
		{
			return ERROR_DEVICE_NOT_CONNECTED;
		}
	}

	return WRAP_XInputPowerOffController(dwUserIndex);
}
