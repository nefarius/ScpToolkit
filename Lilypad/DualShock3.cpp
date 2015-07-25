#include "Global.h"
#include "InputManager.h"
#include <xinput.h>

typedef struct
{
	float SCP_UP;
	float SCP_RIGHT;
	float SCP_DOWN;
	float SCP_LEFT;

	float SCP_LX;
	float SCP_LY;

	float SCP_L1;
	float SCP_L2;
	float SCP_L3;

	float SCP_RX;
	float SCP_RY;

	float SCP_R1;
	float SCP_R2;
	float SCP_R3;

	float SCP_T;
	float SCP_C;
	float SCP_X;
	float SCP_S;

	float SCP_SELECT;
	float SCP_START;

	float SCP_PS;

} SCP_EXTN;

typedef void  (CALLBACK *_XInputEnable)(BOOL enable);
typedef DWORD(CALLBACK *_XInputGetState)(DWORD dwUserIndex, XINPUT_STATE* pState);
typedef DWORD(CALLBACK *_XInputSetState)(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);
typedef DWORD(CALLBACK *_XInputGetExtended)(DWORD dwUserIndex, SCP_EXTN* pPressure);

static _XInputEnable      pXInputEnable = 0;
static _XInputGetState    pXInputGetState = 0;
static _XInputSetState    pXInputSetState = 0;
static _XInputGetExtended pXInputGetExtended = 0;

static int xInputActiveCount = 0;

__forceinline int FloatToValue(float f)
{
	return (int)(f * ((float)(FULLY_DOWN)));
}

class DualShock3Device : public Device
{
protected:
	// Cached last vibration values by pad and motor.
	// Need this, as only one value is changed at a time.
	int ps2Vibration[2][4][2];

	// Minor optimization - cache last set vibration values
	// When there's no change, no need to do anything.
	XINPUT_VIBRATION xInputVibration;

public:
	int index;

	DualShock3Device(int index, wchar_t *name) : Device(DS3, OTHER, name)
	{
		memset(ps2Vibration, 0, sizeof(ps2Vibration));
		memset(&xInputVibration, 0, sizeof(xInputVibration));

		this->index = index;

		for (int i = 0; i < 12; i++)
		{
			AddPhysicalControl(PRESSURE_BTN, i, 0);
		}

		for (int i = 12; i < 16; i++)
		{
			AddPhysicalControl(PSHBTN, i, 0);
		}

		for (int i = 16; i < 20; i++)
		{
			AddPhysicalControl(ABSAXIS, i, 0);
		}

		AddPhysicalControl(PSHBTN, 20, 0);

		AddFFAxis(L"Slow Motor", 0);
		AddFFAxis(L"Fast Motor", 1);

		AddFFEffectType(L"Constant Effect", L"Constant", EFFECT_CONSTANT);
	}

	wchar_t *GetPhysicalControlName(PhysicalControl *control)
	{
		const static wchar_t *names[] =
		{
			L"Up",
			L"Right",
			L"Down",
			L"Left",
			L"Triangle",
			L"Circle",
			L"Cross",
			L"Square",
			L"L1",
			L"L2",
			L"R1",
			L"R2",
			L"L3",
			L"R3",
			L"Select",
			L"Start",
			L"L-Stick X",
			L"L-Stick Y",
			L"R-Stick X",
			L"R-Stick Y",
			L"PS",
		};

		unsigned int i = (unsigned int)(control - physicalControls);

		if (i < 21) return (wchar_t*)names[i];

		return Device::GetPhysicalControlName(control);
	}

	int Activate(InitInfo *initInfo)
	{
		if (active) Deactivate();

		if (!xInputActiveCount) pXInputEnable(1);

		xInputActiveCount++;
		active = 1;

		AllocState();
		return 1;
	}

	int Update()
	{
		if (!active) return 0;

		SCP_EXTN	 extn;

		if (pXInputGetExtended(index, &extn) != ERROR_SUCCESS)
		{
			Deactivate();
			return 0;
		}

		physicalControlState[0] = FloatToValue(extn.SCP_UP);
		physicalControlState[1] = FloatToValue(extn.SCP_RIGHT);
		physicalControlState[2] = FloatToValue(extn.SCP_DOWN);
		physicalControlState[3] = FloatToValue(extn.SCP_LEFT);
		physicalControlState[4] = FloatToValue(extn.SCP_T);
		physicalControlState[5] = FloatToValue(extn.SCP_C);
		physicalControlState[6] = FloatToValue(extn.SCP_X);
		physicalControlState[7] = FloatToValue(extn.SCP_S);
		physicalControlState[8] = FloatToValue(extn.SCP_L1);
		physicalControlState[9] = FloatToValue(extn.SCP_L2);
		physicalControlState[10] = FloatToValue(extn.SCP_R1);
		physicalControlState[11] = FloatToValue(extn.SCP_R2);
		physicalControlState[12] = FloatToValue(extn.SCP_L3);
		physicalControlState[13] = FloatToValue(extn.SCP_R3);
		physicalControlState[14] = FloatToValue(extn.SCP_SELECT);
		physicalControlState[15] = FloatToValue(extn.SCP_START);
		physicalControlState[16] = FloatToValue(extn.SCP_LX);
		physicalControlState[17] = FloatToValue(extn.SCP_LY);
		physicalControlState[18] = FloatToValue(extn.SCP_RX);
		physicalControlState[19] = FloatToValue(extn.SCP_RY);
		physicalControlState[20] = FloatToValue(extn.SCP_PS);

		return 1;
	}

	void SetEffects(unsigned char port, unsigned int slot, unsigned char motor, unsigned char force)
	{
		int newVibration[2] = { 0, 0 };

		ps2Vibration[port][slot][motor] = force;

		for (int p = 0; p < 2; p++)
		{
			for (int s = 0; s < 4; s++)
			{
				for (int i = 0; i < pads[p][s].numFFBindings; i++)
				{
					ForceFeedbackBinding *ffb = &pads[p][s].ffBindings[i];

					newVibration[0] += (int)((ffb->axes[0].force * (__int64)ps2Vibration[p][s][ffb->motor]) / 255);
					newVibration[1] += (int)((ffb->axes[1].force * (__int64)ps2Vibration[p][s][ffb->motor]) / 255);
				}
			}
		}

		newVibration[0] = abs(newVibration[0]); if (newVibration[0] > 65535) newVibration[0] = 65535;
		newVibration[1] = abs(newVibration[1]); if (newVibration[1] > 65535) newVibration[1] = 65535;

		if (newVibration[0] != xInputVibration.wLeftMotorSpeed || newVibration[1] != xInputVibration.wRightMotorSpeed)
		{
			XINPUT_VIBRATION newv = { newVibration[0], newVibration[1] };

			if (pXInputSetState(index, &newv) == ERROR_SUCCESS)
			{
				xInputVibration = newv;
			}
		}
	}

	void SetEffect(ForceFeedbackBinding *binding, unsigned char force)
	{
		PadBindings pBackup = pads[0][0];

		pads[0][0].ffBindings = binding;
		pads[0][0].numFFBindings = 1;

		SetEffects(0, 0, binding->motor, force);

		pads[0][0] = pBackup;
	}

	void Deactivate()
	{
		memset(&xInputVibration, 0, sizeof(xInputVibration));
		memset(ps2Vibration, 0, sizeof(ps2Vibration));

		pXInputSetState(index, &xInputVibration);

		FreeState();

		if (active)
		{
			if (!--xInputActiveCount)
			{
				pXInputEnable(0);
			}

			active = 0;
		}
	}

	~DualShock3Device()
	{
	}
};

void EnumDualShock3s()
{
	wchar_t		 name[30];
	XINPUT_STATE state;
	SCP_EXTN	 extn;

	if (!pXInputSetState)
	{
		if (pXInputGetExtended) return;

		HMODULE hMod = 0;

		if (hMod = LoadLibraryW(L"XInput1_3.dll"))
		{
			if ((pXInputEnable = (_XInputEnable)GetProcAddress(hMod, "XInputEnable"))
				&& (pXInputGetState = (_XInputGetState)GetProcAddress(hMod, "XInputGetState"))
				&& (pXInputSetState = (_XInputSetState)GetProcAddress(hMod, "XInputSetState")))
			{
				pXInputGetExtended = (_XInputGetExtended)GetProcAddress(hMod, "XInputGetExtended");
			}
		}

		if (!pXInputGetExtended)
		{
			pXInputGetExtended = (_XInputGetExtended)-1;
			return;
		}
	}

	pXInputEnable(1);

	for (int index = 0; index < 4; index++)
	{
		if (pXInputGetState(index, &state) == ERROR_SUCCESS && pXInputGetExtended(index, &extn) == ERROR_SUCCESS)
		{
			wsprintfW(name, L"DualShock 3 #%i", index + 1);
			dm->AddDevice(new DualShock3Device(index, name));
		}
	}

	pXInputEnable(0);
}

int DualShock3Possible()
{
	int	retVal = 0;

	HMODULE hMod = 0;

	if (hMod = LoadLibraryW(L"XInput1_3.dll"))
	{
		_XInputGetExtended pXInputGetExtended = 0;

		if (pXInputGetExtended = (_XInputGetExtended)GetProcAddress(hMod, "XInputGetExtended"))
		{
			SCP_EXTN	 extn;

			for (int index = 0; index < 4; index++)
			{
				if (pXInputGetExtended(index, &extn) == ERROR_SUCCESS)
				{
					retVal = 1; break;
				}
			}
		}

		FreeLibrary(hMod);
	}

	return retVal;
}

void UninitLibUsb()
{
	return;
}
