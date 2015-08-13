#include <ntddk.h>
#include <wmilib.h>
#include <initguid.h>
#include <ntintsafe.h>
#include <wdmsec.h>
#include <wdmguid.h>
#include <usbdrivr.h>
#include "ScpVBus.h"

#define NTSTRSAFE_LIB
#include <ntstrsafe.h>
#include <dontuse.h>

//{6D2B0C28-F583-4B58-99F6-4B3FEF105EDB}
DEFINE_GUID (GUID_SD_BUSENUM_PDO, 0x6d2b0c28, 0xf583, 0x4b58, 0x99, 0xf6, 0x4b, 0x3f, 0xef, 0x10, 0x5e, 0xdb);

//{D61CA365-5AF4-4486-998B-9DB4734C6CA3}
DEFINE_GUID (GUID_DEVCLASS_X360WIRED, 0xD61CA365, 0x5AF4, 0x4486, 0x99, 0x8B, 0x9D, 0xB4, 0x73, 0x4C, 0x6C, 0xA3);

#ifndef BUSENUM_H
#define BUSENUM_H

#define BUSENUM_POOL_TAG (ULONG) 'VpcS'

#define DEVICE_HARDWARE_ID L"USB\\VID_045E&PID_028E\0"
#define DEVICE_HARDWARE_ID_LENGTH sizeof(DEVICE_HARDWARE_ID)

#define BUS_HARDWARE_IDS L"USB\\VID_045E&PID_028E&REV_0114\0USB\\VID_045E&PID_028E\0"
#define BUS_HARDWARE_IDS_LENGTH sizeof(BUS_HARDWARE_IDS)

#define BUSENUM_COMPATIBLE_IDS L"USB\\MS_COMP_XUSB10\0USB\\Class_FF&SubClass_5D&Prot_01\0USB\\Class_FF&SubClass_5D\0USB\\Class_FF\0"
#define BUSENUM_COMPATIBLE_IDS_LENGTH sizeof(BUSENUM_COMPATIBLE_IDS)

#define FILE_DEVICE_BUSENUM		FILE_DEVICE_BUS_EXTENDER

#define PRODUCT    L"Xbox 360 Controller for Windows"
#define VENDORNAME L"SCP "
#define MODEL      L"Virtual X360 Bus : #"

#define DESCRIPTOR_SIZE		0x0099

#if defined(_X86_)
#define CONFIGURATION_SIZE	0x00E4
#else
#define CONFIGURATION_SIZE	0x0130
#endif

#define RUMBLE_SIZE 8
#define LEDSET_SIZE 3

#if DBG

#define DRIVERNAME "ScpVBus.sys: "

#define Bus_KdPrint(_x_) \
               DbgPrint (DRIVERNAME); \
               DbgPrint _x_;

#else

#define Bus_KdPrint(_x_)

#endif

typedef enum _DEVICE_PNP_STATE {

    NotStarted = 0,         // Not started yet
    Started,                // Device has received the START_DEVICE IRP
    StopPending,            // Device has received the QUERY_STOP IRP
    Stopped,                // Device has received the STOP_DEVICE IRP
    RemovePending,          // Device has received the QUERY_REMOVE IRP
    SurpriseRemovePending,  // Device has received the SURPRISE_REMOVE IRP
    Deleted,                // Device has received the REMOVE_DEVICE IRP
    UnKnown                 // Unknown state

} DEVICE_PNP_STATE;

typedef struct _GLOBALS {

    UNICODE_STRING RegistryPath;

} GLOBALS;

extern GLOBALS Globals;

typedef struct _PENDING_IRP {

	LIST_ENTRY	Link;
	PIRP		Irp;

} PENDING_IRP, *PPENDING_IRP;

typedef struct _COMMON_DEVICE_DATA {

    PDEVICE_OBJECT  Self;

    BOOLEAN         IsFDO;

    DEVICE_PNP_STATE DevicePnPState;
    DEVICE_PNP_STATE PreviousPnPState;

    SYSTEM_POWER_STATE  SystemPowerState;
    DEVICE_POWER_STATE  DevicePowerState;

} COMMON_DEVICE_DATA, *PCOMMON_DEVICE_DATA;

typedef struct _PDO_DEVICE_DATA {

    #pragma warning(suppress:4201)
    COMMON_DEVICE_DATA;

    PDEVICE_OBJECT  ParentFdo;

    __drv_aliasesMem
    PWCHAR      HardwareIDs;

    ULONG SerialNo;

    LIST_ENTRY  Link;

    BOOLEAN     Present;
    BOOLEAN     ReportedMissing;
    UCHAR       Reserved[2]; // for 4 byte alignment

    ULONG       InterfaceRefCount;

    LIST_ENTRY  HoldingQueue;
    LIST_ENTRY  PendingQueue;
    KSPIN_LOCK  PendingQueueLock;

	UCHAR		Rumble[8];
	UCHAR		Report[20];

	UNICODE_STRING      InterfaceName;

} PDO_DEVICE_DATA, *PPDO_DEVICE_DATA;

typedef struct _FDO_DEVICE_DATA {

    #pragma warning(suppress:4201)
    COMMON_DEVICE_DATA;

    PDEVICE_OBJECT  UnderlyingPDO;

    PDEVICE_OBJECT  NextLowerDriver;

    LIST_ENTRY      ListOfPDOs;

    ULONG           NumPDOs;

    FAST_MUTEX      Mutex;

    ULONG           OutstandingIO;

    KEVENT          RemoveEvent;
    KEVENT          StopEvent;

    UNICODE_STRING      InterfaceName;

} FDO_DEVICE_DATA, *PFDO_DEVICE_DATA;


#define FDO_FROM_PDO(pdoData) \
          ((PFDO_DEVICE_DATA) (pdoData)->ParentFdo->DeviceExtension)

#define INITIALIZE_PNP_STATE(_Data_)    \
        (_Data_)->DevicePnPState =  NotStarted;\
        (_Data_)->PreviousPnPState = NotStarted;

#define SET_NEW_PNP_STATE(_Data_, _state_) \
        (_Data_)->PreviousPnPState =  (_Data_)->DevicePnPState;\
        (_Data_)->DevicePnPState = (_state_);

#define RESTORE_PREVIOUS_PNP_STATE(_Data_)   \
        (_Data_)->DevicePnPState =   (_Data_)->PreviousPnPState;\

//
// Defined in busenum.c
//

DRIVER_INITIALIZE DriverEntry;
DRIVER_UNLOAD Bus_DriverUnload;

__drv_dispatchType(IRP_MJ_CREATE)
__drv_dispatchType(IRP_MJ_CLOSE)
DRIVER_DISPATCH Bus_CreateClose;

__drv_dispatchType(IRP_MJ_DEVICE_CONTROL)
DRIVER_DISPATCH Bus_IoCtl;

__drv_dispatchType(IRP_MJ_INTERNAL_DEVICE_CONTROL)
DRIVER_DISPATCH Bus_Internal_IoCtl;

DRIVER_CANCEL Bus_CancelIrp;

VOID Bus_IncIoCount(__in PFDO_DEVICE_DATA Data);
VOID Bus_DecIoCount(__in PFDO_DEVICE_DATA Data);

NTSTATUS Bus_ReportDevice(PBUSENUM_REPORT_HARDWARE Report, PFDO_DEVICE_DATA fdoData, PUCHAR pBuffer);

//
// Defined in pnp.c
//

DRIVER_ADD_DEVICE Bus_AddDevice;

__drv_dispatchType(IRP_MJ_PNP)
DRIVER_DISPATCH Bus_PnP;

NTSTATUS Bus_FDO_PnP(__in PDEVICE_OBJECT DeviceObject, __in PIRP Irp, __in PIO_STACK_LOCATION IrpStack, __in PFDO_DEVICE_DATA DeviceData);

NTSTATUS Bus_StartFdo(__in PFDO_DEVICE_DATA FdoData, __in PIRP Irp);

VOID Bus_RemoveFdo(__in PFDO_DEVICE_DATA FdoData);

DRIVER_DISPATCH Bus_SendIrpSynchronously;

IO_COMPLETION_ROUTINE Bus_CompletionRoutine;

NTSTATUS Bus_DestroyPdo(PDEVICE_OBJECT Device, PPDO_DEVICE_DATA PdoData);

VOID Bus_InitializePdo(__drv_in(__drv_aliasesMem) PDEVICE_OBJECT Pdo, PFDO_DEVICE_DATA FdoData);

NTSTATUS Bus_PlugInDevice(PBUSENUM_PLUGIN_HARDWARE PlugIn, ULONG PlugInLength, PFDO_DEVICE_DATA DeviceData);

NTSTATUS Bus_UnPlugDevice(PBUSENUM_UNPLUG_HARDWARE UnPlug, PFDO_DEVICE_DATA DeviceData);

NTSTATUS Bus_EjectDevice(PBUSENUM_EJECT_HARDWARE Eject, PFDO_DEVICE_DATA FdoData);

PCHAR DbgDeviceIDString(BUS_QUERY_ID_TYPE Type);

PCHAR DbgDeviceRelationString(__in DEVICE_RELATION_TYPE Type);

PCHAR PnPMinorFunctionString(UCHAR MinorFunction);

//
// Defined in power.c
//

__drv_dispatchType(IRP_MJ_POWER)
DRIVER_DISPATCH Bus_Power;

NTSTATUS Bus_FDO_Power(PFDO_DEVICE_DATA FdoData, PIRP Irp);

NTSTATUS Bus_PDO_Power(PPDO_DEVICE_DATA PdoData, PIRP Irp);

PCHAR PowerMinorFunctionString(UCHAR MinorFunction);

PCHAR DbgSystemPowerString(__in SYSTEM_POWER_STATE Type);

PCHAR DbgDevicePowerString(__in DEVICE_POWER_STATE Type);

//
// Defined in buspdo.c
//

NTSTATUS Bus_PDO_PnP(__in PDEVICE_OBJECT DeviceObject, __in PIRP Irp, __in PIO_STACK_LOCATION IrpStack, __in PPDO_DEVICE_DATA DeviceData);

NTSTATUS Bus_PDO_QueryDeviceCaps(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryDeviceId(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryDeviceText(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryResources(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryResourceRequirements(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryDeviceRelations(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

NTSTATUS Bus_PDO_QueryBusInformation(__in PPDO_DEVICE_DATA DeviceData,__in PIRP Irp);

NTSTATUS Bus_GetDeviceCapabilities(__in PDEVICE_OBJECT DeviceObject, __out PDEVICE_CAPABILITIES DeviceCapabilities);


VOID Bus_InterfaceReference(__in PVOID Context);

VOID Bus_InterfaceDereference(__in PVOID Context);


BOOLEAN  USB_BUSIFFN Bus_IsDeviceHighSpeed(IN PVOID BusContext);

NTSTATUS USB_BUSIFFN Bus_QueryBusInformation(IN PVOID BusContext, IN ULONG Level, IN OUT PVOID BusInformationBuffer, IN OUT PULONG BusInformationBufferLength, OUT PULONG BusInformationActualLength);

NTSTATUS USB_BUSIFFN Bus_SubmitIsoOutUrb(IN PVOID BusContext, IN PURB Urb);

NTSTATUS USB_BUSIFFN Bus_QueryBusTime(IN PVOID BusContext, IN OUT PULONG CurrentUsbFrame);

VOID     USB_BUSIFFN Bus_GetUSBDIVersion(IN PVOID BusContext, IN OUT PUSBD_VERSION_INFORMATION VersionInformation, IN OUT PULONG HcdCapabilities);


NTSTATUS Bus_PDO_QueryInterface(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp);

#endif
