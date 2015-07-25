#include "StdAfx.h"
#define REPORT_SIZE	4

// Byte 2 Right Motor
// Byte 3 Left Motor

static BYTE l_Report[REPORT_SIZE] = 
{ 
	0x01, 0x00, 0x00, 0x00,
};

CDS2Controller::CDS2Controller(DWORD dwIndex) : CSCPController(dwIndex, REPORT_SIZE)
{
	m_deviceId = _tcsdup(dwIndex % CollectionSize == 0 ? _T("vid_0b43&pid_0003&col01") : _T("vid_0b43&pid_0003&col02"));

	memcpy(m_Report, l_Report, m_dwReportSize);

	m_dwIndex /= CollectionSize;

	m_bReportEnabled = false;
}

void CDS2Controller::FormatReport(void)
{
	m_Report[0] = (BYTE)  m_lpHidDevice->OutputData[0].ReportID;

	m_Report[2] = (BYTE) (m_padVibration.wRightMotorSpeed >  0 ? 0x01 : 0); // Only has [ON|OFF]
	m_Report[3] = (BYTE) (m_padVibration.wLeftMotorSpeed  >> 8);
}

void CDS2Controller::XInputMapState(void)
{
	m_padState.Gamepad.wButtons = 0;
	m_padState.Gamepad.bLeftTrigger  = 0;
	m_padState.Gamepad.bRightTrigger = 0;

	for (ULONG Index = 0, Axis = 0; Index < m_lpHidDevice->InputDataLength; Index++)
	{
		if (m_lpHidDevice->InputData[Index].IsButtonData)
		{
			for (ULONG j = 0; j < m_lpHidDevice->InputData[Index].ButtonData.MaxUsageLength; j++)
			{
				// Remap for Buttons + Triggers
				switch(m_lpHidDevice->InputData[Index].ButtonData.Usages[j])
				{
					case  1: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_Y; break;
					case  2: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_B; break;
					case  3: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_A; break;
					case  4: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_X; break;

					case  5: m_padState.Gamepad.bLeftTrigger  = (BYTE) 0xFF; break;
					case  6: m_padState.Gamepad.bRightTrigger = (BYTE) 0xFF; break;

					case  7: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER; break;
					case  8: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER; break;

					case  9: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_BACK; break;
					case 10: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_START; break;

					case 11: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB; break;
					case 12: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB; break;

					case 13: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_UP; break;
					case 14: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT; break;
					case 15: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN; break;
					case 16: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT; break;
				}
			}
		}
		else
		{
			// Remap for Axis
			switch(Axis++)
			{
				case 0: m_padState.Gamepad.sThumbRY = -Scale((SHORT) m_lpHidDevice->InputData[Index].ValueData.Value); break;
				case 1: m_padState.Gamepad.sThumbRX =  Scale((SHORT) m_lpHidDevice->InputData[Index].ValueData.Value); break;
				case 2: m_padState.Gamepad.sThumbLY = -Scale((SHORT) m_lpHidDevice->InputData[Index].ValueData.Value); break;
				case 3: m_padState.Gamepad.sThumbLX =  Scale((SHORT) m_lpHidDevice->InputData[Index].ValueData.Value); break;
				case 4:	// Digital Mode
					if ((m_lpHidDevice->InputData[Index].ValueData.Value == 0x41) || (m_lpHidDevice->InputData[Index].ValueData.Value == 0xFF))
					{
						m_padState.Gamepad.sThumbLX = m_padState.Gamepad.sThumbLY = m_padState.Gamepad.sThumbRX = m_padState.Gamepad.sThumbRY = 0;
					}
					break;
			}
		}
	}
}

DWORD CDS2Controller::GetExtended(DWORD dwUserIndex, SCP_EXTN *Pressure)
{
	return m_xConnected ? ERROR_NOT_SUPPORTED : ERROR_DEVICE_NOT_CONNECTED;;
}
