#pragma once

class CSCPController
{
protected:

	PTCHAR	m_deviceId, m_devicePath;

	volatile BOOL m_bConnected, m_xConnected;

	PHID_DEVICE      m_lpHidDevice;
	XINPUT_STATE     m_padState;
	XINPUT_VIBRATION m_padVibration;

	OVERLAPPED	m_Reader, m_Writer;

	DWORD		m_dwIndex;

	DWORD		m_dwReportSize;
	BYTE*		m_Report;

	BOOL		m_bReportEnabled;

	SCP_EXTN	m_Extended;

protected:

	CSCPController();
	CSCPController(DWORD dwIndex, DWORD dwReportSize);

	static inline float ToPressure(BYTE Value) { return ((int) Value & 0xFF) / 255.0f; }

	static inline float ClampAxis(float Value) { if (Value > 1.0f) return 1.0f; else if (Value < -1.0f) return -1.0f; else return Value; }
	static inline float ToAxis   (SHORT Value) { return ClampAxis(Value / 32767.0f); }

public:

	virtual ~CSCPController(void);


	virtual BOOL Open(void);

	virtual BOOL Close(void);


	virtual DWORD GetState(DWORD dwUserIndex, XINPUT_STATE* pState);

	virtual DWORD SetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);

	virtual DWORD GetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities);

	virtual DWORD GetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid);

	virtual DWORD GetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation);

	virtual DWORD GetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke);

	virtual DWORD GetExtended(DWORD dwUserIndex, SCP_EXTN* pPressure);

	// UNDOCUMENTED

	virtual DWORD GetStateEx(DWORD dwUserIndex, XINPUT_STATE *pState);

	virtual DWORD WaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid);

	virtual DWORD CancelGuideButtonWait(DWORD dwUserIndex);

	virtual DWORD PowerOffController(DWORD dwUserIndex);

protected:

	virtual BOOL Reopen(void);

	virtual BOOL Read(void);

	static void ReadThread(void *lpController);

	virtual void Report(void);

	virtual void InitReport(void);

	virtual SHORT Scale(SHORT Value);

	virtual void FormatReport(void) { };

	virtual void XInputMapState(void) { };
};
