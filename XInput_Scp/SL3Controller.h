#pragma once

class CSL3Controller : public CSCPController
{
public:

	static const DWORD CollectionSize = 1;

public:

	CSL3Controller(DWORD dwIndex);
		
protected:

	virtual void FormatReport(void);

	virtual void XInputMapState(void);
};

