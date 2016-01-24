// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SCPZADIG_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SCPZADIG_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SCPZADIG_EXPORTS
#define SCPZADIG_API __declspec(dllexport)
#else
#define SCPZADIG_API __declspec(dllimport)
#endif

#include "libwdi\inc\libwdi.h"
#if _WIN64
#pragma comment(lib, "libwdi/lib/amd64/libwdi.lib")
#else
#pragma comment(lib, "libwdi/lib/x86/libwdi.lib")
#endif

extern "C"
{
	SCPZADIG_API wdi_error InstallDeviceDriver(USHORT vid, USHORT pid, LPSTR deviceDescription, LPSTR deviceGuid, LPCSTR driverPath, LPCSTR infName, HWND hWnd, BOOL force, wdi_driver_type driverType);
}
