# ScpServer
Windows Driver and XInput Wrapper for Sony DualShock 3/4 Controllers

## Credits
 * Major props to [original author Scarlet.Crush](http://forums.pcsx2.net/User-Scarlet-Crush) for developing this awesome toolkit!
 * [Original PCSX2 forums thread](http://forums.pcsx2.net/Thread-XInput-Wrapper-for-DS3-and-Play-com-USB-Dual-DS2-Controller)
 * [MadMilkman.Ini parsing library by Mario Z.](https://github.com/MarioZ/MadMilkman.Ini)
 * [reactivesockets library by Daniel Cazzulino](https://github.com/clariuslabs/reactivesockets)
 * [Windows Driver Installer library for USB devices](https://github.com/pbatard/libwdi)
 * [PortableSettingsProvider](https://github.com/crdx/PortableSettingsProvider)
 * [log4net logging library](https://logging.apache.org/log4net/)
 * [Libarius .NET library](https://github.com/nefarius/Libarius)
 * [Rx networking library](https://github.com/clariuslabs/reactivesockets)
 * [libusbK driver package](https://code.google.com/p/usb-travis/)
 * [irrKlang cross platform sound library](http://www.ambiera.com/irrklang/index.html)
 * [Metro Light and Dark Themes for WPF](http://brianlagunas.com/free-metro-light-and-dark-themes-for-wpf-and-silverlight-microsoft-controls/)
 * [Fody/PropertyChanged](https://github.com/Fody/PropertyChanged)
 * `ScpCleanWipe` uses code from [DriverStore Explorer](https://driverstoreexplorer.codeplex.com/)
 * [AutoDependencyProperty.Fody](http://blog.angeloflogic.com/2014/12/no-more-dependencyproperty-with.html)
 * [HIDSharp library](http://www.zer7.com/software/hidsharp)

## Installation requirements
 * Microsoft Windows Vista/7/8/8.1/10 x86 or amd64
 * [Microsoft .NET Framework 4.5](https://www.microsoft.com/en-US/download/details.aspx?id=42642)
 * [Microsoft Visual C++ 2010 Redistributable Package](http://www.microsoft.com/en-US/download/details.aspx?id=5555)
 * [Microsoft Visual C++ 2013 Runtime](https://www.microsoft.com/en-US/download/details.aspx?id=40784)
 * [DirectX Runtime](https://www.microsoft.com/en-US/download/details.aspx?id=35)
 * [Xbox 360 Controller driver](https://www.microsoft.com/hardware/en-US/d/xbox-360-controller-for-windows)
  * Already integrated in Windows 8.x or greater
 * At least one supported Sony DualShock 3/4 controller (see **Compatible Controllers**)
 * Administrator rights *during driver setup*

### Optional
 * A supported Bluetooth 2.0 (or higher) compatible USB dongle **with EDR** (**E**nhanced **D**ata **R**ate)
  * See [**Compatible Bluetooth Devices**](https://github.com/nefarius/ScpServer/wiki/Compatible-Bluetooth-Devices)
 * For DS4s to be useable the minimal supported Bluetooth version is **2.1**!

## Installation How-To
1. Connect your Dongle (only needed if you want to use it wirelessly) and controllers via USB and let Windows install it's default drivers.
2. Download the [latest release of ScpServer](https://github.com/nefarius/ScpServer/releases/latest) to an arbitrary location on your PC.
3. Right click on the archive and select `Properties` (depends on your native OS language).
4. Click the `Unblock` button if it is displayed on the `General` tab.
 - **Don't skip this step!** The driver setup may fail because Windows won't install driver files tagged as "unsafe" (e.g. downloaded from the big bad Internet).
5. Extract the archive to a location of your choice.
6. Create a directory where you want the Service to run from. (e.g `C:\Program Files\Scarlet.Crush Productions`)
7. Copy the contents of the `bin` directory to the location you created.
8. Run `ScpDriver.exe`. You may be propted to permit execution as administrator. Please accept or the installation will fail.
9. The next step depends on your operating system:
 - Vista: check the Force Install option.
 - Win 7/8/8.1: leave it unchecked.
10. Click Install.

![Install Screenshot](http://nefarius.at/wp-content/uploads/2013/12/02-10-_2015_23-54-37.png "Install Screenshot")

## To-Do list
 * Increase supported controller count from 4 to 8
 * Rewrite profile manager
 * Add Turbo option
 * Add support for fake PANHAI DS3 controllers
 * Fix pressure sensitivity for PCSX2
 * Implement gyroscope and accelerometer readout for DS3 and DS4
 * Implement touchpad readout for DS4

## Compatible Controllers
To be filled...
 * `USB\VID_054C&PID_0268`
  * Original Sony DualShock 3 Controller
 * `USB\VID_054C&PID_0268&REV_0100`
  * BigBen BB4401 PS3PADRFLX (3rd Party Controller)
 * `USB\VID_0E6F&PID_0214&REV_0580`
  * Afterglow AP.2 Wireless Controller for PS3 (3rd Party Controller)
  * Although it's a wireless controller, technically it's an USB controller because it uses a proprietary protocol and ships with it's own USB dongle which can't/must not be paired manually.
  * Rumble, LED-Control and battery charging status isn't supported/implemented yet.
