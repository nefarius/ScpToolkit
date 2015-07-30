#pragma once

#define STRICT
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <commdlg.h>
#include <XInput.h>
#include <tchar.h>
#include <stdio.h>
#include "Resource.h"

#define XINPUT_GAMEPAD_GUIDE 0x400

#include "BugTrap\BugTrap.h"
#include "BugTrap\BTTrace.h"

#ifdef _M_AMD64
#pragma comment(lib, "BugTrap\\BugTrapU-x64.lib")
#else
#pragma comment(lib, "BugTrap\\BugTrapU.lib")
#endif

namespace SCPUser 
{
	static void SetupExceptionHandler()
	{
		BT_SetAppName(_T("XInput Controller Tester"));
		//BT_SetSupportEMail(_T("your@email.com"));
		BT_SetFlags(BTF_DETAILEDMODE);
		//BT_SetSupportServer(_T("localhost"), 9999);
		BT_SetSupportURL(_T("https://github.com/nefarius/ScpServer"));
	}
}