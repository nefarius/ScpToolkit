#include "busenum.h"

NTSTATUS Bus_Power(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
    PIO_STACK_LOCATION  irpStack;
    NTSTATUS            status;
    PCOMMON_DEVICE_DATA commonData;

    status = STATUS_SUCCESS;
    irpStack = IoGetCurrentIrpStackLocation(Irp);

	ASSERT(irpStack->MajorFunction == IRP_MJ_POWER);

    commonData = (PCOMMON_DEVICE_DATA) DeviceObject->DeviceExtension;

    if (commonData->DevicePnPState == Deleted)
	{
        PoStartNextPowerIrp(Irp);

        Irp->IoStatus.Status = status = STATUS_NO_SUCH_DEVICE ;
        IoCompleteRequest(Irp, IO_NO_INCREMENT);

        return status;
    }

    if (commonData->IsFDO)
	{
        Bus_KdPrint(("FDO %s IRP:0x%p %s %s\n", PowerMinorFunctionString(irpStack->MinorFunction), Irp, DbgSystemPowerString(commonData->SystemPowerState), DbgDevicePowerString(commonData->DevicePowerState)));

        status = Bus_FDO_Power((PFDO_DEVICE_DATA) DeviceObject->DeviceExtension, Irp);
    }
	else
	{
        Bus_KdPrint(("PDO %s IRP:0x%p %s %s\n", PowerMinorFunctionString(irpStack->MinorFunction), Irp, DbgSystemPowerString(commonData->SystemPowerState), DbgDevicePowerString(commonData->DevicePowerState)));

        status = Bus_PDO_Power((PPDO_DEVICE_DATA) DeviceObject->DeviceExtension, Irp);
    }

    return status;
}

NTSTATUS Bus_FDO_Power(PFDO_DEVICE_DATA Data, PIRP Irp)
{
    NTSTATUS            status;
    POWER_STATE         powerState;
    POWER_STATE_TYPE    powerType;
    PIO_STACK_LOCATION  stack;

    stack = IoGetCurrentIrpStackLocation(Irp);

	powerType  = stack->Parameters.Power.Type;
    powerState = stack->Parameters.Power.State;

    Bus_IncIoCount(Data);

    if (Data->DevicePnPState == NotStarted)
	{
        PoStartNextPowerIrp(Irp);

        IoSkipCurrentIrpStackLocation(Irp);
        status = PoCallDriver(Data->NextLowerDriver, Irp);

        Bus_DecIoCount(Data);
        return status;
    }

    if (stack->MinorFunction == IRP_MN_SET_POWER)
	{
        Bus_KdPrint(("\tRequest to set %s state to %s\n", ((powerType == SystemPowerState) ?  "System" : "Device"), ((powerType == SystemPowerState) ?  DbgSystemPowerString(powerState.SystemState) : DbgDevicePowerString(powerState.DeviceState))));
    }

    PoStartNextPowerIrp(Irp);

    IoSkipCurrentIrpStackLocation(Irp);
    status = PoCallDriver(Data->NextLowerDriver, Irp);

    Bus_DecIoCount(Data);
    return status;
}

NTSTATUS Bus_PDO_Power(PPDO_DEVICE_DATA PdoData, PIRP Irp)
{
    NTSTATUS            status;
    PIO_STACK_LOCATION  stack;
    POWER_STATE         powerState;
    POWER_STATE_TYPE    powerType;

    stack = IoGetCurrentIrpStackLocation(Irp);

    powerType  = stack->Parameters.Power.Type;
    powerState = stack->Parameters.Power.State;

    switch (stack->MinorFunction)
	{
    case IRP_MN_SET_POWER:

        Bus_KdPrint(("\tSetting %s power state to %s\n", ((powerType == SystemPowerState) ? "System" : "Device"), ((powerType == SystemPowerState) ? DbgSystemPowerString(powerState.SystemState) : DbgDevicePowerString(powerState.DeviceState))));

        switch (powerType) 
		{
            case DevicePowerState:

                PoSetPowerState(PdoData->Self, powerType, powerState);
                PdoData->DevicePowerState = powerState.DeviceState;

                status = STATUS_SUCCESS;
                break;

            case SystemPowerState:

                PdoData->SystemPowerState = powerState.SystemState;

                status = STATUS_SUCCESS;
                break;

            default:

                status = STATUS_NOT_SUPPORTED;
                break;
        }
        break;

    case IRP_MN_QUERY_POWER:

        status = STATUS_SUCCESS;
        break;

    case IRP_MN_WAIT_WAKE:
    case IRP_MN_POWER_SEQUENCE:
    default:

        status = STATUS_NOT_SUPPORTED;
        break;
    }

    if (status != STATUS_NOT_SUPPORTED)
	{
        Irp->IoStatus.Status = status;
    }

    PoStartNextPowerIrp(Irp);

    status = Irp->IoStatus.Status;
    IoCompleteRequest(Irp, IO_NO_INCREMENT);

    return status;
}

#if DBG

PCHAR PowerMinorFunctionString(UCHAR MinorFunction)
{
    switch (MinorFunction)
    {
        case IRP_MN_SET_POWER:
            return "IRP_MN_SET_POWER";
        case IRP_MN_QUERY_POWER:
            return "IRP_MN_QUERY_POWER";
        case IRP_MN_POWER_SEQUENCE:
            return "IRP_MN_POWER_SEQUENCE";
        case IRP_MN_WAIT_WAKE:
            return "IRP_MN_WAIT_WAKE";
        default:
            return "unknown_power_irp";
    }
}

PCHAR DbgSystemPowerString(__in SYSTEM_POWER_STATE Type)
{
    switch (Type)
    {
        case PowerSystemUnspecified:
            return "PowerSystemUnspecified";
        case PowerSystemWorking:
            return "PowerSystemWorking";
        case PowerSystemSleeping1:
            return "PowerSystemSleeping1";
        case PowerSystemSleeping2:
            return "PowerSystemSleeping2";
        case PowerSystemSleeping3:
            return "PowerSystemSleeping3";
        case PowerSystemHibernate:
            return "PowerSystemHibernate";
        case PowerSystemShutdown:
            return "PowerSystemShutdown";
        case PowerSystemMaximum:
            return "PowerSystemMaximum";
        default:
            return "UnKnown System Power State";
    }
 }

PCHAR DbgDevicePowerString(__in DEVICE_POWER_STATE Type)
{
    switch (Type)
    {
        case PowerDeviceUnspecified:
            return "PowerDeviceUnspecified";
        case PowerDeviceD0:
            return "PowerDeviceD0";
        case PowerDeviceD1:
            return "PowerDeviceD1";
        case PowerDeviceD2:
            return "PowerDeviceD2";
        case PowerDeviceD3:
            return "PowerDeviceD3";
        case PowerDeviceMaximum:
            return "PowerDeviceMaximum";
        default:
            return "UnKnown Device Power State";
    }
}

#endif
