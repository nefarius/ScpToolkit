#include "StdAfx.h"

CSCPController::CSCPController()
{
	m_bConnected = false;
	m_xConnected = false;

	m_Report      = NULL;
	m_deviceId    = NULL;
	m_devicePath  = NULL;
	m_lpHidDevice = NULL;
}

CSCPController::CSCPController(DWORD dwIndex, DWORD dwReportSize)
{
	m_bConnected = false;
	m_xConnected = false;

	m_bReportEnabled  = true;

	m_dwIndex = dwIndex;

	m_deviceId   = NULL;
	m_devicePath = NULL;

	m_dwReportSize = dwReportSize;

	if (m_dwReportSize) m_Report = (BYTE *) calloc(m_dwReportSize, sizeof(BYTE));
	else m_Report = NULL;

	m_lpHidDevice = new HID_DEVICE();

	memset(&m_Writer, 0, sizeof(OVERLAPPED));
	m_Writer.hEvent = CreateEvent(NULL, TRUE, TRUE, NULL);

	memset(&m_Reader, 0, sizeof(OVERLAPPED));
	m_Reader.hEvent = CreateEvent(NULL, TRUE, TRUE, NULL);
}

CSCPController::~CSCPController(void)
{
	if (m_deviceId  ) free(m_deviceId);
	if (m_devicePath) free(m_devicePath);

	if (m_Report) free(m_Report);

	Close();

	if (m_lpHidDevice)
	{
		delete m_lpHidDevice; m_lpHidDevice = NULL;

		CloseHandle(m_Writer.hEvent);
		CloseHandle(m_Reader.hEvent);
	}
}


SHORT CSCPController::Scale(SHORT Value)
{
	Value -= 0x80;

	if(Value == -128) Value = -127;

	return (SHORT) (((float) Value / 127.0f) * 32767.0f);
}


BOOL CSCPController::Open(void)
{
	PHID_DEVICE	lpHidDevice = NULL;
	ULONG nDevices = 0;
	DWORD nDevice  = 0;

	FindKnownHidDevices(&lpHidDevice, &nDevices);

	// Look for our Device
	for (ULONG nCurrent = 0; nCurrent < nDevices; nCurrent++)
	{
		_tstring devicePath(lpHidDevice[nCurrent].DevicePath);

		if (!m_bConnected && devicePath.find(m_deviceId) != _tstring::npos)
		{
			if (nDevice == m_dwIndex)
			{
				m_devicePath = _tcsdup(lpHidDevice[nCurrent].DevicePath);

				init_lib_usb();

				if (OpenHidDevice(m_devicePath, true, true, true, false, m_lpHidDevice))
				{
					InitReport();

					m_bConnected = m_xConnected = true;

					_beginthread(ReadThread, 0, this);
				}
			}

			nDevice++;
		}

		CloseHidDevice(&lpHidDevice[nCurrent]);
	}

	if (lpHidDevice != NULL) free(lpHidDevice);

	return m_bConnected;
}

BOOL CSCPController::Close(void)
{
	if (m_bConnected)
	{
		m_bConnected = false;

		if (m_xConnected)
		{
			InitReport(); Sleep(100);

			CloseHidDevice(m_lpHidDevice); 
		}
	}

	return !m_bConnected;
}


DWORD CSCPController::GetState(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	if (m_xConnected)
	{
		m_padState.dwPacketNumber++;

		memcpy(pState, &m_padState, sizeof(XINPUT_STATE));
	}

	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::SetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration)
{
	if (m_xConnected)
	{
		if ((pVibration->wLeftMotorSpeed  != m_padVibration.wLeftMotorSpeed)
		||  (pVibration->wRightMotorSpeed != m_padVibration.wRightMotorSpeed))
		{
			m_padVibration.wRightMotorSpeed = pVibration->wRightMotorSpeed;
			m_padVibration.wLeftMotorSpeed  = pVibration->wLeftMotorSpeed;

			Report();
		}
	}

	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::GetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities)
{
	if (m_xConnected)
	{
		if (dwFlags == XINPUT_FLAG_GAMEPAD || dwFlags == 0)
		{
			memset(pCapabilities, 0, sizeof(XINPUT_CAPABILITIES));

			pCapabilities->Flags   = XINPUT_CAPS_VOICE_SUPPORTED;
			pCapabilities->Type    = XINPUT_DEVTYPE_GAMEPAD;
			pCapabilities->SubType = XINPUT_DEVSUBTYPE_GAMEPAD;

			pCapabilities->Gamepad.wButtons    = 0xF3FF;

			pCapabilities->Gamepad.bLeftTrigger  =
			pCapabilities->Gamepad.bRightTrigger = 0xFF;

			pCapabilities->Gamepad.sThumbLX =
			pCapabilities->Gamepad.sThumbLY =
			pCapabilities->Gamepad.sThumbRX =
			pCapabilities->Gamepad.sThumbRY = (SHORT) 0xFFC0;

			pCapabilities->Vibration.wLeftMotorSpeed  =
			pCapabilities->Vibration.wRightMotorSpeed = 0xFF;

			return ERROR_SUCCESS;
		}
	}
	else
	{
		return ERROR_BAD_ARGUMENTS;
	}

	return ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::GetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid)
{
	if (m_xConnected)
	{
		memset(pDSoundRenderGuid,  0, sizeof(GUID));
		memset(pDSoundCaptureGuid, 0, sizeof(GUID));
	}

	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::GetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation)
{
	if (m_xConnected)
	{
		pBatteryInformation->BatteryType  = BATTERY_TYPE_WIRED;
		pBatteryInformation->BatteryLevel = BATTERY_LEVEL_FULL;
	}

	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::GetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke)
{
	return m_xConnected ? ERROR_EMPTY : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::GetExtended(DWORD dwUserIndex, SCP_EXTN* pPressure)
{
	if (m_xConnected)
	{
		memcpy(pPressure, &m_Extended, sizeof(SCP_EXTN));
	}

	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;;
}


BOOL CSCPController::Reopen(void)
{
	init_lib_usb();

	if (OpenHidDevice(m_devicePath, true, true, true, false, m_lpHidDevice))
	{
		InitReport();

		m_xConnected = true;
	}

	return m_xConnected;
}

BOOL CSCPController::Read(void)
{
	if (m_bConnected)
	{
		if(ReadOverlapped(m_lpHidDevice, &m_Reader))
		{
			if (WaitForSingleObject(m_Reader.hEvent, 250) == WAIT_OBJECT_0)
			{
				UnpackReport(m_lpHidDevice->InputReportBuffer, m_lpHidDevice->Caps.InputReportByteLength, HidP_Input, m_lpHidDevice->InputData, m_lpHidDevice->InputDataLength, m_lpHidDevice->Ppd);

				return true;
			}
			else
			{
				SetEvent(m_Reader.hEvent);
			}
		}
	}

	return false;
}

void CSCPController::ReadThread(void *lpController)
{
	CSCPController* Pad = (CSCPController *) lpController;

	while (Pad->m_bConnected)
	{		
		if (Pad->Read())
		{
			Pad->m_xConnected = true;
			Pad->XInputMapState();
		}
		else
		{
			Pad->m_xConnected = false;
			CloseHidDevice(Pad->m_lpHidDevice); 

			do { Sleep(500); }
			while (Pad->m_bConnected && !Pad->Reopen());
		}
	}

	_endthread();
}

void CSCPController::Report(void)
{
	if (m_bReportEnabled)
	{
		DWORD bytesWritten = 0;

		FormatReport();

		if (WaitForSingleObject(m_Writer.hEvent, 100) == WAIT_OBJECT_0)
		{
			WriteFile(m_lpHidDevice->HidDevice, m_Report, m_dwReportSize, &bytesWritten, &m_Writer);
		}
		else
		{
			SetEvent(m_Writer.hEvent);
		}
	}
}

void CSCPController::InitReport(void)
{		
	memset(&m_padState,     0, sizeof(XINPUT_STATE));
	memset(&m_padVibration, 0, sizeof(XINPUT_VIBRATION));

	Report();
}


// UNDOCUMENTED

DWORD CSCPController::GetStateEx(DWORD dwUserIndex, XINPUT_STATE *pState)
{
	return GetState(m_dwIndex, pState);
}

DWORD CSCPController::WaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid)
{
	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::CancelGuideButtonWait(DWORD dwUserIndex)
{
	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}

DWORD CSCPController::PowerOffController(DWORD dwUserIndex)
{
	return m_xConnected ? ERROR_SUCCESS : ERROR_DEVICE_NOT_CONNECTED;
}
