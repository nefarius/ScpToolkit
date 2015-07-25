#include "busenum.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, Bus_AddDevice)
#pragma alloc_text(PAGE, Bus_PnP)
#pragma alloc_text(PAGE, Bus_PlugInDevice)
#pragma alloc_text(PAGE, Bus_InitializePdo)
#pragma alloc_text(PAGE, Bus_UnPlugDevice)
#pragma alloc_text(PAGE, Bus_DestroyPdo)
#pragma alloc_text(PAGE, Bus_RemoveFdo)
#pragma alloc_text(PAGE, Bus_FDO_PnP)
#pragma alloc_text(PAGE, Bus_StartFdo)
#pragma alloc_text(PAGE, Bus_SendIrpSynchronously)
#pragma alloc_text(PAGE, Bus_EjectDevice)
#endif

NTSTATUS Bus_AddDevice(__in PDRIVER_OBJECT DriverObject, __in PDEVICE_OBJECT PhysicalDeviceObject)
{
    NTSTATUS            status;
    PDEVICE_OBJECT      deviceObject = NULL;
    PFDO_DEVICE_DATA    deviceData = NULL;
    PWCHAR              deviceName = NULL;
    ULONG               nameLength;

    UNREFERENCED_PARAMETER(nameLength);
    PAGED_CODE();

    Bus_KdPrint(("Add Device: 0x%p\n", PhysicalDeviceObject));

    status = IoCreateDevice(DriverObject, sizeof(FDO_DEVICE_DATA), NULL, FILE_DEVICE_BUS_EXTENDER, FILE_DEVICE_SECURE_OPEN, TRUE, &deviceObject);

    if (!NT_SUCCESS (status))
    {
        goto End;
    }

    deviceData = (PFDO_DEVICE_DATA) deviceObject->DeviceExtension;
    RtlZeroMemory(deviceData, sizeof (FDO_DEVICE_DATA));

    INITIALIZE_PNP_STATE(deviceData);

    deviceData->IsFDO = TRUE;
    deviceData->Self  = deviceObject;

    ExInitializeFastMutex(&deviceData->Mutex);
    InitializeListHead(&deviceData->ListOfPDOs);

    deviceData->UnderlyingPDO = PhysicalDeviceObject;

    deviceData->DevicePowerState = PowerDeviceUnspecified;
    deviceData->SystemPowerState = PowerSystemWorking;
    deviceData->OutstandingIO = 1;

    KeInitializeEvent(&deviceData->RemoveEvent, SynchronizationEvent, FALSE);
    KeInitializeEvent(&deviceData->StopEvent,   SynchronizationEvent, TRUE);

    deviceObject->Flags |= DO_POWER_PAGABLE;

    status = IoRegisterDeviceInterface(PhysicalDeviceObject, (LPGUID) &GUID_DEVINTERFACE_SCPVBUS, NULL, &deviceData->InterfaceName);

    if (!NT_SUCCESS(status))
	{
        Bus_KdPrint(("AddDevice: IoRegisterDeviceInterface failed (%x)", status));
        goto End;
    }

    deviceData->NextLowerDriver = IoAttachDeviceToDeviceStack(deviceObject, PhysicalDeviceObject);

    if (deviceData->NextLowerDriver == NULL)
	{
        status = STATUS_NO_SUCH_DEVICE;
        goto End;
    }

    deviceObject->Flags &= ~DO_DEVICE_INITIALIZING;

End:

    if (deviceName)
	{
        ExFreePool(deviceName);
    }

    if (!NT_SUCCESS(status) && deviceObject)
	{
        if (deviceData && deviceData->NextLowerDriver)
		{
            IoDetachDevice(deviceData->NextLowerDriver);
        }

        IoDeleteDevice(deviceObject);
    }

    return status;
}

NTSTATUS Bus_PnP(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
    PIO_STACK_LOCATION      irpStack;
    NTSTATUS                status;
    PCOMMON_DEVICE_DATA     commonData;

    PAGED_CODE();

    irpStack = IoGetCurrentIrpStackLocation(Irp);
    ASSERT(IRP_MJ_PNP == irpStack->MajorFunction);

    commonData = (PCOMMON_DEVICE_DATA) DeviceObject->DeviceExtension;

    if (commonData->DevicePnPState == Deleted)
	{
        Irp->IoStatus.Status = status = STATUS_NO_SUCH_DEVICE ;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    if (commonData->IsFDO)
	{
        Bus_KdPrint(("FDO %s IRP:0x%p\n", PnPMinorFunctionString(irpStack->MinorFunction), Irp));

		status = Bus_FDO_PnP(DeviceObject, Irp, irpStack, (PFDO_DEVICE_DATA) commonData);
    }
	else
	{
        Bus_KdPrint(("PDO %s IRP: 0x%p\n", PnPMinorFunctionString(irpStack->MinorFunction), Irp));

		status = Bus_PDO_PnP(DeviceObject, Irp, irpStack, (PPDO_DEVICE_DATA) commonData);
    }

    return status;
}

NTSTATUS Bus_FDO_PnP(__in PDEVICE_OBJECT DeviceObject, __in PIRP Irp, __in PIO_STACK_LOCATION IrpStack, __in PFDO_DEVICE_DATA DeviceData)
{
    NTSTATUS            status;
    ULONG               length, prevcount, numPdosPresent, numPdosMissing;
    PLIST_ENTRY         entry, listHead, nextEntry;
    PPDO_DEVICE_DATA    pdoData;
    PDEVICE_RELATIONS   relations, oldRelations;

    PAGED_CODE();

    Bus_IncIoCount(DeviceData);

    switch (IrpStack->MinorFunction)
	{
    case IRP_MN_START_DEVICE:

        status = Bus_SendIrpSynchronously(DeviceData->NextLowerDriver, Irp);

        if (NT_SUCCESS(status))
		{
            status = Bus_StartFdo (DeviceData, Irp);
        }

        Irp->IoStatus.Status = status;
        IoCompleteRequest (Irp, IO_NO_INCREMENT);

        Bus_DecIoCount(DeviceData);
        return status;

    case IRP_MN_QUERY_STOP_DEVICE:

        SET_NEW_PNP_STATE(DeviceData, StopPending);

		Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_CANCEL_STOP_DEVICE:

        if (StopPending == DeviceData->DevicePnPState)
        {
            RESTORE_PREVIOUS_PNP_STATE(DeviceData);
            ASSERT(DeviceData->DevicePnPState == Started);
        }

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_STOP_DEVICE:

        Bus_DecIoCount(DeviceData);

        KeWaitForSingleObject(&DeviceData->StopEvent, Executive, KernelMode, FALSE, NULL);

        Bus_IncIoCount(DeviceData);

        SET_NEW_PNP_STATE(DeviceData, Stopped);

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_QUERY_REMOVE_DEVICE:

        SET_NEW_PNP_STATE(DeviceData, RemovePending);

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_CANCEL_REMOVE_DEVICE:

        if (DeviceData->DevicePnPState == RemovePending)
        {
            RESTORE_PREVIOUS_PNP_STATE(DeviceData);
        }

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_SURPRISE_REMOVAL:

        SET_NEW_PNP_STATE(DeviceData, SurpriseRemovePending);
        Bus_RemoveFdo(DeviceData);

        ExAcquireFastMutex(&DeviceData->Mutex);
		{
			listHead = &DeviceData->ListOfPDOs;

			for(entry = listHead->Flink,nextEntry = entry->Flink; entry != listHead; entry = nextEntry, nextEntry = entry->Flink)
			{
				pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

				RemoveEntryList(&pdoData->Link);
				InitializeListHead(&pdoData->Link);

				pdoData->ParentFdo       = NULL;
				pdoData->ReportedMissing = TRUE;
			}
		}
        ExReleaseFastMutex(&DeviceData->Mutex);

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    case IRP_MN_REMOVE_DEVICE:

        if (DeviceData->DevicePnPState != SurpriseRemovePending)
        {
            Bus_RemoveFdo(DeviceData);
        }

        SET_NEW_PNP_STATE(DeviceData, Deleted);

        Bus_DecIoCount(DeviceData);
        Bus_DecIoCount(DeviceData);

        KeWaitForSingleObject(&DeviceData->RemoveEvent, Executive, KernelMode, FALSE, NULL);

        ExAcquireFastMutex(&DeviceData->Mutex);
		{
			listHead = &DeviceData->ListOfPDOs;

			for(entry = listHead->Flink,nextEntry = entry->Flink; entry != listHead; entry = nextEntry, nextEntry = entry->Flink)
			{
				pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

				RemoveEntryList(&pdoData->Link);

				if (pdoData->DevicePnPState == SurpriseRemovePending)
				{
					Bus_KdPrint(("\tFound a surprise removed device: 0x%p\n", pdoData->Self));

					InitializeListHead(&pdoData->Link);

					pdoData->ParentFdo       = NULL;
					pdoData->ReportedMissing = TRUE;

					continue;
				}

				Bus_DestroyPdo(pdoData->Self, pdoData);
			}
		}
        ExReleaseFastMutex(&DeviceData->Mutex);

        Irp->IoStatus.Status = STATUS_SUCCESS;
        IoSkipCurrentIrpStackLocation(Irp);

        status = IoCallDriver(DeviceData->NextLowerDriver, Irp);

        IoDetachDevice(DeviceData->NextLowerDriver);

        Bus_KdPrint(("\tDeleting FDO: 0x%p\n", DeviceObject));

        IoDeleteDevice(DeviceObject);
        return status;

    case IRP_MN_QUERY_DEVICE_RELATIONS:

        Bus_KdPrint(("\tQueryDeviceRelation Type: %s\n", DbgDeviceRelationString(IrpStack->Parameters.QueryDeviceRelations.Type)));

        if (IrpStack->Parameters.QueryDeviceRelations.Type != BusRelations)
		{
            break;
        }

        ExAcquireFastMutex(&DeviceData->Mutex);

        oldRelations = (PDEVICE_RELATIONS) Irp->IoStatus.Information;

        if (oldRelations)
		{
            prevcount = oldRelations->Count;

            if (!DeviceData->NumPDOs)
			{
                ExReleaseFastMutex(&DeviceData->Mutex);
                break;
            }
        }
        else
		{
            prevcount = 0;
        }

        numPdosPresent = 0;
		numPdosMissing = 0;

        for (entry = DeviceData->ListOfPDOs.Flink; entry != &DeviceData->ListOfPDOs; entry = entry->Flink)
		{
            pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

            if (pdoData->Present) numPdosPresent++;
        }

        length = sizeof(DEVICE_RELATIONS) + ((numPdosPresent + prevcount) * sizeof (PDEVICE_OBJECT)) - 1;

        relations = (PDEVICE_RELATIONS) ExAllocatePoolWithTag(PagedPool, length, BUSENUM_POOL_TAG);

        if (relations == NULL)
		{
            ExReleaseFastMutex(&DeviceData->Mutex);

            Irp->IoStatus.Status = status = STATUS_INSUFFICIENT_RESOURCES;
            IoCompleteRequest(Irp, IO_NO_INCREMENT);

            Bus_DecIoCount(DeviceData);
            return status;
        }

        if (prevcount)
		{
            RtlCopyMemory(relations->Objects, oldRelations->Objects, prevcount * sizeof (PDEVICE_OBJECT));
        }

        relations->Count = prevcount + numPdosPresent;

        for (entry = DeviceData->ListOfPDOs.Flink; entry != &DeviceData->ListOfPDOs; entry = entry->Flink)
		{
            pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

            if (pdoData->Present)
			{
                relations->Objects[prevcount] = pdoData->Self;
                ObReferenceObject(pdoData->Self);

                prevcount++;
            }
			else
			{
                pdoData->ReportedMissing = TRUE;
				numPdosMissing++;
            }
        }

		Bus_KdPrint(("#PDOS Present = %d, Reported = %d, Missing = %d, Listed = %d", numPdosPresent, relations->Count, numPdosMissing, DeviceData->NumPDOs));

        if (oldRelations)
		{
            ExFreePool(oldRelations);
        }

        Irp->IoStatus.Information = (ULONG_PTR) relations;

        ExReleaseFastMutex(&DeviceData->Mutex);

        Irp->IoStatus.Status = STATUS_SUCCESS;
        break;

    default:

        break;
    }

    IoSkipCurrentIrpStackLocation(Irp);
    status = IoCallDriver(DeviceData->NextLowerDriver, Irp);

    Bus_DecIoCount(DeviceData);
    return status;
}

NTSTATUS Bus_StartFdo(__in PFDO_DEVICE_DATA FdoData, __in PIRP Irp)
{
    NTSTATUS status;
    POWER_STATE powerState;

    UNREFERENCED_PARAMETER(Irp);
    PAGED_CODE();

    status = IoSetDeviceInterfaceState(&FdoData->InterfaceName, TRUE);

    if (!NT_SUCCESS (status))
	{
        Bus_KdPrint(("IoSetDeviceInterfaceState failed: 0x%x\n", status));
        return status;
    }

    FdoData->DevicePowerState = PowerDeviceD0;
    powerState.DeviceState    = PowerDeviceD0;

	PoSetPowerState(FdoData->Self, DevicePowerState, powerState);

    SET_NEW_PNP_STATE(FdoData, Started);

    return status;
}

VOID Bus_RemoveFdo(__in PFDO_DEVICE_DATA FdoData)
{
    PAGED_CODE();

    if (FdoData->InterfaceName.Buffer != NULL)
	{
        IoSetDeviceInterfaceState(&FdoData->InterfaceName, FALSE);

        ExFreePool(FdoData->InterfaceName.Buffer);
        RtlZeroMemory(&FdoData->InterfaceName, sizeof(UNICODE_STRING));
    }
}

NTSTATUS Bus_SendIrpSynchronously(__in PDEVICE_OBJECT DeviceObject, __in PIRP Irp)
{
    KEVENT   event;
    NTSTATUS status;

    PAGED_CODE();

    KeInitializeEvent(&event, NotificationEvent, FALSE);
    IoCopyCurrentIrpStackLocationToNext(Irp);

    IoSetCompletionRoutine(Irp, Bus_CompletionRoutine, &event, TRUE, TRUE, TRUE);
    status = IoCallDriver(DeviceObject, Irp);

    if (status == STATUS_PENDING)
	{
       KeWaitForSingleObject(&event, Executive, KernelMode, FALSE, NULL);
       status = Irp->IoStatus.Status;
    }

    return status;
}

NTSTATUS Bus_CompletionRoutine(PDEVICE_OBJECT DeviceObject, PIRP Irp, PVOID Context)
{
    UNREFERENCED_PARAMETER(DeviceObject);

    if (Irp->PendingReturned == TRUE)
	{
        KeSetEvent((PKEVENT) Context, IO_NO_INCREMENT, FALSE);
    }

    return STATUS_MORE_PROCESSING_REQUIRED;
}

NTSTATUS Bus_DestroyPdo(PDEVICE_OBJECT Device, PPDO_DEVICE_DATA PdoData)
{
    PFDO_DEVICE_DATA fdoData;
    PAGED_CODE();

	fdoData = FDO_FROM_PDO(PdoData);
	fdoData->NumPDOs--;

    if (PdoData->InterfaceName.Buffer != NULL)
	{
        ExFreePool(PdoData->InterfaceName.Buffer);
        RtlZeroMemory(&PdoData->InterfaceName, sizeof(UNICODE_STRING));
    }

    if (PdoData->HardwareIDs)
	{
        ExFreePool(PdoData->HardwareIDs);
        PdoData->HardwareIDs = NULL;
    }

    Bus_KdPrint(("\tDeleting PDO: 0x%p\n", Device));
    IoDeleteDevice(Device);

    return STATUS_SUCCESS;
}

VOID Bus_InitializePdo(__drv_in(__drv_aliasesMem) PDEVICE_OBJECT Pdo, PFDO_DEVICE_DATA FdoData)
{
    PPDO_DEVICE_DATA pdoData;

    PAGED_CODE();

    pdoData = (PPDO_DEVICE_DATA)  Pdo->DeviceExtension;

    Bus_KdPrint(("PDO 0x%p, Extension 0x%p\n", Pdo, pdoData));

    pdoData->IsFDO      = FALSE;
    pdoData->Self       = Pdo;

    pdoData->ParentFdo = FdoData->Self;

    pdoData->Present = TRUE;
    pdoData->ReportedMissing = FALSE;

    INITIALIZE_PNP_STATE(pdoData);

    pdoData->DevicePowerState = PowerDeviceD3;
    pdoData->SystemPowerState = PowerSystemWorking;

    Pdo->Flags |= DO_POWER_PAGABLE;

	KeInitializeSpinLock(&pdoData->PendingQueueLock);
	InitializeListHead  (&pdoData->PendingQueue);
	InitializeListHead  (&pdoData->HoldingQueue);

	ExAcquireFastMutex(&FdoData->Mutex);
	{
		InsertTailList(&FdoData->ListOfPDOs, &pdoData->Link);
		FdoData->NumPDOs++;
	}
    ExReleaseFastMutex(&FdoData->Mutex);

    Pdo->Flags &= ~DO_DEVICE_INITIALIZING;
}


NTSTATUS Bus_PlugInDevice(PBUSENUM_PLUGIN_HARDWARE PlugIn, ULONG PlugInSize, PFDO_DEVICE_DATA FdoData)
{
    PDEVICE_OBJECT      pdo;
    PPDO_DEVICE_DATA    pdoData;
    NTSTATUS            status;
    ULONG               length;
    BOOLEAN             unique;
    PLIST_ENTRY         entry;

    PAGED_CODE();

    length = (PlugInSize - sizeof(BUSENUM_PLUGIN_HARDWARE)) / sizeof(WCHAR);

    Bus_KdPrint(("Exposing PDO\n" "====== Serial : %d\n" "====== Device : %ws\n" "====== Length : %d\n", PlugIn->SerialNo, BUS_HARDWARE_IDS, BUS_HARDWARE_IDS_LENGTH));

    unique = TRUE;

    ExAcquireFastMutex(&FdoData->Mutex);
	{
		for (entry = FdoData->ListOfPDOs.Flink; entry != &FdoData->ListOfPDOs; entry = entry->Flink)
		{
			pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

			if (PlugIn->SerialNo == pdoData->SerialNo && pdoData->DevicePnPState != SurpriseRemovePending)
			{
				unique = FALSE;
				break;
			}
		}
	}
    ExReleaseFastMutex(&FdoData->Mutex);

    if (!unique || PlugIn->SerialNo == 0) return STATUS_INVALID_PARAMETER;

    length *= sizeof(WCHAR);

    Bus_KdPrint(("FdoData->NextLowerDriver = 0x%p\n", FdoData->NextLowerDriver));

    status = IoCreateDeviceSecure(FdoData->Self->DriverObject, sizeof(PDO_DEVICE_DATA), NULL, FILE_DEVICE_BUS_EXTENDER, FILE_AUTOGENERATED_DEVICE_NAME | FILE_DEVICE_SECURE_OPEN, FALSE, &SDDL_DEVOBJ_SYS_ALL_ADM_RWX_WORLD_RWX_RES_RWX, (LPCGUID) &GUID_DEVCLASS_X360WIRED, &pdo);

	if (!NT_SUCCESS(status))
	{
        return status;
    }

    pdoData = (PPDO_DEVICE_DATA) pdo->DeviceExtension;
    pdoData->HardwareIDs = ExAllocatePoolWithTag(NonPagedPool, BUS_HARDWARE_IDS_LENGTH, BUSENUM_POOL_TAG);

    if (pdoData->HardwareIDs == NULL)
	{
        IoDeleteDevice(pdo);
        return STATUS_INSUFFICIENT_RESOURCES;
    }

    RtlCopyMemory(pdoData->HardwareIDs, BUS_HARDWARE_IDS, BUS_HARDWARE_IDS_LENGTH);

    pdoData->SerialNo = PlugIn->SerialNo;
    Bus_InitializePdo(pdo, FdoData);

    IoInvalidateDeviceRelations(FdoData->UnderlyingPDO, BusRelations);
    return status;
}

NTSTATUS Bus_UnPlugDevice(PBUSENUM_UNPLUG_HARDWARE UnPlug, PFDO_DEVICE_DATA FdoData)
{
    PLIST_ENTRY         entry;
    PPDO_DEVICE_DATA    pdoData;
    BOOLEAN             found = FALSE, plugOutAll;

    PAGED_CODE();

    plugOutAll = (UnPlug->SerialNo == 0);

    ExAcquireFastMutex(&FdoData->Mutex);

    if (plugOutAll)
	{
        Bus_KdPrint(("Plugging out all the devices!\n"));
    }
	else
	{
        Bus_KdPrint(("Plugging out %d\n", UnPlug->SerialNo));
    }

    if (FdoData->NumPDOs == 0)
	{
        ExReleaseFastMutex(&FdoData->Mutex);

        return STATUS_NO_SUCH_DEVICE;
    }

    for (entry = FdoData->ListOfPDOs.Flink; entry != &FdoData->ListOfPDOs; entry = entry->Flink)
	{
        pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

        Bus_KdPrint(("Found device %d\n", pdoData->SerialNo));

        if (plugOutAll || UnPlug->SerialNo == pdoData->SerialNo)
		{
            Bus_KdPrint(("Plugged out %d\n", pdoData->SerialNo));

            pdoData->Present = FALSE;
            found = TRUE;

            if (!plugOutAll) break;
        }
    }

    ExReleaseFastMutex(&FdoData->Mutex);

    if (found)
	{
        IoInvalidateDeviceRelations(FdoData->UnderlyingPDO, BusRelations);
        return STATUS_SUCCESS;
    }

    Bus_KdPrint(("Device %d is not present\n", UnPlug->SerialNo));
    return STATUS_INVALID_PARAMETER;
}

NTSTATUS Bus_EjectDevice(PBUSENUM_EJECT_HARDWARE Eject, PFDO_DEVICE_DATA FdoData)
{
    PLIST_ENTRY         entry;
    PPDO_DEVICE_DATA    pdoData;
    BOOLEAN             found = FALSE, ejectAll;

    PAGED_CODE();

    ejectAll = (Eject->SerialNo == 0);

    ExAcquireFastMutex(&FdoData->Mutex);

    if (ejectAll)
	{
        Bus_KdPrint(("Ejecting all the pdos!\n"));
    }
	else
	{
        Bus_KdPrint(("Ejecting %d\n", Eject->SerialNo));
    }

    if (FdoData->NumPDOs == 0)
	{
        Bus_KdPrint(("No devices to eject!\n"));
        ExReleaseFastMutex(&FdoData->Mutex);

        return STATUS_NO_SUCH_DEVICE;
    }

    for (entry = FdoData->ListOfPDOs.Flink; entry != &FdoData->ListOfPDOs; entry = entry->Flink)
	{
        pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

        Bus_KdPrint(("Found device %d\n", pdoData->SerialNo));

        if (ejectAll || Eject->SerialNo == pdoData->SerialNo)
		{
            Bus_KdPrint(("Ejected %d\n", pdoData->SerialNo));
            found = TRUE;

            IoRequestDeviceEject(pdoData->Self);

            if (!ejectAll) break;
        }
    }

    ExReleaseFastMutex(&FdoData->Mutex);

    if (found)
	{
        return STATUS_SUCCESS;
    }

    Bus_KdPrint(("Device %d is not present\n", Eject->SerialNo));
    return STATUS_INVALID_PARAMETER;
}

#if DBG

PCHAR PnPMinorFunctionString(UCHAR MinorFunction)
{
    switch (MinorFunction)
    {
        case IRP_MN_START_DEVICE:
            return "IRP_MN_START_DEVICE";
        case IRP_MN_QUERY_REMOVE_DEVICE:
            return "IRP_MN_QUERY_REMOVE_DEVICE";
        case IRP_MN_REMOVE_DEVICE:
            return "IRP_MN_REMOVE_DEVICE";
        case IRP_MN_CANCEL_REMOVE_DEVICE:
            return "IRP_MN_CANCEL_REMOVE_DEVICE";
        case IRP_MN_STOP_DEVICE:
            return "IRP_MN_STOP_DEVICE";
        case IRP_MN_QUERY_STOP_DEVICE:
            return "IRP_MN_QUERY_STOP_DEVICE";
        case IRP_MN_CANCEL_STOP_DEVICE:
            return "IRP_MN_CANCEL_STOP_DEVICE";
        case IRP_MN_QUERY_DEVICE_RELATIONS:
            return "IRP_MN_QUERY_DEVICE_RELATIONS";
        case IRP_MN_QUERY_INTERFACE:
            return "IRP_MN_QUERY_INTERFACE";
        case IRP_MN_QUERY_CAPABILITIES:
            return "IRP_MN_QUERY_CAPABILITIES";
        case IRP_MN_QUERY_RESOURCES:
            return "IRP_MN_QUERY_RESOURCES";
        case IRP_MN_QUERY_RESOURCE_REQUIREMENTS:
            return "IRP_MN_QUERY_RESOURCE_REQUIREMENTS";
        case IRP_MN_QUERY_DEVICE_TEXT:
            return "IRP_MN_QUERY_DEVICE_TEXT";
        case IRP_MN_FILTER_RESOURCE_REQUIREMENTS:
            return "IRP_MN_FILTER_RESOURCE_REQUIREMENTS";
        case IRP_MN_READ_CONFIG:
            return "IRP_MN_READ_CONFIG";
        case IRP_MN_WRITE_CONFIG:
            return "IRP_MN_WRITE_CONFIG";
        case IRP_MN_EJECT:
            return "IRP_MN_EJECT";
        case IRP_MN_SET_LOCK:
            return "IRP_MN_SET_LOCK";
        case IRP_MN_QUERY_ID:
            return "IRP_MN_QUERY_ID";
        case IRP_MN_QUERY_PNP_DEVICE_STATE:
            return "IRP_MN_QUERY_PNP_DEVICE_STATE";
        case IRP_MN_QUERY_BUS_INFORMATION:
            return "IRP_MN_QUERY_BUS_INFORMATION";
        case IRP_MN_DEVICE_USAGE_NOTIFICATION:
            return "IRP_MN_DEVICE_USAGE_NOTIFICATION";
        case IRP_MN_SURPRISE_REMOVAL:
            return "IRP_MN_SURPRISE_REMOVAL";
        case IRP_MN_QUERY_LEGACY_BUS_INFORMATION:
            return "IRP_MN_QUERY_LEGACY_BUS_INFORMATION";
        default:
            return "unknown_pnp_irp";
    }
}

PCHAR DbgDeviceRelationString(__in DEVICE_RELATION_TYPE Type)
{
    switch (Type)
    {
        case BusRelations:
            return "BusRelations";
        case EjectionRelations:
            return "EjectionRelations";
        case RemovalRelations:
            return "RemovalRelations";
        case TargetDeviceRelation:
            return "TargetDeviceRelation";
        default:
            return "UnKnown Relation";
    }
}

PCHAR DbgDeviceIDString(BUS_QUERY_ID_TYPE Type)
{
    switch (Type)
    {
        case BusQueryDeviceID:
            return "BusQueryDeviceID";
        case BusQueryHardwareIDs:
            return "BusQueryHardwareIDs";
        case BusQueryCompatibleIDs:
            return "BusQueryCompatibleIDs";
        case BusQueryInstanceID:
            return "BusQueryInstanceID";
        case BusQueryDeviceSerialNumber:
            return "BusQueryDeviceSerialNumber";
		case BusQueryContainerID:
			return "BusQueryContainerID";
        default:
            return "UnKnown ID";
    }
}

#endif
