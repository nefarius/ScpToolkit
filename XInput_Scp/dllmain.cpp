// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

INITIALIZE_EASYLOGGINGPP

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	// Load configuration from file
	el::Configurations conf("Xinput_SCP_logger.conf");

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:

		// Reconfigure single logger
		el::Loggers::reconfigureLogger("default", conf);
		// Actually reconfigure all loggers instead
		el::Loggers::reconfigureAllLoggers(conf);
		// Now all the loggers will use configuration from file

		LOG(INFO) << "Loading API...";
		LoadApi(true);
		LOG(INFO) << "API loaded";
		break;

	case DLL_THREAD_ATTACH:
		break;

	case DLL_THREAD_DETACH:
		break;

	case DLL_PROCESS_DETACH:
		LOG(INFO) << "Unloading API...";
		LoadApi(false);
		LOG(INFO) << "API unloaded";
		break;
	}

	return TRUE;
}
