#pragma once

// Play.com USB Adapter only (vid_0b43&pid_0003)

class CDS2Controller : public CSCPController
{
public:

	static const DWORD CollectionSize = 2;

public:

	CDS2Controller(DWORD dwIndex);

	virtual DWORD GetExtended(DWORD dwUserIndex, SCP_EXTN *Pressure);

protected:

	virtual void FormatReport(void);

	virtual void XInputMapState(void);
};
