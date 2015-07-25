// DS3User.cpp : main project file.

#include "stdafx.h"
#include "SCPPad.h"

using namespace SCPUser;

[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
	// Enabling Windows XP visual effects before any controls are created
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false); 

	// Create the main window and run it
	Application::Run(gcnew SCPPad());
	return 0;
}
