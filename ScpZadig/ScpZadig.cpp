// ScpZadig.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ScpZadig.h"


// This is an example of an exported variable
SCPZADIG_API int nScpZadig=0;

// This is an example of an exported function.
SCPZADIG_API int fnScpZadig(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see ScpZadig.h for the class definition
CScpZadig::CScpZadig()
{
	return;
}
