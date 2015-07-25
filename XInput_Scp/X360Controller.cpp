#include "StdAfx.h"
#define REPORT_SIZE	0


CX360Controller::CX360Controller(DWORD dwIndex) : CSCPController(dwIndex, REPORT_SIZE)
{
	m_bReportEnabled = false;
}

BOOL CX360Controller::Open(void)
{
	return WRAP_XInputGetState(m_dwIndex, &m_State) == ERROR_SUCCESS;
}

BOOL CX360Controller::Close(void)
{
	return true;
}

DWORD CX360Controller::GetState(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	return WRAP_XInputGetState(m_dwIndex, pState);
}

DWORD CX360Controller::SetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration)
{
	return WRAP_XInputSetState(m_dwIndex, pVibration);
}

DWORD CX360Controller::GetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities)
{
	return WRAP_XInputGetCapabilities(m_dwIndex, dwFlags, pCapabilities);
}

DWORD CX360Controller::GetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid)
{
	return WRAP_XInputGetDSoundAudioDeviceGuids(m_dwIndex, pDSoundRenderGuid, pDSoundCaptureGuid);
}

DWORD CX360Controller::GetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation)
{
	return WRAP_XInputGetBatteryInformation(m_dwIndex, devType, pBatteryInformation);
}

DWORD CX360Controller::GetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke)
{
	return WRAP_XInputGetKeystroke(m_dwIndex, dwReserved, pKeystroke);
}

DWORD CX360Controller::GetExtended(DWORD dwUserIndex, SCP_EXTN* pPressure)
{
	return WRAP_XInputGetState(m_dwIndex, &m_State) == ERROR_SUCCESS ? ERROR_NOT_SUPPORTED : ERROR_DEVICE_NOT_CONNECTED;
}

// UNDOCUMENTED

DWORD CX360Controller::GetStateEx(DWORD dwUserIndex, XINPUT_STATE *pState)
{
	return WRAP_XInputGetStateEx(m_dwIndex, pState);
}

DWORD CX360Controller::WaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid)
{
	return WRAP_XInputWaitForGuideButton(m_dwIndex, dwFlag, pVoid);
}

DWORD CX360Controller::CancelGuideButtonWait(DWORD dwUserIndex)
{
	return WRAP_XInputCancelGuideButtonWait(m_dwIndex);
}

DWORD CX360Controller::PowerOffController(DWORD dwUserIndex)
{
	return WRAP_XInputPowerOffController(m_dwIndex);
}
