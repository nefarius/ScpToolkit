#pragma once
#include "targetver.h"

#define STRICT
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <basetyps.h>
#include <tchar.h>
#include <stdlib.h>
#include <wtypes.h>
#include <strsafe.h>
#include <intsafe.h>
#include <process.h>
#include <CGuid.h>
#include <windef.h>
#include <XInput.h>
#include <WinSock2.h>

#pragma warning(push)
#pragma warning(disable : 4995)
#include <string>
#pragma warning(pop)

typedef std::basic_string<TCHAR, std::char_traits<TCHAR>, std::allocator<TCHAR>> _tstring;

#define XINPUT_GAMEPAD_GUIDE 0x400

#include "XInput_SCP.h"
#include "XInput_Wrap.h"
#include "LibUsbApi.h"

extern "C" { 
#include "hid.h" 
}

#include "SCPController.h"
#include "BTConnection.h"
#include "DS2Controller.h"
#include "DS3Controller.h"
#include "SL3Controller.h"
#include "X360Controller.h"
