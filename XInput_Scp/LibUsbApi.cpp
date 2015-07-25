#include "StdAfx.h"
#include "lusb0_usb.h"

static HMODULE l_hLibUsbDll = NULL;
static FARPROC l_hLibUsbFunc[] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL }; 

static unsigned short idVendor  = 0x054c;
static unsigned short idProduct = 0x0268;

#define DS3_REQUEST_CODE	0x09
#define DS3_REQUEST_VALUE	0x03F4
#define DS3_REQUEST_SIZE	0x04

static CHAR l_hwStartData[DS3_REQUEST_SIZE] = { 0x42, 0x0C, 0x00, 0x00 };

static volatile bool bInited = false;

typedef void (__cdecl *usb_initFunction)(void);
typedef int  (__cdecl *usb_find_bussesFunction)(void);
typedef int  (__cdecl *usb_find_devicesFunction)(void);
typedef struct usb_bus *(__cdecl *usb_get_bussesFunction)(void);
typedef usb_dev_handle *(__cdecl *usb_openFunction)(struct usb_device *dev);
typedef int  (__cdecl *usb_control_msgFunction)(usb_dev_handle *dev, int requesttype, int request, int value, int index, char *bytes, int size, int timeout);
typedef int  (__cdecl *usb_closeFunction)(usb_dev_handle *dev);

void load_lib_usb()
{
	if (!bInited) 
	{
		if ((l_hLibUsbDll = LoadLibrary(_T("C:\\Windows\\System32\\libusb0.dll"))) != NULL)
		{
			if ((l_hLibUsbFunc[0] = GetProcAddress(l_hLibUsbDll, "usb_init"))
			 && (l_hLibUsbFunc[1] = GetProcAddress(l_hLibUsbDll, "usb_find_busses"))
			 && (l_hLibUsbFunc[2] = GetProcAddress(l_hLibUsbDll, "usb_find_devices"))
			 && (l_hLibUsbFunc[3] = GetProcAddress(l_hLibUsbDll, "usb_get_busses"))
			 && (l_hLibUsbFunc[4] = GetProcAddress(l_hLibUsbDll, "usb_open"))
			 && (l_hLibUsbFunc[5] = GetProcAddress(l_hLibUsbDll, "usb_control_msg"))
			 && (l_hLibUsbFunc[6] = GetProcAddress(l_hLibUsbDll, "usb_close")))
			{
				((usb_initFunction) l_hLibUsbFunc[0])();
				bInited = true; 
			}
		}
	}
}

void init_lib_usb()
{
	bool bFound = false;

	if (bInited)
	{
	    struct usb_bus* bus;
		struct usb_device* dev;
		struct usb_dev_handle* udev;

		((usb_find_bussesFunction)  l_hLibUsbFunc[1])();
		((usb_find_devicesFunction) l_hLibUsbFunc[2]());

		for (bus = ((usb_get_bussesFunction) l_hLibUsbFunc[3])(); bus; bus = bus->next)
		{
			for (dev = bus->devices; dev; dev = dev->next)
			{
				if (dev->descriptor.idVendor == idVendor && dev->descriptor.idProduct == idProduct)
				{
					if ((udev = ((usb_openFunction) l_hLibUsbFunc[4])(dev)))
					{
						((usb_control_msgFunction) l_hLibUsbFunc[5])(udev, 
							USB_ENDPOINT_OUT | USB_TYPE_CLASS | USB_RECIP_INTERFACE, DS3_REQUEST_CODE, 
							DS3_REQUEST_VALUE, dev->config->interface->altsetting->bInterfaceNumber, 
							l_hwStartData, DS3_REQUEST_SIZE, 500);

						((usb_closeFunction) l_hLibUsbFunc[6])(udev); bFound = true;
					}
				}
			}
		}
	}

	if (bFound) Sleep(100);
}
