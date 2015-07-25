#pragma once

class CDS3Controller : public CSCPController
{
public:

	static const DWORD CollectionSize = 1;

public:

	CDS3Controller(DWORD dwIndex);
	
protected:

	virtual void FormatReport(void);

	virtual void XInputMapState(void);
};
