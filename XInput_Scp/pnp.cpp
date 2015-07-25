#include "StdAfx.h"

BOOLEAN FindKnownHidDevices(OUT PHID_DEVICE* HidDevices, OUT PULONG NumberDevices)
{
    HDEVINFO                            hardwareDeviceInfo;
    SP_DEVICE_INTERFACE_DATA            deviceInfoData;
    ULONG                               i = 0;
    BOOLEAN                             done;
    PHID_DEVICE                         hidDeviceInst;
    GUID                                hidGuid;
    PSP_DEVICE_INTERFACE_DETAIL_DATA    functionClassDeviceData = NULL;
    ULONG                               predictedLength = 0;
    ULONG                               requiredLength = 0;
    PHID_DEVICE                         newHidDevices;

    HidD_GetHidGuid(&hidGuid);

    *HidDevices = NULL;
    *NumberDevices = 0;

    //
    // Open a handle to the plug and play dev node.
    //
    hardwareDeviceInfo = SetupDiGetClassDevs(&hidGuid,
                                             NULL, // Define no enumerator (global)
                                             NULL, // Define no
                                             (DIGCF_PRESENT | // Only Devices present
                                              DIGCF_DEVICEINTERFACE)); // Function class devices.

    if (hardwareDeviceInfo == INVALID_HANDLE_VALUE)
    {
        return FALSE;
    }

    //
    // Take a wild guess to start
    //    
    *NumberDevices = 4;
    done = FALSE;
    deviceInfoData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);

    while (!done) 
    {
        *NumberDevices *= 2;

        if (*HidDevices) 
        {
            newHidDevices = (PHID_DEVICE) realloc(*HidDevices, (*NumberDevices * sizeof(HID_DEVICE)));

            if (newHidDevices == NULL)
            {
                free(*HidDevices);
            }

            *HidDevices = newHidDevices;
        }
        else
        {
            *HidDevices = (PHID_DEVICE) calloc(*NumberDevices, sizeof(HID_DEVICE));
        }

        if (*HidDevices == NULL)
        {
            SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
            return FALSE;
        }

        hidDeviceInst = *HidDevices + i;

        for (; i < *NumberDevices; i++, hidDeviceInst++) 
        {
            if (SetupDiEnumDeviceInterfaces(hardwareDeviceInfo,
                                            0, // No care about specific PDOs
                                            &hidGuid,
                                            i,
                                            &deviceInfoData))
            {
                //
                // allocate a function class device data structure to receive the
                // goods about this particular device.
                //
                SetupDiGetDeviceInterfaceDetail(
                        hardwareDeviceInfo,
                        &deviceInfoData,
                        NULL,  // probing so no output buffer yet
                        0,     // probing so output buffer length of zero
                        &requiredLength,
                        NULL); // not interested in the specific dev-node


                predictedLength = requiredLength;

                functionClassDeviceData = (PSP_DEVICE_INTERFACE_DETAIL_DATA) malloc(predictedLength);

                if (functionClassDeviceData)
                {
                    functionClassDeviceData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
                    ZeroMemory(functionClassDeviceData->DevicePath, sizeof(functionClassDeviceData->DevicePath));
                }
                else
                {
                    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
                    return FALSE;
                }

                //
                // Retrieve the information from Plug and Play.
                //
                if (! SetupDiGetDeviceInterfaceDetail(
                           hardwareDeviceInfo,
                           &deviceInfoData,
                           functionClassDeviceData,
                           predictedLength,
                           &requiredLength,
                           NULL)) 
                {
                    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
                    free(functionClassDeviceData);

                    return FALSE;
                }

                //
                // Open device with just generic query abilities to begin with
                //                
                if (! OpenHidDevice(functionClassDeviceData->DevicePath, 
                               FALSE,       // ReadAccess  - none
                               FALSE,       // WriteAccess - none
                               FALSE,       // Overlapped  - no
                               FALSE,       // Exclusive   - no
                               hidDeviceInst))
                {
                    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
                    free(functionClassDeviceData);

                    return FALSE;
                }
            } 
            else
            {
                if (GetLastError() == ERROR_NO_MORE_ITEMS) 
                {
                    done = TRUE;
                    break;
                }
            }
        }
    }

    *NumberDevices = i;

    SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
    free(functionClassDeviceData);

    return TRUE;
}

BOOLEAN OpenHidDevice(IN LPTSTR DevicePath, IN BOOL HasReadAccess, IN BOOL HasWriteAccess, IN BOOL IsOverlapped, IN BOOL IsExclusive, __inout PHID_DEVICE HidDevice)
{
    DWORD   accessFlags  = 0;
    DWORD   sharingFlags = 0;
    INT     iDevicePathSize;

    if (DevicePath == NULL)
    {
        return FALSE;
    }

    iDevicePathSize = (INT) (_tcslen(DevicePath) + 1) * sizeof(TCHAR);
    
    HidDevice->DevicePath = (PTCHAR) malloc(iDevicePathSize);

    if (HidDevice->DevicePath == NULL) 
    {
        return FALSE;
    }

    StringCbCopy(HidDevice->DevicePath, iDevicePathSize, DevicePath);
    
    if (HasReadAccess ) accessFlags |= GENERIC_READ;
    if (HasWriteAccess) accessFlags |= GENERIC_WRITE;

    if (!IsExclusive  ) sharingFlags = FILE_SHARE_READ | FILE_SHARE_WRITE;
    
    HidDevice->HidDevice = CreateFile(DevicePath, accessFlags, sharingFlags, NULL, OPEN_EXISTING, 0, NULL);

    if (HidDevice->HidDevice == INVALID_HANDLE_VALUE) 
    {
        free(HidDevice->DevicePath);
		HidDevice->DevicePath = NULL;

        return FALSE;
    }

    HidDevice->OpenedForRead    = HasReadAccess;
    HidDevice->OpenedForWrite   = HasWriteAccess;
    HidDevice->OpenedOverlapped = IsOverlapped;
    HidDevice->OpenedExclusive  = IsExclusive;
    
    if (!HidD_GetPreparsedData(HidDevice->HidDevice, &HidDevice->Ppd)) 
    {
        free(HidDevice->DevicePath);
		HidDevice->DevicePath = NULL;

        CloseHandle(HidDevice->HidDevice); 
		HidDevice->HidDevice = INVALID_HANDLE_VALUE;

        return FALSE;
    }

    if (!HidD_GetAttributes(HidDevice->HidDevice, &HidDevice->Attributes)) 
    {
        free(HidDevice->DevicePath); 
		HidDevice->DevicePath = NULL;

        CloseHandle(HidDevice->HidDevice); 
		HidDevice->HidDevice = INVALID_HANDLE_VALUE;

        HidD_FreePreparsedData(HidDevice->Ppd); 
		HidDevice->Ppd = NULL;

        return FALSE;
    }

    if (!HidP_GetCaps(HidDevice->Ppd, &HidDevice->Caps))
    {
        free(HidDevice->DevicePath);
		HidDevice->DevicePath = NULL;

        CloseHandle(HidDevice->HidDevice); 
		HidDevice->HidDevice = INVALID_HANDLE_VALUE;

        HidD_FreePreparsedData(HidDevice->Ppd);
		HidDevice->Ppd = NULL;

        return FALSE;
    }

    if (!FillDeviceInfo(HidDevice))
    {
        CloseHidDevice(HidDevice);

        return FALSE;
    }

    if (IsOverlapped)
    {
        CloseHandle(HidDevice->HidDevice);

        HidDevice->HidDevice = CreateFile(DevicePath, accessFlags, sharingFlags, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);

        if (HidDevice->HidDevice == INVALID_HANDLE_VALUE)
        {
            CloseHidDevice(HidDevice);

            return FALSE;
        }
    }

    return TRUE;
}

BOOLEAN FillDeviceInfo(IN PHID_DEVICE HidDevice)
{
    ULONG				numValues;
    USHORT				numCaps;
    PHIDP_BUTTON_CAPS	buttonCaps;
    PHIDP_VALUE_CAPS	valueCaps;
    PHID_DATA			data;
    ULONG				i;
    USAGE				usage;
    UINT				dataIdx;
    ULONG				newFeatureDataLength;
    ULONG				tmpSum;    

    HidDevice->InputReportBuffer = (PCHAR) calloc(HidDevice->Caps.InputReportByteLength, sizeof(CHAR));
    HidDevice->InputButtonCaps = buttonCaps = (PHIDP_BUTTON_CAPS) calloc(HidDevice->Caps.NumberInputButtonCaps, sizeof(HIDP_BUTTON_CAPS));

    if (buttonCaps == NULL)
    {
        return FALSE;
    }

    HidDevice->InputValueCaps = valueCaps = (PHIDP_VALUE_CAPS) calloc(HidDevice->Caps.NumberInputValueCaps, sizeof(HIDP_VALUE_CAPS));

    if (valueCaps == NULL)
    {
        return FALSE;
    }

    numCaps = HidDevice->Caps.NumberInputButtonCaps;

    if(numCaps > 0)
    {
        if(HidP_GetButtonCaps(HidP_Input, buttonCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numCaps = HidDevice->Caps.NumberInputValueCaps;

    if(numCaps > 0)
    {
        if(HidP_GetValueCaps(HidP_Input, valueCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numValues = 0;

    for (i = 0; i < HidDevice->Caps.NumberInputValueCaps; i++, valueCaps++) 
    {
        if (valueCaps->IsRange) 
        {
            numValues += valueCaps->Range.UsageMax - valueCaps->Range.UsageMin + 1;

            if(valueCaps->Range.UsageMin >= valueCaps->Range.UsageMax + (HidDevice->Caps).NumberInputButtonCaps)
            {
                return FALSE;
            }
        }
        else
        {
            numValues++;
        }
    }
    
    valueCaps = HidDevice->InputValueCaps;

    HidDevice->InputDataLength = HidDevice->Caps.NumberInputButtonCaps + numValues;
    HidDevice->InputData = data = (PHID_DATA) calloc(HidDevice->InputDataLength, sizeof(HID_DATA));

    if (data == NULL)
    {
        return FALSE;
    }

    dataIdx = 0;

    for (i = 0; i < HidDevice->Caps.NumberInputButtonCaps; i++, data++, buttonCaps++, dataIdx++) 
    {  
        data->IsButtonData = TRUE;
        data->Status = HIDP_STATUS_SUCCESS;
        data->UsagePage = buttonCaps->UsagePage;

        if (buttonCaps->IsRange) 
        {
            data->ButtonData.UsageMin = buttonCaps->Range.UsageMin;
            data->ButtonData.UsageMax = buttonCaps->Range.UsageMax;
        }
        else
        {
            data->ButtonData.UsageMin = data->ButtonData.UsageMax = buttonCaps->NotRange.Usage;
        }
        
        data->ButtonData.MaxUsageLength = HidP_MaxUsageListLength(HidP_Input, buttonCaps->UsagePage, HidDevice->Ppd);
        data->ButtonData.Usages = (PUSAGE) calloc(data->ButtonData.MaxUsageLength, sizeof(USAGE));

        data->ReportID = buttonCaps->ReportID;
    }

    for (i = 0; i < HidDevice->Caps.NumberInputValueCaps ; i++, valueCaps++)
    {
        if (valueCaps->IsRange) 
        {
            for (usage = valueCaps->Range.UsageMin; usage <= valueCaps->Range.UsageMax; usage++) 
            {
                if(dataIdx >= HidDevice->InputDataLength)
                {
                    return FALSE;
                }

                data->IsButtonData    = FALSE;
                data->Status          = HIDP_STATUS_SUCCESS;
                data->UsagePage       = valueCaps->UsagePage;
                data->ValueData.Usage = usage;
                data->ReportID        = valueCaps->ReportID;

				data++; dataIdx++;
            }
        } 
        else
        {
            if(dataIdx >= HidDevice->InputDataLength)
            {
                return FALSE;
            }

            data->IsButtonData    = FALSE;
            data->Status          = HIDP_STATUS_SUCCESS;
            data->UsagePage       = valueCaps->UsagePage;
            data->ValueData.Usage = valueCaps->NotRange.Usage;
            data->ReportID        = valueCaps->ReportID;

            data++; dataIdx++;
        }
    }

	HidDevice->OutputReportBuffer = (PCHAR) calloc(HidDevice->Caps.OutputReportByteLength, sizeof(CHAR));
    HidDevice->OutputButtonCaps = buttonCaps = (PHIDP_BUTTON_CAPS) calloc(HidDevice->Caps.NumberOutputButtonCaps, sizeof(HIDP_BUTTON_CAPS));

    if (buttonCaps == NULL)
    {
        return FALSE;
    }    

    HidDevice->OutputValueCaps = valueCaps = (PHIDP_VALUE_CAPS) calloc(HidDevice->Caps.NumberOutputValueCaps, sizeof(HIDP_VALUE_CAPS));

    if (valueCaps == NULL)
    {
        return FALSE;
    }

    numCaps = HidDevice->Caps.NumberOutputButtonCaps;

    if(numCaps > 0)
    {
        if(HidP_GetButtonCaps(HidP_Output, buttonCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numCaps = HidDevice->Caps.NumberOutputValueCaps;

    if(numCaps > 0)
    {
        if(HidP_GetValueCaps(HidP_Output, valueCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numValues = 0;

    for (i = 0; i < HidDevice->Caps.NumberOutputValueCaps; i++, valueCaps++) 
    {
        if (valueCaps->IsRange) 
        {
            numValues += valueCaps->Range.UsageMax - valueCaps->Range.UsageMin + 1;
        } 
        else
        {
            numValues++;
        }
    }

    valueCaps = HidDevice->OutputValueCaps;

    HidDevice->OutputDataLength = HidDevice->Caps.NumberOutputButtonCaps + numValues;
    HidDevice->OutputData = data = (PHID_DATA) calloc(HidDevice->OutputDataLength, sizeof(HID_DATA));

    if (data == NULL)
    {
        return FALSE;
    }

    for (i = 0; i < HidDevice->Caps.NumberOutputButtonCaps; i++, data++, buttonCaps++) 
    {
        if (i >= HidDevice->OutputDataLength)
        {
            return FALSE;
        }

        if(FAILED(ULongAdd(HidDevice->Caps.NumberOutputButtonCaps, valueCaps->Range.UsageMax, &tmpSum))) 
        {
            return FALSE;
        }        

        if(valueCaps->Range.UsageMin == tmpSum)
        {
            return FALSE;
        }
        
        data->IsButtonData = TRUE;
        data->Status       = HIDP_STATUS_SUCCESS;
        data->UsagePage    = buttonCaps->UsagePage;

        if (buttonCaps->IsRange)
        {
            data->ButtonData.UsageMin = buttonCaps->Range.UsageMin;
            data->ButtonData.UsageMax = buttonCaps->Range.UsageMax;
        }
        else
        {
            data->ButtonData.UsageMin = data->ButtonData.UsageMax = buttonCaps->NotRange.Usage;
        }

        data->ButtonData.MaxUsageLength = HidP_MaxUsageListLength(HidP_Output, buttonCaps->UsagePage, HidDevice->Ppd);
        data->ButtonData.Usages = (PUSAGE) calloc(data->ButtonData.MaxUsageLength, sizeof(USAGE));
        data->ReportID = buttonCaps->ReportID;
    }

    for (i = 0; i < HidDevice->Caps.NumberOutputValueCaps ; i++, valueCaps++)
    {
        if (valueCaps->IsRange)
        {
            for (usage = valueCaps->Range.UsageMin; usage <= valueCaps->Range.UsageMax; usage++) 
            {
                data->IsButtonData    = FALSE;
                data->Status          = HIDP_STATUS_SUCCESS;
                data->UsagePage       = valueCaps->UsagePage;
                data->ValueData.Usage = usage;
                data->ReportID        = valueCaps->ReportID;

                data++;
            }
        }
        else
        {
            data->IsButtonData    = FALSE;
            data->Status          = HIDP_STATUS_SUCCESS;
            data->UsagePage       = valueCaps->UsagePage;
            data->ValueData.Usage = valueCaps->NotRange.Usage;
            data->ReportID        = valueCaps->ReportID;

            data++;
        }
    }

    HidDevice->FeatureReportBuffer = (PCHAR) calloc(HidDevice->Caps.FeatureReportByteLength, sizeof(CHAR));
    HidDevice->FeatureButtonCaps = buttonCaps = (PHIDP_BUTTON_CAPS) calloc(HidDevice->Caps.NumberFeatureButtonCaps, sizeof(HIDP_BUTTON_CAPS));

    if (buttonCaps == NULL)
    {
        return FALSE;
    }

    HidDevice->FeatureValueCaps = valueCaps = (PHIDP_VALUE_CAPS) calloc(HidDevice->Caps.NumberFeatureValueCaps, sizeof(HIDP_VALUE_CAPS));

    if (valueCaps == NULL)
    {
        return FALSE;
    }

    numCaps = HidDevice->Caps.NumberFeatureButtonCaps;

    if(numCaps > 0)
    {
        if(HidP_GetButtonCaps(HidP_Feature, buttonCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numCaps = HidDevice->Caps.NumberFeatureValueCaps;

    if(numCaps > 0)
    {
        if(HidP_GetValueCaps(HidP_Feature, valueCaps, &numCaps, HidDevice->Ppd) != HIDP_STATUS_SUCCESS)
        {
            return FALSE;
        }
    }

    numValues = 0;

    for (i = 0; i < HidDevice->Caps.NumberFeatureValueCaps; i++, valueCaps++) 
    {
        if (valueCaps->IsRange) 
        {
            numValues += valueCaps->Range.UsageMax - valueCaps->Range.UsageMin + 1;
        }
        else
        {
            numValues++;
        }
    }

    valueCaps = HidDevice->FeatureValueCaps;

    if(FAILED(ULongAdd(HidDevice->Caps.NumberFeatureButtonCaps, numValues, &newFeatureDataLength))) 
    {
        return FALSE;
    }

    HidDevice->FeatureDataLength = newFeatureDataLength;
    HidDevice->FeatureData = data = (PHID_DATA) calloc(HidDevice->FeatureDataLength, sizeof(HID_DATA));

    if (data == NULL)
    {
        return FALSE;
    }

    dataIdx = 0;

    for (i = 0; i < HidDevice->Caps.NumberFeatureButtonCaps; i++, data++, buttonCaps++, dataIdx++) 
    {
        data->IsButtonData = TRUE;
        data->Status       = HIDP_STATUS_SUCCESS;
        data->UsagePage    = buttonCaps->UsagePage;

        if (buttonCaps->IsRange)
        {
            data->ButtonData.UsageMin = buttonCaps->Range.UsageMin;
            data->ButtonData.UsageMax = buttonCaps->Range.UsageMax;
        }
        else
        {
            data->ButtonData.UsageMin = data->ButtonData.UsageMax = buttonCaps->NotRange.Usage;
        }
        
        data->ButtonData.MaxUsageLength = HidP_MaxUsageListLength(HidP_Feature, buttonCaps->UsagePage, HidDevice->Ppd);
        data->ButtonData.Usages = (PUSAGE) calloc(data->ButtonData.MaxUsageLength, sizeof(USAGE));

        data->ReportID = buttonCaps->ReportID;
    }

    for (i = 0; i < HidDevice->Caps.NumberFeatureValueCaps ; i++, valueCaps++) 
    {
        if (valueCaps->IsRange)
        {
            for (usage = valueCaps->Range.UsageMin; usage <= valueCaps->Range.UsageMax; usage++)
            {
                if(dataIdx >= HidDevice->FeatureDataLength)
                {
                    return FALSE;
                }

                data->IsButtonData    = FALSE;
                data->Status          = HIDP_STATUS_SUCCESS;
                data->UsagePage       = valueCaps->UsagePage;
                data->ValueData.Usage = usage;
                data->ReportID        = valueCaps->ReportID;

                data++; dataIdx++;
            }
        } 
        else
        {
            if(dataIdx >= HidDevice->FeatureDataLength)
            {
                return FALSE;
            }

            data->IsButtonData    = FALSE;
            data->Status          = HIDP_STATUS_SUCCESS;
            data->UsagePage       = valueCaps->UsagePage;
            data->ValueData.Usage = valueCaps->NotRange.Usage;
            data->ReportID        = valueCaps->ReportID;

            data++; dataIdx++;
        }
    }

    return TRUE;
}

VOID CloseHidDevices(IN PHID_DEVICE HidDevices, IN ULONG NumberDevices)
{
    for (ULONG Index = 0; Index < NumberDevices; Index++) 
    {
        CloseHidDevice(&HidDevices[Index]);
    }
}

VOID CloseHidDevice(IN PHID_DEVICE HidDevice)
{
    if (HidDevice->DevicePath != NULL)
    {
        free(HidDevice->DevicePath);
        HidDevice->DevicePath = NULL;
    }

    if (HidDevice->HidDevice != INVALID_HANDLE_VALUE)
    {
        CloseHandle(HidDevice->HidDevice);
        HidDevice->HidDevice = INVALID_HANDLE_VALUE;
    }
    
    if (HidDevice->Ppd != NULL)
    {
        HidD_FreePreparsedData(HidDevice->Ppd);
        HidDevice->Ppd = NULL;
    }

    if (HidDevice->InputReportBuffer != NULL)
    {
        free(HidDevice->InputReportBuffer);
        HidDevice->InputReportBuffer = NULL;
    }

    if (HidDevice->InputData != NULL)
    {
        free(HidDevice->InputData);
        HidDevice->InputData = NULL;
    }

    if (HidDevice->InputButtonCaps != NULL)
    {
        free(HidDevice->InputButtonCaps);
        HidDevice->InputButtonCaps = NULL;
    }

    if (HidDevice->InputValueCaps != NULL)
    {
        free(HidDevice->InputValueCaps);
        HidDevice->InputValueCaps = NULL;
    }

    if (HidDevice->OutputReportBuffer != NULL)
    {
        free(HidDevice->OutputReportBuffer);
        HidDevice->OutputReportBuffer = NULL;
    }

    if (HidDevice->OutputData != NULL)
    {
        free(HidDevice->OutputData);
        HidDevice->OutputData = NULL;
    }

    if (HidDevice->OutputButtonCaps != NULL) 
    {
        free(HidDevice->OutputButtonCaps);
        HidDevice->OutputButtonCaps = NULL;
    }

    if (HidDevice->OutputValueCaps != NULL)
    {
        free(HidDevice->OutputValueCaps);
        HidDevice->OutputValueCaps = NULL;
    }

    if (HidDevice->FeatureReportBuffer != NULL)
    {
        free(HidDevice->FeatureReportBuffer);
        HidDevice->FeatureReportBuffer = NULL;
    }

    if (HidDevice->FeatureData != NULL) 
    {
        free(HidDevice->FeatureData);
        HidDevice->FeatureData = NULL;
    }

    if (HidDevice->FeatureButtonCaps != NULL) 
    {
        free(HidDevice->FeatureButtonCaps);
        HidDevice->FeatureButtonCaps = NULL;
    }

    if (HidDevice->FeatureValueCaps != NULL) 
    {
        free(HidDevice->FeatureValueCaps);
        HidDevice->FeatureValueCaps = NULL;
    }

	return;
}
