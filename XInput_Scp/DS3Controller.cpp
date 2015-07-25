#include "StdAfx.h"
#define REPORT_SIZE	49

// Byte  3 Right Motor
// Byte  5 Left Motor
// Byte 10 LEDs - Bit Flags: 0x2 LED 1, 0x4 LED 2, 0x8 LED 3, 0x10 LED 4

static BYTE l_Report[REPORT_SIZE] = 
{ 
	0x01, 
	0x00, 0xFF, 0x00, 0xFF, 0x00, 
	0x00, 0x00, 0x00, 0x00,	0x00, 
	0xFF, 0x27, 0x10, 0x00, 0x32, 
	0xFF, 0x27, 0x10, 0x00,	0x32, 
	0xFF, 0x27, 0x10, 0x00, 0x32, 
	0xFF, 0x27, 0x10, 0x00,	0x32, 
	0x00, 0x00, 0x00, 0x00, 0x00, 
	0x00, 0x00, 0x00, 0x00,	0x00, 
	0x00, 0x00, 0x00, 0x00, 0x00, 
	0x00, 0x00, 0x00,
};

static BYTE l_Flags[4] = { 0x02, 0x04, 0x08, 0x10 }; 

CDS3Controller::CDS3Controller(DWORD dwIndex) : CSCPController(dwIndex, REPORT_SIZE)
{
	m_deviceId = _tcsdup(_T("vid_054c&pid_0268"));

	memcpy(m_Report, l_Report, m_dwReportSize);
}

void CDS3Controller::FormatReport(void)
{
	m_Report[ 0] = (BYTE) m_lpHidDevice->OutputData[0].ReportID;

	m_Report[ 3] = (BYTE) (m_padVibration.wRightMotorSpeed >  0 ? 0x01 : 0); // Only has [ON|OFF]
	m_Report[ 5] = (BYTE) (m_padVibration.wLeftMotorSpeed  >> 8);

	m_Report[10] = (BYTE) l_Flags[m_dwIndex];
}

void CDS3Controller::XInputMapState(void)
{
	m_padState.Gamepad.wButtons = 0;

	for (ULONG Index = 0, Axis = 0; Index < m_lpHidDevice->InputDataLength; Index++)
	{
		if (m_lpHidDevice->InputData[Index].IsButtonData)
		{
			for (ULONG j = 0; j < m_lpHidDevice->InputData[Index].ButtonData.MaxUsageLength; j++)
			{
				// Remap for Buttons
				switch(m_lpHidDevice->InputData[Index].ButtonData.Usages[j])
				{
				case  1: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_BACK; break;
				case  2: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_LEFT_THUMB;  break;
				case  3: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_RIGHT_THUMB; break;
				case  4: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_START; break;

				case  5: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_UP;    break;
				case  6: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_RIGHT; break;
				case  7: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_DOWN;  break;
				case  8: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_DPAD_LEFT;  break;

				case 11: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_LEFT_SHOULDER;  break;
				case 12: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_RIGHT_SHOULDER; break;

				case 13: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_Y; break;
				case 14: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_B; break;
				case 15: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_A; break;
				case 16: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_X; break;

				case 17: m_padState.Gamepad.wButtons |= XINPUT_GAMEPAD_GUIDE; break;
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
			}
		}
	}

	// Remap for Triggers - Not Unpacked as Axis by UnpackReport
	m_padState.Gamepad.bLeftTrigger  = m_lpHidDevice->InputReportBuffer[18];
	m_padState.Gamepad.bRightTrigger = m_lpHidDevice->InputReportBuffer[19];

	// Convert for Extension
	m_Extended.SCP_UP    = ToPressure(m_lpHidDevice->InputReportBuffer[14]);
	m_Extended.SCP_RIGHT = ToPressure(m_lpHidDevice->InputReportBuffer[15]);
	m_Extended.SCP_DOWN  = ToPressure(m_lpHidDevice->InputReportBuffer[16]);
	m_Extended.SCP_LEFT  = ToPressure(m_lpHidDevice->InputReportBuffer[17]);

	m_Extended.SCP_LX = ToAxis(m_padState.Gamepad.sThumbLX);
	m_Extended.SCP_LY = ToAxis(m_padState.Gamepad.sThumbLY);

	m_Extended.SCP_L1 = ToPressure(m_lpHidDevice->InputReportBuffer[20]);
	m_Extended.SCP_L2 = ToPressure(m_lpHidDevice->InputReportBuffer[18]);
	m_Extended.SCP_L3 = m_padState.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB ? 1.0f : 0.0f;

	m_Extended.SCP_RX = ToAxis(m_padState.Gamepad.sThumbRX);
	m_Extended.SCP_RY = ToAxis(m_padState.Gamepad.sThumbRY);

	m_Extended.SCP_R1 = ToPressure(m_lpHidDevice->InputReportBuffer[21]);
	m_Extended.SCP_R2 = ToPressure(m_lpHidDevice->InputReportBuffer[19]);
	m_Extended.SCP_R3 = m_padState.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_THUMB ? 1.0f : 0.0f;

	m_Extended.SCP_T  = ToPressure(m_lpHidDevice->InputReportBuffer[22]);
	m_Extended.SCP_C  = ToPressure(m_lpHidDevice->InputReportBuffer[23]);
	m_Extended.SCP_X  = ToPressure(m_lpHidDevice->InputReportBuffer[24]);
	m_Extended.SCP_S  = ToPressure(m_lpHidDevice->InputReportBuffer[25]);

	m_Extended.SCP_SELECT = m_padState.Gamepad.wButtons & XINPUT_GAMEPAD_BACK  ? 1.0f : 0.0f;
	m_Extended.SCP_START  = m_padState.Gamepad.wButtons & XINPUT_GAMEPAD_START ? 1.0f : 0.0f;

	m_Extended.SCP_PS = m_padState.Gamepad.wButtons & XINPUT_GAMEPAD_GUIDE ? 1.0f : 0.0f;
}
