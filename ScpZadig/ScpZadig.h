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

// This class is exported from the ScpZadig.dll
class SCPZADIG_API CScpZadig {
public:
	CScpZadig(void);
	// TODO: add your methods here.
};

extern SCPZADIG_API int nScpZadig;

SCPZADIG_API int fnScpZadig(void);
