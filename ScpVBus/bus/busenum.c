#include "busenum.h"

GLOBALS Globals;
NPAGED_LOOKASIDE_LIST g_LookAside;

#ifdef ALLOC_PRAGMA
#pragma alloc_text(INIT, DriverEntry)
#pragma alloc_text(PAGE, Bus_DriverUnload)
#pragma alloc_text(PAGE, Bus_CreateClose)
#endif

NTSTATUS DriverEntry(__in PDRIVER_OBJECT DriverObject, __in PUNICODE_STRING RegistryPath)
{
    Bus_KdPrint(("Driver Entry\n"));

    ExInitializeNPagedLookasideList(&g_LookAside, NULL, NULL, 0, sizeof(PENDING_IRP), BUSENUM_POOL_TAG, 0);

	Globals.RegistryPath.MaximumLength = RegistryPath->Length + sizeof(UNICODE_NULL);
    Globals.RegistryPath.Length = RegistryPath->Length;
    Globals.RegistryPath.Buffer = ExAllocatePoolWithTag(PagedPool, Globals.RegistryPath.MaximumLength, BUSENUM_POOL_TAG);

    if (!Globals.RegistryPath.Buffer)
	{
		return STATUS_INSUFFICIENT_RESOURCES;
    }

    RtlCopyUnicodeString(&Globals.RegistryPath, RegistryPath);

    DriverObject->MajorFunction [IRP_MJ_CREATE                 ] =
    DriverObject->MajorFunction [IRP_MJ_CLOSE                  ] = Bus_CreateClose;
    DriverObject->MajorFunction [IRP_MJ_PNP                    ] = Bus_PnP;
    DriverObject->MajorFunction [IRP_MJ_POWER                  ] = Bus_Power;
    DriverObject->MajorFunction [IRP_MJ_DEVICE_CONTROL         ] = Bus_IoCtl;
    DriverObject->MajorFunction [IRP_MJ_INTERNAL_DEVICE_CONTROL] = Bus_Internal_IoCtl;

	DriverObject->DriverUnload = Bus_DriverUnload;
    DriverObject->DriverExtension->AddDevice = Bus_AddDevice;

    return STATUS_SUCCESS;
}

VOID Bus_DriverUnload(__in PDRIVER_OBJECT DriverObject)
{
    UNREFERENCED_PARAMETER(DriverObject);
    PAGED_CODE();

    Bus_KdPrint(("Driver Unload\n"));

    ASSERT(NULL == DriverObject->DeviceObject);

    ExDeleteNPagedLookasideList(&g_LookAside);

    if (Globals.RegistryPath.Buffer) ExFreePool(Globals.RegistryPath.Buffer);

    return;
}


NTSTATUS Bus_CreateClose(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
    PIO_STACK_LOCATION  irpStack;
    NTSTATUS            status;
    PFDO_DEVICE_DATA    fdoData;
    PCOMMON_DEVICE_DATA commonData;

    PAGED_CODE();

    commonData = (PCOMMON_DEVICE_DATA) DeviceObject->DeviceExtension;

    if (!commonData->IsFDO)
	{
        Irp->IoStatus.Status = status = STATUS_INVALID_DEVICE_REQUEST;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    fdoData = (PFDO_DEVICE_DATA) DeviceObject->DeviceExtension;

    if (fdoData->DevicePnPState == Deleted)
	{
        Irp->IoStatus.Status = status = STATUS_NO_SUCH_DEVICE;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    Bus_IncIoCount(fdoData);
    irpStack = IoGetCurrentIrpStackLocation(Irp);

    switch (irpStack->MajorFunction)
	{
    case IRP_MJ_CREATE:

        Bus_KdPrint(("Create \n"));
        status = STATUS_SUCCESS;
        break;

    case IRP_MJ_CLOSE:

        Bus_KdPrint(("Close \n"));
        status = STATUS_SUCCESS;
        break;

     default:

        status = STATUS_INVALID_DEVICE_REQUEST;
        break;
    }

    Irp->IoStatus.Information = 0;
    Irp->IoStatus.Status = status;

    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    Bus_DecIoCount(fdoData);

    return status;
}

NTSTATUS Bus_IoCtl(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
    PIO_STACK_LOCATION      irpStack;
    NTSTATUS                status;
    ULONG                   inlen;
    PFDO_DEVICE_DATA        fdoData;
    PVOID                   buffer;
    PCOMMON_DEVICE_DATA     commonData;

    commonData = (PCOMMON_DEVICE_DATA) DeviceObject->DeviceExtension;

	if (!commonData->IsFDO)
	{
        Irp->IoStatus.Status = status = STATUS_INVALID_DEVICE_REQUEST;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    fdoData = (PFDO_DEVICE_DATA) DeviceObject->DeviceExtension;

    if (fdoData->DevicePnPState == Deleted)
	{
        Irp->IoStatus.Status = status = STATUS_NO_SUCH_DEVICE;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    Bus_IncIoCount(fdoData);
    irpStack = IoGetCurrentIrpStackLocation(Irp);

    buffer = Irp->AssociatedIrp.SystemBuffer;
    inlen = irpStack->Parameters.DeviceIoControl.InputBufferLength;

    status = STATUS_INVALID_PARAMETER;
	Irp->IoStatus.Information = 0;

    switch (irpStack->Parameters.DeviceIoControl.IoControlCode)
	{
    case IOCTL_BUSENUM_PLUGIN_HARDWARE:

        if ((sizeof(BUSENUM_PLUGIN_HARDWARE) == inlen) && (((PBUSENUM_PLUGIN_HARDWARE) buffer)->Size == inlen))
		{
            Bus_KdPrint(("PlugIn called\n"));

            status = Bus_PlugInDevice((PBUSENUM_PLUGIN_HARDWARE) buffer, inlen, fdoData);
        }
        break;

    case IOCTL_BUSENUM_UNPLUG_HARDWARE:

        if ((sizeof(BUSENUM_UNPLUG_HARDWARE) == inlen) && (((PBUSENUM_UNPLUG_HARDWARE) buffer)->Size == inlen))
		{
            Bus_KdPrint(("UnPlug called\n"));

            status = Bus_UnPlugDevice((PBUSENUM_UNPLUG_HARDWARE) buffer, fdoData);
        }
        break;

    case IOCTL_BUSENUM_EJECT_HARDWARE:

        if ((sizeof(BUSENUM_EJECT_HARDWARE) == inlen) && (((PBUSENUM_EJECT_HARDWARE) buffer)->Size == inlen))
		{
            Bus_KdPrint(("Eject called\n"));

            status = Bus_EjectDevice((PBUSENUM_EJECT_HARDWARE) buffer, fdoData);
        }
        break;

	case IOCTL_BUSENUM_REPORT_HARDWARE:

        if ((sizeof(BUSENUM_REPORT_HARDWARE) == inlen) && (((PBUSENUM_REPORT_HARDWARE) buffer)->Size == inlen))
		{
            status = Bus_ReportDevice((PBUSENUM_REPORT_HARDWARE) buffer, fdoData, buffer);

			if (NT_SUCCESS(status)) Irp->IoStatus.Information = RUMBLE_SIZE;
        }
		break;

    default:

        break; // default status is STATUS_INVALID_PARAMETER
    }

    Irp->IoStatus.Status = status;

    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    Bus_DecIoCount(fdoData);

    return status;
}

NTSTATUS Bus_Internal_IoCtl(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
    PIO_STACK_LOCATION      irpStack;
    NTSTATUS                status;
    ULONG                   inlen;
    PPDO_DEVICE_DATA        pdoData;
    PVOID                   buffer;
    PCOMMON_DEVICE_DATA     commonData;

    commonData = (PCOMMON_DEVICE_DATA) DeviceObject->DeviceExtension;
    irpStack = IoGetCurrentIrpStackLocation(Irp);

    if (commonData->IsFDO)
	{
        Irp->IoStatus.Status = status = STATUS_INVALID_DEVICE_REQUEST;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    pdoData = (PPDO_DEVICE_DATA) DeviceObject->DeviceExtension;
    
	if (pdoData->Present == FALSE)
	{
        Irp->IoStatus.Status = status = STATUS_DEVICE_NOT_CONNECTED;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

	buffer = Irp->AssociatedIrp.SystemBuffer;
    inlen = irpStack->Parameters.DeviceIoControl.InputBufferLength;

    status = STATUS_INVALID_PARAMETER;

    switch(irpStack->Parameters.DeviceIoControl.IoControlCode)
	{
    case IOCTL_INTERNAL_USB_SUBMIT_URB:
		{
			PURB pHxp = (PURB) irpStack->Parameters.Others.Argument1;

			switch (pHxp->UrbHeader.Function)
			{
			case URB_FUNCTION_CONTROL_TRANSFER :
				{
#if DBG
					struct _URB_CONTROL_TRANSFER* pTransfer = &pHxp->UrbControlTransfer;

					Bus_KdPrint(("URB_FUNCTION_CONTROL_TRANSFER : Function %X\n", pTransfer->Hdr.Function));
					Bus_KdPrint(("URB_FUNCTION_CONTROL_TRANSFER : Handle %p, Flags %X, Length %d\n", pTransfer->PipeHandle, pTransfer->TransferFlags, pTransfer->TransferBufferLength));
					Bus_KdPrint(("URB_FUNCTION_CONTROL_TRANSFER : Setup [%02X %02X %02X %02X %02X %02X %02X %02X]\n", 
						(int) pTransfer->SetupPacket[0], 
						(int) pTransfer->SetupPacket[1], 
						(int) pTransfer->SetupPacket[2], 
						(int) pTransfer->SetupPacket[3], 
						(int) pTransfer->SetupPacket[4], 
						(int) pTransfer->SetupPacket[5], 
						(int) pTransfer->SetupPacket[6], 
						(int) pTransfer->SetupPacket[7]));
#endif
					status = STATUS_UNSUCCESSFUL;
				}
				break;

			case URB_FUNCTION_BULK_OR_INTERRUPT_TRANSFER :
				{
					struct _URB_BULK_OR_INTERRUPT_TRANSFER* pTransfer = &pHxp->UrbBulkOrInterruptTransfer;

					if (pdoData->DevicePnPState != Started)
					{
						status = STATUS_UNSUCCESSFUL;
					}
					else if (pTransfer->TransferFlags & USBD_TRANSFER_DIRECTION_IN)
					{
						KIRQL PrevIrql;
						UCHAR Pipe = pTransfer->PipeHandle == (USBD_PIPE_HANDLE) 0xFFFF0081 ? 0x81 : 0x83;

						if (Pipe == 0x81)
						{
							status = STATUS_PENDING;
							IoMarkIrpPending(Irp);

							KeAcquireSpinLock(&pdoData->PendingQueueLock, &PrevIrql);
							{
							    IoSetCancelRoutine(Irp, Bus_CancelIrp);

							    if (Irp->Cancel == TRUE)
								{
									IoSetCancelRoutine(Irp, NULL);
									KeReleaseSpinLock(&pdoData->PendingQueueLock, PrevIrql);

						            Irp->IoStatus.Status = STATUS_CANCELLED;
									Irp->IoStatus.Information = 0;

									IoCompleteRequest(Irp, IO_NO_INCREMENT);
								}
								else
								{
									PPENDING_IRP le = ExAllocateFromNPagedLookasideList(&g_LookAside);

									le->Irp = Irp; InsertTailList(&pdoData->PendingQueue, &le->Link);
									KeReleaseSpinLock(&pdoData->PendingQueueLock, PrevIrql);
								}
							}
						}
						else
						{
							status = STATUS_PENDING;
							IoMarkIrpPending(Irp);

							KeAcquireSpinLock(&pdoData->PendingQueueLock, &PrevIrql);
							{
							    IoSetCancelRoutine(Irp, Bus_CancelIrp);

							    if (Irp->Cancel == TRUE)
								{
									IoSetCancelRoutine(Irp, NULL);
									KeReleaseSpinLock(&pdoData->PendingQueueLock, PrevIrql);

						            Irp->IoStatus.Status = STATUS_CANCELLED;
									Irp->IoStatus.Information = 0;

									IoCompleteRequest(Irp, IO_NO_INCREMENT);
								}
								else
								{
									PPENDING_IRP le = ExAllocateFromNPagedLookasideList(&g_LookAside);

									le->Irp = Irp; InsertTailList(&pdoData->HoldingQueue, &le->Link);
									KeReleaseSpinLock(&pdoData->PendingQueueLock, PrevIrql);
								}
							}
						}
					}
					else
					{
						Bus_KdPrint(("<< URB_FUNCTION_BULK_OR_INTERRUPT_TRANSFER : Handle %p, Flags %X, Length %d\n", pTransfer->PipeHandle, pTransfer->TransferFlags, pTransfer->TransferBufferLength));

						status = STATUS_SUCCESS;

#if DBG
						if (pTransfer->TransferBufferLength == LEDSET_SIZE) // Led
						{
							UCHAR* Buffer = pTransfer->TransferBuffer;

							Bus_KdPrint(("-- Buffer : %02X %02X %02X", Buffer[0], Buffer[1], Buffer[2]));
						}
#endif

						if (pTransfer->TransferBufferLength == RUMBLE_SIZE) // Rumble
						{
							UCHAR* Buffer = pTransfer->TransferBuffer;
							ULONG  Index;

							Bus_KdPrint(("-- Buffer : %02X %02X %02X %02X %02X %02X %02X %02X", Buffer[0], Buffer[1], Buffer[2], Buffer[3], Buffer[4], Buffer[5], Buffer[6], Buffer[7]));

							for (Index = 0; Index < pTransfer->TransferBufferLength; Index++)
							{
								pdoData->Rumble[Index] = Buffer[Index];
							}
						}
					}
				}
				break;

			case URB_FUNCTION_SELECT_CONFIGURATION :
				{
					PUSBD_INTERFACE_INFORMATION pInfo;

					pInfo = &pHxp->UrbSelectConfiguration.Interface;

					Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : TotalLength %d, Size %d\n", pHxp->UrbHeader.Length, CONFIGURATION_SIZE));

					if (pHxp->UrbHeader.Length == sizeof(struct _URB_SELECT_CONFIGURATION))
					{
						Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : NULL ConfigurationDescriptor\n"));
						status = STATUS_SUCCESS;
						break;
					}

					if (pHxp->UrbHeader.Length < CONFIGURATION_SIZE)
					{
						Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : Invalid ConfigurationDescriptor\n"));
						status = STATUS_INVALID_PARAMETER;
						break;
					}

					status = STATUS_SUCCESS;

					Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : Length %d, Interface %d, Alternate %d, Pipes %d\n", (int) pInfo->Length, (int) pInfo->InterfaceNumber, (int) pInfo->AlternateSetting, pInfo->NumberOfPipes));

					pInfo->Class    = 0xFF;
					pInfo->SubClass = 0x5D;
					pInfo->Protocol = 0x01;

					pInfo->InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;

					pInfo->Pipes[0].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[0].MaximumPacketSize   = 0x20;
					pInfo->Pipes[0].EndpointAddress     = 0x81;
					pInfo->Pipes[0].Interval            = 0x04;
					pInfo->Pipes[0].PipeType            = 0x03;
					pInfo->Pipes[0].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0081;
					pInfo->Pipes[0].PipeFlags           = 0x00;

					pInfo->Pipes[1].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[1].MaximumPacketSize   = 0x20;
					pInfo->Pipes[1].EndpointAddress     = 0x01;
					pInfo->Pipes[1].Interval            = 0x08;
					pInfo->Pipes[1].PipeType            = 0x03;
					pInfo->Pipes[1].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0001;
					pInfo->Pipes[1].PipeFlags           = 0x00;

					pInfo = (PUSBD_INTERFACE_INFORMATION)((PCHAR) pInfo + pInfo->Length);

					Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : Length %d, Interface %d, Alternate %d, Pipes %d\n", (int) pInfo->Length, (int) pInfo->InterfaceNumber, (int) pInfo->AlternateSetting, pInfo->NumberOfPipes));

					pInfo->Class    = 0xFF;
					pInfo->SubClass = 0x5D;
					pInfo->Protocol = 0x03;

					pInfo->InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;

					pInfo->Pipes[0].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[0].MaximumPacketSize   = 0x20;
					pInfo->Pipes[0].EndpointAddress     = 0x82;
					pInfo->Pipes[0].Interval            = 0x04;
					pInfo->Pipes[0].PipeType            = 0x03;
					pInfo->Pipes[0].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0082;
					pInfo->Pipes[0].PipeFlags           = 0x00;

					pInfo->Pipes[1].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[1].MaximumPacketSize   = 0x20;
					pInfo->Pipes[1].EndpointAddress     = 0x02;
					pInfo->Pipes[1].Interval            = 0x08;
					pInfo->Pipes[1].PipeType            = 0x03;
					pInfo->Pipes[1].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0002;
					pInfo->Pipes[1].PipeFlags           = 0x00;

					pInfo->Pipes[2].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[2].MaximumPacketSize   = 0x20;
					pInfo->Pipes[2].EndpointAddress     = 0x83;
					pInfo->Pipes[2].Interval            = 0x08;
					pInfo->Pipes[2].PipeType            = 0x03;
					pInfo->Pipes[2].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0083;
					pInfo->Pipes[2].PipeFlags           = 0x00;

					pInfo->Pipes[3].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[3].MaximumPacketSize   = 0x20;
					pInfo->Pipes[3].EndpointAddress     = 0x03;
					pInfo->Pipes[3].Interval            = 0x08;
					pInfo->Pipes[3].PipeType            = 0x03;
					pInfo->Pipes[3].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0003;
					pInfo->Pipes[3].PipeFlags           = 0x00;

					pInfo = (PUSBD_INTERFACE_INFORMATION)((PCHAR) pInfo + pInfo->Length);

					Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : Length %d, Interface %d, Alternate %d, Pipes %d\n", (int) pInfo->Length, (int) pInfo->InterfaceNumber, (int) pInfo->AlternateSetting, pInfo->NumberOfPipes));

					pInfo->Class    = 0xFF;
					pInfo->SubClass = 0x5D;
					pInfo->Protocol = 0x02;

					pInfo->InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;

					pInfo->Pipes[0].MaximumTransferSize = 0x00400000;
					pInfo->Pipes[0].MaximumPacketSize   = 0x20;
					pInfo->Pipes[0].EndpointAddress     = 0x84;
					pInfo->Pipes[0].Interval            = 0x04;
					pInfo->Pipes[0].PipeType            = 0x03;
					pInfo->Pipes[0].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0084;
					pInfo->Pipes[0].PipeFlags           = 0x00;

					pInfo = (PUSBD_INTERFACE_INFORMATION)((PCHAR) pInfo + pInfo->Length);

					Bus_KdPrint(("URB_FUNCTION_SELECT_CONFIGURATION : Length %d, Interface %d, Alternate %d, Pipes %d\n", (int) pInfo->Length, (int) pInfo->InterfaceNumber, (int) pInfo->AlternateSetting, pInfo->NumberOfPipes));

					pInfo->Class    = 0xFF;
					pInfo->SubClass = 0xFD;
					pInfo->Protocol = 0x13;

					pInfo->InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;
				}
				break;

			case URB_FUNCTION_SELECT_INTERFACE :
				{
					PUSBD_INTERFACE_INFORMATION pInfo = &pHxp->UrbSelectInterface.Interface;

					Bus_KdPrint(("URB_FUNCTION_SELECT_INTERFACE : Length %d, Interface %d, Alternate %d, Pipes %d\n", (int) pInfo->Length, (int) pInfo->InterfaceNumber, (int) pInfo->AlternateSetting, pInfo->NumberOfPipes));
					Bus_KdPrint(("URB_FUNCTION_SELECT_INTERFACE : Class %d, SubClass %d, Protocol %d\n", (int) pInfo->Class, (int) pInfo->SubClass, (int) pInfo->Protocol));

					if (pInfo->InterfaceNumber == 1)
					{
						status = STATUS_SUCCESS;

						pInfo[0].Class         = 0xFF;
						pInfo[0].SubClass      = 0x5D;
						pInfo[0].Protocol      = 0x03;
						pInfo[0].NumberOfPipes = 0x04;

						pInfo[0].InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;

						pInfo[0].Pipes[0].MaximumTransferSize = 0x00400000;
						pInfo[0].Pipes[0].MaximumPacketSize   = 0x20;
						pInfo[0].Pipes[0].EndpointAddress     = 0x82;
						pInfo[0].Pipes[0].Interval            = 0x04;
						pInfo[0].Pipes[0].PipeType            = 0x03;
						pInfo[0].Pipes[0].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0082;
						pInfo[0].Pipes[0].PipeFlags           = 0x00;

						pInfo[0].Pipes[1].MaximumTransferSize = 0x00400000;
						pInfo[0].Pipes[1].MaximumPacketSize   = 0x20;
						pInfo[0].Pipes[1].EndpointAddress     = 0x02;
						pInfo[0].Pipes[1].Interval            = 0x08;
						pInfo[0].Pipes[1].PipeType            = 0x03;
						pInfo[0].Pipes[1].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0002;
						pInfo[0].Pipes[1].PipeFlags           = 0x00;

						pInfo[0].Pipes[2].MaximumTransferSize = 0x00400000;
						pInfo[0].Pipes[2].MaximumPacketSize   = 0x20;
						pInfo[0].Pipes[2].EndpointAddress     = 0x83;
						pInfo[0].Pipes[2].Interval            = 0x08;
						pInfo[0].Pipes[2].PipeType            = 0x03;
						pInfo[0].Pipes[2].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0083;
						pInfo[0].Pipes[2].PipeFlags           = 0x00;

						pInfo[0].Pipes[3].MaximumTransferSize = 0x00400000;
						pInfo[0].Pipes[3].MaximumPacketSize   = 0x20;
						pInfo[0].Pipes[3].EndpointAddress     = 0x03;
						pInfo[0].Pipes[3].Interval            = 0x08;
						pInfo[0].Pipes[3].PipeType            = 0x03;
						pInfo[0].Pipes[3].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0003;
						pInfo[0].Pipes[3].PipeFlags           = 0x00;
					}
					else if (pInfo->InterfaceNumber == 2)
					{
						status = STATUS_SUCCESS;

						pInfo[0].Class         = 0xFF;
						pInfo[0].SubClass      = 0x5D;
						pInfo[0].Protocol      = 0x02;
						pInfo[0].NumberOfPipes = 0x01;

						pInfo[0].InterfaceHandle = (USBD_INTERFACE_HANDLE) 0xFFFF0000;

						pInfo[0].Pipes[0].MaximumTransferSize = 0x00400000;
						pInfo[0].Pipes[0].MaximumPacketSize   = 0x20;
						pInfo[0].Pipes[0].EndpointAddress     = 0x84;
						pInfo[0].Pipes[0].Interval            = 0x04;
						pInfo[0].Pipes[0].PipeType            = 0x03;
						pInfo[0].Pipes[0].PipeHandle          = (USBD_PIPE_HANDLE) 0xFFFF0084;
						pInfo[0].Pipes[0].PipeFlags           = 0x00;
					}
				}
				break;

			case URB_FUNCTION_GET_DESCRIPTOR_FROM_DEVICE :

				Bus_KdPrint(("Descriptor : Type %d, Index %d\n", pHxp->UrbControlDescriptorRequest.DescriptorType, pHxp->UrbControlDescriptorRequest.Index));

				switch (pHxp->UrbControlDescriptorRequest.DescriptorType)
				{
				case USB_DEVICE_DESCRIPTOR_TYPE:

					Bus_KdPrint(("USB_DEVICE_DESCRIPTOR_TYPE : Buffer %p, Length %d\n", pHxp->UrbControlDescriptorRequest.TransferBuffer, pHxp->UrbControlDescriptorRequest.TransferBufferLength));
					{
						PUSB_DEVICE_DESCRIPTOR pDescriptor = (PUSB_DEVICE_DESCRIPTOR) pHxp->UrbControlDescriptorRequest.TransferBuffer;

						status = STATUS_SUCCESS;

						pDescriptor->bLength            = 0x12;
						pDescriptor->bDescriptorType    = USB_DEVICE_DESCRIPTOR_TYPE;
						pDescriptor->bcdUSB             = 0x0200;
						pDescriptor->bDeviceClass       = 0xFF;
						pDescriptor->bDeviceSubClass    = 0xFF;
						pDescriptor->bDeviceProtocol    = 0xFF;
						pDescriptor->bMaxPacketSize0    = 0x08;
						pDescriptor->idVendor           = 0x045E;
						pDescriptor->idProduct          = 0x028E;
						pDescriptor->bcdDevice          = 0x0114;
						pDescriptor->iManufacturer      = 0x01;
						pDescriptor->iProduct           = 0x02;
						pDescriptor->iSerialNumber      = 0x03;
						pDescriptor->bNumConfigurations = 0x01;
					}
					break;

				case USB_CONFIGURATION_DESCRIPTOR_TYPE:

					Bus_KdPrint(("USB_CONFIGURATION_DESCRIPTOR_TYPE : Buffer %p, Length %d\n", pHxp->UrbControlDescriptorRequest.TransferBuffer, pHxp->UrbControlDescriptorRequest.TransferBufferLength));
					{
						UCHAR Descriptor_Data[DESCRIPTOR_SIZE] = 
						{
							0x09, 0x02, 0x99, 0x00, 0x04, 0x01, 0x00, 0xA0, 0xFA, 0x09, 
							0x04, 0x00, 0x00, 0x02, 0xFF, 0x5D, 0x01, 0x00, 0x11, 0x21, 
							0x00, 0x01, 0x01, 0x25, 0x81, 0x14, 0x00, 0x00, 0x00, 0x00, 
							0x13, 0x01, 0x08, 0x00, 0x00, 0x07, 0x05, 0x81, 0x03, 0x20, 
							0x00, 0x04, 0x07, 0x05, 0x01, 0x03, 0x20, 0x00, 0x08, 0x09, 
							0x04, 0x01, 0x00, 0x04, 0xFF, 0x5D, 0x03, 0x00, 0x1B, 0x21, 
							0x00, 0x01, 0x01, 0x01, 0x82, 0x40, 0x01, 0x02, 0x20, 0x16, 
							0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x16, 0x03, 0x00, 
							0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x05, 0x82, 0x03, 0x20, 
							0x00, 0x02, 0x07, 0x05, 0x02, 0x03, 0x20, 0x00, 0x04, 0x07, 
							0x05, 0x83, 0x03, 0x20, 0x00, 0x40, 0x07, 0x05, 0x03, 0x03, 
							0x20, 0x00, 0x10, 0x09, 0x04, 0x02, 0x00, 0x01, 0xFF, 0x5D, 
							0x02, 0x00, 0x09, 0x21, 0x00, 0x01, 0x01, 0x22, 0x84, 0x07, 
							0x00, 0x07, 0x05, 0x84, 0x03, 0x20, 0x00, 0x10, 0x09, 0x04, 
							0x03, 0x00, 0x00, 0xFF, 0xFD, 0x13, 0x04, 0x06, 0x41, 0x00, 
							0x01, 0x01, 0x03
						};

						PUSB_CONFIGURATION_DESCRIPTOR pDescriptor = (PUSB_CONFIGURATION_DESCRIPTOR) pHxp->UrbControlDescriptorRequest.TransferBuffer;

						status = STATUS_SUCCESS;

						pDescriptor->bLength             = 0x09;
						pDescriptor->bDescriptorType     = USB_CONFIGURATION_DESCRIPTOR_TYPE;
						pDescriptor->wTotalLength        = DESCRIPTOR_SIZE;
						pDescriptor->bNumInterfaces      = 0x04;
						pDescriptor->bConfigurationValue = 0x01;
						pDescriptor->iConfiguration      = 0x00;
						pDescriptor->bmAttributes        = 0xA0;
						pDescriptor->MaxPower            = 0xFA;

						if (pHxp->UrbControlDescriptorRequest.TransferBufferLength >= DESCRIPTOR_SIZE)
						{
							UCHAR* Buffer = pHxp->UrbControlDescriptorRequest.TransferBuffer;
							int    Index;

							for (Index = 0; Index < DESCRIPTOR_SIZE; Index++)
							{
								Buffer[Index] = Descriptor_Data[Index];
							}
						}
					}
					break;

				case USB_STRING_DESCRIPTOR_TYPE:

					Bus_KdPrint(("USB_STRING_DESCRIPTOR_TYPE : Index %d, Buffer %p, Length %d\n", pHxp->UrbControlDescriptorRequest.Index, pHxp->UrbControlDescriptorRequest.TransferBuffer, pHxp->UrbControlDescriptorRequest.TransferBufferLength));

					if (pHxp->UrbControlDescriptorRequest.Index == 2)
					{
						PUSB_STRING_DESCRIPTOR pDescriptor = (PUSB_STRING_DESCRIPTOR) pHxp->UrbControlDescriptorRequest.TransferBuffer;
						WCHAR	*bString = L"Controller";
					    USHORT  Length;

						status = STATUS_SUCCESS;

						Length = (USHORT)((wcslen(bString) + 1) * sizeof(WCHAR));

						pDescriptor->bLength         = (UCHAR)(sizeof(USB_STRING_DESCRIPTOR) + Length);
						pDescriptor->bDescriptorType = USB_STRING_DESCRIPTOR_TYPE;

						if (pHxp->UrbControlDescriptorRequest.TransferBufferLength >= pDescriptor->bLength)
						{
				            RtlStringCchPrintfW(pDescriptor->bString, Length / sizeof(WCHAR), L"%ws", bString);
						}
					}
					break;

				case USB_INTERFACE_DESCRIPTOR_TYPE:

					Bus_KdPrint(("USB_INTERFACE_DESCRIPTOR_TYPE : Buffer %p, Length %d\n", pHxp->UrbControlDescriptorRequest.TransferBuffer, pHxp->UrbControlDescriptorRequest.TransferBufferLength));
					break;

				case USB_ENDPOINT_DESCRIPTOR_TYPE:

					Bus_KdPrint(("USB_ENDPOINT_DESCRIPTOR_TYPE : Buffer %p, Length %d\n", pHxp->UrbControlDescriptorRequest.TransferBuffer, pHxp->UrbControlDescriptorRequest.TransferBufferLength));
					break;

				default:

					break;
				}
				break;
			}
		}
	    break;

	case IOCTL_INTERNAL_USB_GET_PORT_STATUS:

		Bus_KdPrint(("IOCTL_INTERNAL_USB_GET_PORT_STATUS\n"));

		*(unsigned long *) irpStack->Parameters.Others.Argument1 = USBD_PORT_ENABLED | USBD_PORT_CONNECTED;

		status = STATUS_SUCCESS;
		break;

	case IOCTL_INTERNAL_USB_RESET_PORT:

		Bus_KdPrint(("IOCTL_INTERNAL_USB_RESET_PORT\n"));

		status = STATUS_SUCCESS;
		break;

	default:

		Bus_KdPrint(("Unknown Ioctrl code\n"));
		break;
    }

    Irp->IoStatus.Information = 0;

    if(status != STATUS_PENDING) 
	{
		Irp->IoStatus.Status = status;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);
    }

    return status;
}


VOID Bus_CancelIrp(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp)
{
    PPDO_DEVICE_DATA pdoData = (PPDO_DEVICE_DATA) DeviceObject->DeviceExtension;
    PLIST_ENTRY le = NULL;
    PIRP cancelIrp = NULL;
    KIRQL irql;

	Bus_KdPrint(("Bus_CancelIrp : %p", Irp));

    IoReleaseCancelSpinLock(Irp->CancelIrql);

    KeAcquireSpinLock(&pdoData->PendingQueueLock, &irql);

    for (le = pdoData->PendingQueue.Flink; le != &pdoData->PendingQueue; le = le->Flink) 
    {
		PPENDING_IRP lr = CONTAINING_RECORD(le, PENDING_IRP, Link);

        cancelIrp = lr->Irp;
        
        if (cancelIrp->Cancel && cancelIrp == Irp)
		{
			Bus_KdPrint(("PendingQueue : %p", cancelIrp));

            RemoveEntryList(le);
            break;
        }

        cancelIrp = NULL;
    }

	if (cancelIrp == NULL)
	{
		for (le = pdoData->HoldingQueue.Flink; le != &pdoData->HoldingQueue; le = le->Flink) 
		{
			PPENDING_IRP lr = CONTAINING_RECORD(le, PENDING_IRP, Link);

			cancelIrp = lr->Irp;
        
	        if (cancelIrp->Cancel && cancelIrp == Irp)
			{
				Bus_KdPrint(("HoldingQueue : %p", cancelIrp));

				RemoveEntryList(le);
				break;
			}

			cancelIrp = NULL;
		}
	}

    KeReleaseSpinLock(&pdoData->PendingQueueLock, irql); 

    if (cancelIrp)
	{
        cancelIrp->IoStatus.Status = STATUS_CANCELLED;
        cancelIrp->IoStatus.Information = 0;

        IoCompleteRequest(cancelIrp, IO_NO_INCREMENT);
    } 
}


VOID Bus_IncIoCount(__in PFDO_DEVICE_DATA FdoData)
{
    LONG result;

    result = InterlockedIncrement((LONG *) &FdoData->OutstandingIO);

    ASSERT(result > 0);

	if (result == 2)
	{
        KeClearEvent(&FdoData->StopEvent);
    }

    return;
}

VOID Bus_DecIoCount(__in PFDO_DEVICE_DATA FdoData)
{

    LONG result;

    result = InterlockedDecrement((LONG *) &FdoData->OutstandingIO);

    ASSERT(result >= 0);

    if (result == 1)
	{
        KeSetEvent(&FdoData->StopEvent, IO_NO_INCREMENT, FALSE);
    }

    if (result == 0)
	{
        ASSERT(FdoData->DevicePnPState == Deleted);

        KeSetEvent(&FdoData->RemoveEvent, IO_NO_INCREMENT, FALSE);
    }

    return;
}


NTSTATUS Bus_ReportDevice(PBUSENUM_REPORT_HARDWARE Report, PFDO_DEVICE_DATA fdoData, PUCHAR Transfer)
{
    PLIST_ENTRY         entry;
    PPDO_DEVICE_DATA    pdoData = NULL;
    BOOLEAN             Found = FALSE;

    UNREFERENCED_PARAMETER(Transfer);

    ExAcquireFastMutex(&fdoData->Mutex);

    if (fdoData->NumPDOs == 0)
	{
        Bus_KdPrint(("No devices to report!\n"));
        ExReleaseFastMutex(&fdoData->Mutex);

        return STATUS_NO_SUCH_DEVICE;
    }

    for (entry = fdoData->ListOfPDOs.Flink; entry != &fdoData->ListOfPDOs && !Found; entry = entry->Flink)
	{
        pdoData = CONTAINING_RECORD(entry, PDO_DEVICE_DATA, Link);

        if (Report->SerialNo == pdoData->SerialNo) Found = pdoData->Present;
    }

    ExReleaseFastMutex(&fdoData->Mutex);

    if (Found)
	{
		int     Index;
		BOOLEAN Changed = FALSE;

		for (Index = 0; Index < REPORT_SIZE && !Changed; Index++)
		{
			if (pdoData->Report[Index] != Report->Data[Index]) Changed = TRUE;
		}

		if (Changed)
		{
			PIRP  PendingIrp = NULL;
			KIRQL PrevIrql;

			KeAcquireSpinLock(&pdoData->PendingQueueLock, &PrevIrql);
			{
				if (!IsListEmpty(&pdoData->PendingQueue))
				{
					PLIST_ENTRY  le = RemoveHeadList(&pdoData->PendingQueue);
					PPENDING_IRP lr = CONTAINING_RECORD(le, PENDING_IRP, Link);

					PendingIrp = lr->Irp;
					ExFreeToNPagedLookasideList(&g_LookAside, le);
				}
			}
			KeReleaseSpinLock(&pdoData->PendingQueueLock, PrevIrql);

			if (PendingIrp != NULL)
			{
				KeRaiseIrql(DISPATCH_LEVEL, &PrevIrql);
				{
					PIO_STACK_LOCATION irpStack = IoGetCurrentIrpStackLocation(PendingIrp);
					PURB pHxp = (PURB) irpStack->Parameters.Others.Argument1;

					PUCHAR Buffer = (PUCHAR) pHxp->UrbBulkOrInterruptTransfer.TransferBuffer;
					pHxp->UrbBulkOrInterruptTransfer.TransferBufferLength = REPORT_SIZE;

					for (Index = 0; Index < REPORT_SIZE; Index++)
					{
						Buffer[Index] = pdoData->Report[Index] = Report->Data[Index];
					}

					PendingIrp->IoStatus.Status = STATUS_SUCCESS;
					IoCompleteRequest(PendingIrp, IO_NO_INCREMENT);
				}
				KeLowerIrql(PrevIrql);
			}
		}

		for (Index = 0; Index < RUMBLE_SIZE; Index++)
		{
			Transfer[Index] = pdoData->Rumble[Index];
			pdoData->Rumble[Index] = 0;
		}

		return STATUS_SUCCESS;
    }

    Bus_KdPrint(("Device %d is not present\n", Report->SerialNo));
    return STATUS_NO_SUCH_DEVICE;
}
