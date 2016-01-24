// ScpZadig.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ScpZadig.h"

#include <Shlwapi.h>
#pragma comment(lib, "Shlwapi")
#pragma comment(lib, "setupapi")

SCPZADIG_API wdi_error InstallWinUsbDriver(LPCSTR deviceId, LPSTR deviceGuid, LPCSTR driverPath, LPCSTR infName, HWND hWnd, BOOL force = false)
{
	// default return value is no matching device found
	auto result = WDI_ERROR_NO_DEVICE;
	// pointer to write device list to
	struct wdi_device_info *device, *list;
	// list all Usb devices, not only driverless ones
	auto listOptions = new wdi_options_create_list();
	listOptions->list_all = true;
	listOptions->list_hubs = false;
	listOptions->trim_whitespaces = false;

	// use WinUSB and overrride device GUID
	auto prepOpts = new wdi_options_prepare_driver();
	prepOpts->driver_type = WDI_WINUSB;
	prepOpts->device_guid = deviceGuid;
	prepOpts->vendor_name = "ScpToolkit compatible device";
	prepOpts->cert_subject = "CN=Nefarius Software Solutions";

	// set parent window handle (may be NULL)
	auto intOpts = new wdi_options_install_driver();
	intOpts->hWnd = hWnd;

	// receive Usb device list
	if (wdi_create_list(&list, listOptions) == WDI_SUCCESS)
	{
		// loop through linked list until last element
		for (device = list; device != NULL; device = device->next)
		{
			
		}
	}

	return WDI_SUCCESS;
}
