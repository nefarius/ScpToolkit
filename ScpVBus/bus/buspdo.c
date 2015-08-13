#include "busenum.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, Bus_PDO_PnP)
#pragma alloc_text(PAGE, Bus_PDO_QueryDeviceCaps)
#pragma alloc_text(PAGE, Bus_PDO_QueryDeviceId)
#pragma alloc_text(PAGE, Bus_PDO_QueryDeviceText)
#pragma alloc_text(PAGE, Bus_PDO_QueryResources)
#pragma alloc_text(PAGE, Bus_PDO_QueryResourceRequirements)
#pragma alloc_text(PAGE, Bus_PDO_QueryDeviceRelations)
#pragma alloc_text(PAGE, Bus_PDO_QueryBusInformation)
#pragma alloc_text(PAGE, Bus_GetDeviceCapabilities)
#pragma alloc_text(PAGE, Bus_PDO_QueryInterface)
#endif

NTSTATUS Bus_PDO_PnP(__in PDEVICE_OBJECT DeviceObject, __in PIRP Irp, __in PIO_STACK_LOCATION IrpStack, __in PPDO_DEVICE_DATA DeviceData)
{
    NTSTATUS status;

    PAGED_CODE();

    switch (IrpStack->MinorFunction)
	{
    case IRP_MN_START_DEVICE:

        DeviceData->DevicePowerState = PowerDeviceD0;
        SET_NEW_PNP_STATE(DeviceData, Started);

		status = IoRegisterDeviceInterface(DeviceObject, (LPGUID) &GUID_DEVINTERFACE_USB_DEVICE, NULL, &DeviceData->InterfaceName);

		if (NT_SUCCESS(status))
		{
			IoSetDeviceInterfaceState(&DeviceData->InterfaceName, TRUE);
		}

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_STOP_DEVICE:

        SET_NEW_PNP_STATE(DeviceData, Stopped);

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_QUERY_STOP_DEVICE:

        SET_NEW_PNP_STATE(DeviceData, StopPending);

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_CANCEL_STOP_DEVICE:

        if (DeviceData->DevicePnPState == StopPending)
        {
            RESTORE_PREVIOUS_PNP_STATE(DeviceData);
        }

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_QUERY_REMOVE_DEVICE:

        SET_NEW_PNP_STATE(DeviceData, RemovePending);

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_CANCEL_REMOVE_DEVICE:

        if (DeviceData->DevicePnPState == RemovePending)
        {
            RESTORE_PREVIOUS_PNP_STATE(DeviceData);
        }

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_SURPRISE_REMOVAL:

        SET_NEW_PNP_STATE(DeviceData, SurpriseRemovePending);
		IoSetDeviceInterfaceState(&DeviceData->InterfaceName, FALSE);

        if (DeviceData->ReportedMissing)
		{
            PFDO_DEVICE_DATA fdoData;

            SET_NEW_PNP_STATE(DeviceData, Deleted);

            if (DeviceData->ParentFdo)
			{
                fdoData = FDO_FROM_PDO(DeviceData);

                ExAcquireFastMutex(&fdoData->Mutex);
				{
					RemoveEntryList(&DeviceData->Link);
				}
                ExReleaseFastMutex(&fdoData->Mutex);
            }

			status = Bus_DestroyPdo(DeviceObject, DeviceData);
            break;
        }

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_REMOVE_DEVICE:

		IoSetDeviceInterfaceState(&DeviceData->InterfaceName, FALSE);

        if (DeviceData->ReportedMissing)
		{
            PFDO_DEVICE_DATA fdoData;

            SET_NEW_PNP_STATE(DeviceData, Deleted);

            if (DeviceData->ParentFdo)
			{
                fdoData = FDO_FROM_PDO(DeviceData);

                ExAcquireFastMutex(&fdoData->Mutex);
				{
					RemoveEntryList(&DeviceData->Link);
				}
                ExReleaseFastMutex(&fdoData->Mutex);
            }

			status = Bus_DestroyPdo(DeviceObject, DeviceData);
            break;
        }

        if (DeviceData->Present)
		{
            SET_NEW_PNP_STATE(DeviceData, NotStarted);
            status = STATUS_SUCCESS;
        }
		else
		{
            ASSERT(DeviceData->Present);
            status = STATUS_SUCCESS;
        }
        break;

    case IRP_MN_QUERY_CAPABILITIES:

        status = Bus_PDO_QueryDeviceCaps(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_ID:

        Bus_KdPrint(("\tQueryId Type: %s\n", DbgDeviceIDString(IrpStack->Parameters.QueryId.IdType)));

        status = Bus_PDO_QueryDeviceId(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_DEVICE_RELATIONS:

        Bus_KdPrint(("\tQueryDeviceRelation Type: %s\n", DbgDeviceRelationString(IrpStack->Parameters.QueryDeviceRelations.Type)));

        status = Bus_PDO_QueryDeviceRelations(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_DEVICE_TEXT:

        status = Bus_PDO_QueryDeviceText(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_RESOURCES:

        status = Bus_PDO_QueryResources(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_RESOURCE_REQUIREMENTS:

        status = Bus_PDO_QueryResourceRequirements(DeviceData, Irp);
        break;

    case IRP_MN_QUERY_BUS_INFORMATION:

        status = Bus_PDO_QueryBusInformation(DeviceData, Irp);
        break;

    case IRP_MN_DEVICE_USAGE_NOTIFICATION:

        status = STATUS_UNSUCCESSFUL;
        break;

    case IRP_MN_EJECT:

        DeviceData->Present = FALSE;

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_QUERY_INTERFACE:

		status = Bus_PDO_QueryInterface(DeviceData, Irp);
        break;

    case IRP_MN_FILTER_RESOURCE_REQUIREMENTS:

    default:

        status = Irp->IoStatus.Status;
        break;
    }

    Irp->IoStatus.Status = status;
    IoCompleteRequest (Irp, IO_NO_INCREMENT);

    return status;
}

NTSTATUS Bus_PDO_QueryDeviceCaps(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    PIO_STACK_LOCATION      stack;
    PDEVICE_CAPABILITIES    deviceCapabilities;
    DEVICE_CAPABILITIES     parentCapabilities;
    NTSTATUS                status;

    PAGED_CODE();

    stack = IoGetCurrentIrpStackLocation(Irp);
    deviceCapabilities = stack->Parameters.DeviceCapabilities.Capabilities;

    if (deviceCapabilities->Version != 1 || deviceCapabilities->Size < sizeof(DEVICE_CAPABILITIES))
    {
       return STATUS_UNSUCCESSFUL;
    }

    status = Bus_GetDeviceCapabilities(FDO_FROM_PDO(DeviceData)->NextLowerDriver, &parentCapabilities);

    if (!NT_SUCCESS(status))
	{
        Bus_KdPrint(("\tQueryDeviceCaps failed\n"));
        return status;
    }

    RtlCopyMemory(deviceCapabilities->DeviceState, parentCapabilities.DeviceState, (PowerSystemShutdown + 1) * sizeof(DEVICE_POWER_STATE));

    deviceCapabilities->DeviceState[PowerSystemWorking] = PowerDeviceD0;

    if (deviceCapabilities->DeviceState[PowerSystemSleeping1] != PowerDeviceD0)
        deviceCapabilities->DeviceState[PowerSystemSleeping1]  = PowerDeviceD1;

    if (deviceCapabilities->DeviceState[PowerSystemSleeping2] != PowerDeviceD0)
        deviceCapabilities->DeviceState[PowerSystemSleeping2]  = PowerDeviceD3;

    if (deviceCapabilities->DeviceState[PowerSystemSleeping3] != PowerDeviceD0)
        deviceCapabilities->DeviceState[PowerSystemSleeping3]  = PowerDeviceD3;

    deviceCapabilities->DeviceWake = PowerDeviceD1;

    deviceCapabilities->DeviceD1   = TRUE; // Yes we can
    deviceCapabilities->DeviceD2   = FALSE;

    deviceCapabilities->WakeFromD0 = FALSE;
    deviceCapabilities->WakeFromD1 = TRUE; //Yes we can
    deviceCapabilities->WakeFromD2 = FALSE;
    deviceCapabilities->WakeFromD3 = FALSE;

    deviceCapabilities->D1Latency = 0;
    deviceCapabilities->D2Latency = 0;
    deviceCapabilities->D3Latency = 0;

    deviceCapabilities->EjectSupported    = FALSE;
    deviceCapabilities->HardwareDisabled  = FALSE;
    deviceCapabilities->Removable         = TRUE;
	deviceCapabilities->SurpriseRemovalOK = TRUE;
    deviceCapabilities->UniqueID          = TRUE;
    deviceCapabilities->SilentInstall     = FALSE;
    deviceCapabilities->Address           = DeviceData->SerialNo;

    return STATUS_SUCCESS;
}

NTSTATUS Bus_PDO_QueryDeviceId(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    PIO_STACK_LOCATION      stack;
    PWCHAR                  buffer;
    ULONG                   length;
    NTSTATUS                status = STATUS_SUCCESS;
    ULONG_PTR               result;

    PAGED_CODE();

    stack = IoGetCurrentIrpStackLocation(Irp);

    switch (stack->Parameters.QueryId.IdType)
	{
    case BusQueryDeviceID:

        buffer = ExAllocatePoolWithTag(PagedPool, DEVICE_HARDWARE_ID_LENGTH, BUSENUM_POOL_TAG);

        if (!buffer)
		{
           status = STATUS_INSUFFICIENT_RESOURCES;
           break;
        }

        RtlCopyMemory(buffer, DEVICE_HARDWARE_ID, DEVICE_HARDWARE_ID_LENGTH);
        Irp->IoStatus.Information = (ULONG_PTR) buffer;
        break;

    case BusQueryInstanceID:

		length = 11 * sizeof(WCHAR);

        buffer = ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);

        if (!buffer)
		{
           status = STATUS_INSUFFICIENT_RESOURCES;
           break;
        }

        RtlStringCchPrintfW(buffer, length / sizeof(WCHAR), L"%07d", DeviceData->SerialNo);
        Irp->IoStatus.Information = (ULONG_PTR) buffer;

		Bus_KdPrint(("\tInstanceID : %ws\n", buffer));
        break;

    case BusQueryHardwareIDs:

        buffer = DeviceData->HardwareIDs;

        while (*(buffer++)) while (*(buffer++));

        status = RtlULongPtrSub((ULONG_PTR) buffer, (ULONG_PTR) DeviceData->HardwareIDs, &result);

        if (!NT_SUCCESS(status))
		{
           break;
        }

        length = (ULONG) result;

        buffer = ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);

		if (!buffer)
		{
           status = STATUS_INSUFFICIENT_RESOURCES;
           break;
        }

        RtlCopyMemory(buffer, DeviceData->HardwareIDs, length);
        Irp->IoStatus.Information = (ULONG_PTR) buffer;
        break;

    case BusQueryCompatibleIDs:

        length = BUSENUM_COMPATIBLE_IDS_LENGTH;

        buffer = ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);

		if (!buffer)
		{
           status = STATUS_INSUFFICIENT_RESOURCES;
           break;
        }

        RtlCopyMemory(buffer, BUSENUM_COMPATIBLE_IDS, length);
        Irp->IoStatus.Information = (ULONG_PTR) buffer;
        break;

    default:

        status = Irp->IoStatus.Status;
		break;
    }

    return status;
}

NTSTATUS Bus_PDO_QueryDeviceText(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    PWCHAR             buffer;
    USHORT             length;
    PIO_STACK_LOCATION stack;
    NTSTATUS           status;

    PAGED_CODE();

    stack = IoGetCurrentIrpStackLocation(Irp);

    switch (stack->Parameters.QueryDeviceText.DeviceTextType)
	{
    case DeviceTextDescription:

        if (!Irp->IoStatus.Information)
		{
            length = (USHORT)(wcslen(PRODUCT) + 1) * sizeof(WCHAR);
            buffer = ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);

            if (buffer == NULL )
			{
                status = STATUS_INSUFFICIENT_RESOURCES;
                break;
            }

            RtlStringCchPrintfW(buffer, length / sizeof(WCHAR), L"%ws", PRODUCT);
            Bus_KdPrint(("\tDeviceTextDescription : %ws\n", buffer));

            Irp->IoStatus.Information = (ULONG_PTR) buffer;
        }

        status = STATUS_SUCCESS;
        break;

    case DeviceTextLocationInformation:

        if (!Irp->IoStatus.Information) 
		{
            length = (USHORT)(wcslen(VENDORNAME) + 1 + wcslen(MODEL) + 1 + 10) * sizeof(WCHAR);
            buffer = ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);
            
			if (buffer == NULL ) 
			{
                status = STATUS_INSUFFICIENT_RESOURCES;
                break;
            }

            RtlStringCchPrintfW(buffer, length / sizeof(WCHAR), L"%ws%ws%02d", VENDORNAME, MODEL, DeviceData->SerialNo);
            Bus_KdPrint(("\tDeviceTextLocationInformation : %ws\n", buffer));

            Irp->IoStatus.Information = (ULONG_PTR) buffer;
        }

        status = STATUS_SUCCESS;
		break;

    default:

        status = Irp->IoStatus.Status;
        break;
    }

    return status;
}

NTSTATUS Bus_PDO_QueryResources(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    UNREFERENCED_PARAMETER(DeviceData);
    PAGED_CODE();

    return Irp->IoStatus.Status;
}

NTSTATUS Bus_PDO_QueryResourceRequirements(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    NTSTATUS status;

    UNREFERENCED_PARAMETER(DeviceData);
    UNREFERENCED_PARAMETER(Irp);

    PAGED_CODE();

    status = STATUS_SUCCESS;
    return status;
}

NTSTATUS Bus_PDO_QueryDeviceRelations(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    PIO_STACK_LOCATION stack;
    PDEVICE_RELATIONS  deviceRelations;
    NTSTATUS status;

    PAGED_CODE();

    stack = IoGetCurrentIrpStackLocation(Irp);

    switch (stack->Parameters.QueryDeviceRelations.Type)
	{
    case TargetDeviceRelation:

        deviceRelations = (PDEVICE_RELATIONS) Irp->IoStatus.Information;

        if (deviceRelations)
		{
            ASSERTMSG("Someone above is handling TagerDeviceRelation", !deviceRelations);
        }

        deviceRelations = (PDEVICE_RELATIONS) ExAllocatePoolWithTag(PagedPool, sizeof(DEVICE_RELATIONS), BUSENUM_POOL_TAG);
        
		if (!deviceRelations)
		{
                status = STATUS_INSUFFICIENT_RESOURCES;
                break;
        }

        deviceRelations->Count = 1;
        deviceRelations->Objects[0] = DeviceData->Self;
        ObReferenceObject(DeviceData->Self);

        status = STATUS_SUCCESS;
        Irp->IoStatus.Information = (ULONG_PTR) deviceRelations;
        break;

    case BusRelations:      // Not handled by PDO
    case EjectionRelations: // optional for PDO
    case RemovalRelations:  // optional for PDO
    default:

        status = Irp->IoStatus.Status;
		break;
    }

    return status;
}

NTSTATUS Bus_PDO_QueryBusInformation(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
    PPNP_BUS_INFORMATION busInfo;

    UNREFERENCED_PARAMETER(DeviceData);
    PAGED_CODE();

    busInfo = ExAllocatePoolWithTag(PagedPool, sizeof(PNP_BUS_INFORMATION), BUSENUM_POOL_TAG);

    if (busInfo == NULL)
	{
		return STATUS_INSUFFICIENT_RESOURCES;
    }

    busInfo->BusTypeGuid   = GUID_BUS_TYPE_USB;
    busInfo->LegacyBusType = PNPBus;
    busInfo->BusNumber     = 0;

    Irp->IoStatus.Information = (ULONG_PTR) busInfo;
    return STATUS_SUCCESS;
}

NTSTATUS Bus_GetDeviceCapabilities(__in PDEVICE_OBJECT DeviceObject, __out PDEVICE_CAPABILITIES DeviceCapabilities)
{
    IO_STATUS_BLOCK     ioStatus;
    KEVENT              pnpEvent;
    NTSTATUS            status;
    PDEVICE_OBJECT      targetObject;
    PIO_STACK_LOCATION  irpStack;
    PIRP                pnpIrp;

    PAGED_CODE();

    RtlZeroMemory(DeviceCapabilities, sizeof(DEVICE_CAPABILITIES));

    DeviceCapabilities->Size     = sizeof(DEVICE_CAPABILITIES);
    DeviceCapabilities->Version  = 1;
    DeviceCapabilities->Address  = ULONG_MAX;
    DeviceCapabilities->UINumber = ULONG_MAX;

    KeInitializeEvent(&pnpEvent, NotificationEvent, FALSE);

    targetObject = IoGetAttachedDeviceReference(DeviceObject);

    pnpIrp = IoBuildSynchronousFsdRequest(IRP_MJ_PNP, targetObject, NULL, 0, NULL, &pnpEvent, &ioStatus);

    if (pnpIrp == NULL)
	{
        status = STATUS_INSUFFICIENT_RESOURCES;
		ObDereferenceObject(targetObject);

		return status;
    }

    pnpIrp->IoStatus.Status = STATUS_NOT_SUPPORTED;

    irpStack = IoGetNextIrpStackLocation(pnpIrp);

    RtlZeroMemory(irpStack, sizeof(IO_STACK_LOCATION));

    irpStack->MajorFunction = IRP_MJ_PNP;
    irpStack->MinorFunction = IRP_MN_QUERY_CAPABILITIES;
    irpStack->Parameters.DeviceCapabilities.Capabilities = DeviceCapabilities;

    status = IoCallDriver(targetObject, pnpIrp);

    if (status == STATUS_PENDING)
	{
        KeWaitForSingleObject(&pnpEvent, Executive, KernelMode, FALSE, NULL);
        status = ioStatus.Status;
    }

	ObDereferenceObject(targetObject);
    return status;
}


VOID Bus_InterfaceReference(__in PVOID Context)
{
    InterlockedIncrement((LONG *) &((PPDO_DEVICE_DATA) Context)->InterfaceRefCount);
}

VOID Bus_InterfaceDereference(__in PVOID Context)
{
    InterlockedDecrement((LONG *) &((PPDO_DEVICE_DATA) Context)->InterfaceRefCount);
}


BOOLEAN  USB_BUSIFFN Bus_IsDeviceHighSpeed(IN PVOID BusContext)
{
    UNREFERENCED_PARAMETER(BusContext);

	Bus_KdPrint(("Bus_IsDeviceHighSpeed : TRUE\n"));

	return TRUE;
}

NTSTATUS USB_BUSIFFN Bus_QueryBusInformation(IN PVOID BusContext, IN ULONG Level, IN OUT PVOID BusInformationBuffer, IN OUT PULONG BusInformationBufferLength, OUT PULONG BusInformationActualLength)
{
    UNREFERENCED_PARAMETER(BusContext);
    UNREFERENCED_PARAMETER(Level);
    UNREFERENCED_PARAMETER(BusInformationBuffer);
    UNREFERENCED_PARAMETER(BusInformationBufferLength);
    UNREFERENCED_PARAMETER(BusInformationActualLength);

	Bus_KdPrint(("Bus_QueryBusInformation : STATUS_UNSUCCESSFUL\n"));
	return STATUS_UNSUCCESSFUL;
}

NTSTATUS USB_BUSIFFN Bus_SubmitIsoOutUrb(IN PVOID BusContext, IN PURB Urb)
{
    UNREFERENCED_PARAMETER(BusContext);
    UNREFERENCED_PARAMETER(Urb);

	Bus_KdPrint(("Bus_SubmitIsoOutUrb : STATUS_UNSUCCESSFUL\n"));
	return STATUS_UNSUCCESSFUL;
}

NTSTATUS USB_BUSIFFN Bus_QueryBusTime(IN PVOID BusContext, IN OUT PULONG CurrentUsbFrame)
{
    UNREFERENCED_PARAMETER(BusContext);
    UNREFERENCED_PARAMETER(CurrentUsbFrame);

	Bus_KdPrint(("Bus_QueryBusTime : STATUS_UNSUCCESSFUL\n"));
	return STATUS_UNSUCCESSFUL;
}

VOID     USB_BUSIFFN Bus_GetUSBDIVersion(IN PVOID BusContext, IN OUT PUSBD_VERSION_INFORMATION VersionInformation, IN OUT PULONG HcdCapabilities)
{
    UNREFERENCED_PARAMETER(BusContext);

	Bus_KdPrint(("GetUSBDIVersion : 0x500, 0x200\n"));

	if (VersionInformation != NULL)
	{
		VersionInformation->USBDI_Version         = 0x500; /* Usbport */
		VersionInformation->Supported_USB_Version = 0x200; /* USB 2.0 */
	}

	if (HcdCapabilities != NULL)
	{
		*HcdCapabilities = 0;
	}
}


NTSTATUS Bus_PDO_QueryInterface(__in PPDO_DEVICE_DATA DeviceData, __in PIRP Irp)
{
	PIO_STACK_LOCATION irpStack;
	GUID *             interfaceType;
	NTSTATUS           status = STATUS_SUCCESS;

	PAGED_CODE();

	irpStack      = IoGetCurrentIrpStackLocation(Irp);
	interfaceType = (GUID *) irpStack->Parameters.QueryInterface.InterfaceType;

	if (IsEqualGUID(interfaceType, (PVOID) &USB_BUS_INTERFACE_USBDI_GUID))
	{
		PUSB_BUS_INTERFACE_USBDI_V1 pInterface;

		Bus_KdPrint(("USB_BUS_INTERFACE_USBDI_GUID : Version %d Requested\n", (int) irpStack->Parameters.QueryInterface.Version));

		if ((irpStack->Parameters.QueryInterface.Version != USB_BUSIF_USBDI_VERSION_0 && irpStack->Parameters.QueryInterface.Version != USB_BUSIF_USBDI_VERSION_1)
		||  (irpStack->Parameters.QueryInterface.Version == USB_BUSIF_USBDI_VERSION_0 && irpStack->Parameters.QueryInterface.Size < sizeof(USB_BUS_INTERFACE_USBDI_V0))
		||  (irpStack->Parameters.QueryInterface.Version == USB_BUSIF_USBDI_VERSION_1 && irpStack->Parameters.QueryInterface.Size < sizeof(USB_BUS_INTERFACE_USBDI_V1)))
		{
			return STATUS_INVALID_PARAMETER;
		}

		pInterface = (PUSB_BUS_INTERFACE_USBDI_V1) irpStack->Parameters.QueryInterface.Interface;
		pInterface->BusContext = DeviceData;

		pInterface->InterfaceReference   = (PINTERFACE_REFERENCE)   Bus_InterfaceReference;
		pInterface->InterfaceDereference = (PINTERFACE_DEREFERENCE) Bus_InterfaceDereference;

		switch (irpStack->Parameters.QueryInterface.Version)
		{
		case USB_BUSIF_USBDI_VERSION_1:
			pInterface->IsDeviceHighSpeed   = Bus_IsDeviceHighSpeed;
		case USB_BUSIF_USBDI_VERSION_0:
			pInterface->QueryBusInformation = Bus_QueryBusInformation;
			pInterface->SubmitIsoOutUrb     = Bus_SubmitIsoOutUrb;
			pInterface->QueryBusTime        = Bus_QueryBusTime;
			pInterface->GetUSBDIVersion     = Bus_GetUSBDIVersion;
			break;
		}

		Bus_InterfaceReference(DeviceData);
   }
   else
   {
		Bus_KdPrint(("Query unknown interface GUID: %08X-%04X-%04X-%02X%02X%02X%02X%02X%02X%02X%02X - Version %d\n", (int) interfaceType->Data1, (int) interfaceType->Data2, (int) interfaceType->Data3,
				(int) interfaceType->Data4[0], 
				(int) interfaceType->Data4[1], 
				(int) interfaceType->Data4[2], 
				(int) interfaceType->Data4[3], 
				(int) interfaceType->Data4[4], 
				(int) interfaceType->Data4[5], 
				(int) interfaceType->Data4[6], 
				(int) interfaceType->Data4[7],
				(int) irpStack->Parameters.QueryInterface.Version
			));
        status = Irp->IoStatus.Status;
   }

   return status;
}
