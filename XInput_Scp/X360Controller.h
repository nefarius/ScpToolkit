#pragma once

class CX360Controller : public CSCPController
{
public:

	static const DWORD CollectionSize = 1;

protected:

	XINPUT_STATE m_State;

public:

	CX360Controller(DWORD dwIndex);


	virtual BOOL Open(void);

	virtual BOOL Close(void);


	virtual DWORD GetState(DWORD dwUserIndex, XINPUT_STATE* pState);

	virtual DWORD SetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);

	virtual DWORD GetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities);

	virtual DWORD GetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid);

	virtual DWORD GetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation);

	virtual DWORD GetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke);

	virtual DWORD GetExtended(DWORD dwUserIndex, SCP_EXTN *Pressure);

	// UNDOCUMENTED

	virtual DWORD GetStateEx(DWORD dwUserIndex, XINPUT_STATE *pState);

	virtual DWORD WaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid);

	virtual DWORD CancelGuideButtonWait(DWORD dwUserIndex);

	virtual DWORD PowerOffController(DWORD dwUserIndex);
};

