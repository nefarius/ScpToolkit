// ScpZadig.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ScpZadig.h"

#pragma comment(lib, "setupapi")

SCPZADIG_API wdi_error InstallDeviceDriver(USHORT vid, USHORT pid, LPSTR deviceDescription, LPSTR deviceGuid, LPCSTR driverPath, LPCSTR infName, HWND hWnd, BOOL force, wdi_driver_type driverType)
{
	// default return value is no matching device found
	auto result = WDI_ERROR_NO_DEVICE;
	// pointer to write device list to
	struct wdi_device_info *list, device = {nullptr, vid, pid, FALSE, 0, deviceDescription, nullptr, nullptr, nullptr};
	// list all Usb devices, not only driverless ones
	struct wdi_options_create_list listOptions;
	listOptions.list_all = true;
	listOptions.list_hubs = false;
	listOptions.trim_whitespaces = true;

	// use WinUSB and overrride device GUID
	struct wdi_options_prepare_driver prepOpts;
	prepOpts.driver_type = driverType;
	prepOpts.device_guid = deviceGuid;
	prepOpts.vendor_name = "ScpToolkit compatible device";
	prepOpts.cert_subject = "CN=Nefarius Software Solutions";

	// set parent window handle (may be NULL)
	struct wdi_options_install_driver intOpts;
	intOpts.hWnd = hWnd;

	wdi_log_level(WDI_LOG_LEVEL_DEBUG);

	if ((result = static_cast<wdi_error>(wdi_prepare_driver(&device, driverPath, infName, &prepOpts))) != WDI_SUCCESS)
	{
		return result;
	}

	// receive Usb device list
	if ((result = static_cast<wdi_error>(wdi_create_list(&list, &listOptions))) == WDI_SUCCESS)
	{
		// loop through linked list until last element
		for (; (list != nullptr) && (result == WDI_SUCCESS); list = list->next)
		{
			// is our desired device attached to the system?
			if ((list->vid == device.vid) && (list->pid == device.pid))
			{
				device.hardware_id = list->hardware_id;
				device.device_id = list->device_id;

				result = static_cast<wdi_error>(wdi_install_driver(&device, driverPath, infName, &intOpts));
			}
		}

		// free used memory
		wdi_destroy_list(list);
	}

	return result;
}

