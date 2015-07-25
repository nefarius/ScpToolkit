#include "StdAfx.h"

BOOLEAN ReadOverlapped(PHID_DEVICE HidDevice, LPOVERLAPPED lpOverlapped)
{
    DWORD	bytesRead;
    BOOL	readStatus;

    readStatus = ReadFile(HidDevice->HidDevice, HidDevice->InputReportBuffer, HidDevice->Caps.InputReportByteLength, &bytesRead, lpOverlapped);
                          
    if (!readStatus) 
    {
        return GetLastError() == ERROR_IO_PENDING;
    }
    else 
    {
        SetEvent(lpOverlapped->hEvent);
        return TRUE;
    }
}

BOOLEAN UnpackReport(__in_bcount(ReportBufferLength) PCHAR ReportBuffer, IN USHORT ReportBufferLength, IN HIDP_REPORT_TYPE ReportType, IN OUT PHID_DATA Data, IN ULONG DataLength, IN PHIDP_PREPARSED_DATA Ppd)
{
    ULONG       numUsages;
    ULONG       i;
    UCHAR       reportID;
    ULONG       Index;
    ULONG       nextUsage;

    reportID = ReportBuffer[0];

    for (i = 0; i < DataLength; i++, Data++) 
    {
        if (reportID == Data->ReportID) 
        {
            if (Data->IsButtonData) 
            {
                numUsages = Data->ButtonData.MaxUsageLength;

                Data->Status = HidP_GetUsages(ReportType, Data->UsagePage, 0, Data->ButtonData.Usages, &numUsages, Ppd, ReportBuffer, ReportBufferLength);

                for (Index = 0, nextUsage = 0; Index < numUsages; Index++) 
                {
                    if (Data->ButtonData.UsageMin <= Data->ButtonData.Usages[Index] && Data->ButtonData.Usages[Index] <= Data->ButtonData.UsageMax) 
                    {
                        Data->ButtonData.Usages[nextUsage++] = Data->ButtonData.Usages[Index];                        
                    }
                }

                if (nextUsage < Data->ButtonData.MaxUsageLength) 
                {
                    Data->ButtonData.Usages[nextUsage] = 0;
                }
            }
            else 
            {
                Data->Status = HidP_GetUsageValue(ReportType, Data->UsagePage, 0, Data->ValueData.Usage, &Data->ValueData.Value, Ppd, ReportBuffer, ReportBufferLength);

                if (HIDP_STATUS_SUCCESS != Data->Status)
                {
                    return FALSE;
                }

                Data->Status = HidP_GetScaledUsageValue(ReportType, Data->UsagePage, 0, Data->ValueData.Usage, &Data->ValueData.ScaledValue, Ppd, ReportBuffer, ReportBufferLength);
            } 

            Data->IsDataSet = TRUE;
        }
    }

    return TRUE;
}
