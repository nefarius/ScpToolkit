# ScpToolkit
Windows Driver and XInput Wrapper for Sony DualShock 3/4 Controllers

## Archive statement
**Dear community, it has been an amazing ride but the time has come to let go. The ScpToolkit has outgrown itself and won't be continued any longer. As we're speaking the team behind this solution is working on various successor projects so keep an eye open! Good bye!** 

## Credits
### Community
 * Major props to [original author Scarlet.Crush](http://forums.pcsx2.net/User-Scarlet-Crush) for developing this awesome toolkit!
 * [Original PCSX2 forums thread](http://forums.pcsx2.net/Thread-XInput-Wrapper-for-DS3-and-Play-com-USB-Dual-DS2-Controller)

### Sponsors
 * ScpToolkits Setup is created with [Advanced Installer](http://www.advancedinstaller.com/), a feature-rich and yet easy to learn installation package creation framework for the Windows platform.
 * Development is assisted by [JetBrains ReSharper Ultimate](https://www.jetbrains.com/dotnet/) tool collection, a must-have for every serious .NET developer.

### Used libraries and other 3rd party code
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
 * [Windows Input Simulator](http://inputsimulator.codeplex.com/)
 * [AutoDependencyProperty.Fody](http://blog.angeloflogic.com/2014/12/no-more-dependencyproperty-with.html)
 * [LoadAssembliesOnStartup](https://github.com/Fody/LoadAssembliesOnStartup)
 * [Costura](https://github.com/Fody/Costura/)
 * [DBreeze NoSql embedded object DBMS](https://dbreeze.codeplex.com/)
 * [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)

## Installation requirements
 * Microsoft Windows Vista/7/8/8.1/10 x86 or amd64
 * [Microsoft .NET Framework 4.5](https://www.microsoft.com/en-US/download/details.aspx?id=42642)
 * [Microsoft Visual C++ 2010 Redistributable Package](http://www.microsoft.com/en-US/download/details.aspx?id=5555)
 * [Microsoft Visual C++ 2013 Runtime](https://www.microsoft.com/en-US/download/details.aspx?id=40784)
 * [DirectX Runtime](https://www.microsoft.com/en-us/download/details.aspx?DisplayLang=en&id=35)
 * [Xbox 360 Controller driver](https://www.microsoft.com/accessories/en-us/products/gaming/xbox-360-controller-for-windows/52a-00004#techspecs-connect)
  * Already integrated in Windows 8.x or greater
 * At least one supported Sony DualShock 3/4 controller (see **Compatible Controllers**)
 * Administrator rights *during driver setup*

### Optional
 * A supported Bluetooth 2.0 (or higher) compatible USB dongle **with EDR** (**E**nhanced **D**ata **R**ate)
  * See [**Compatible Bluetooth Devices**](https://github.com/nefarius/ScpToolkit/wiki/Compatible-Bluetooth-Devices)
 * For DS4s to be useable the minimal supported Bluetooth version is **2.1**!

## Installation How-To
1. Connect your Dongle (only needed if you want to use it wirelessly) and controllers (yes, *all* of them) via USB and let Windows install its default drivers. **Leave them plugged in during the entire installation process!**
2. Download the [latest release of the ScpToolkit Setup](https://github.com/nefarius/ScpServer/releases/latest) to an arbitrary location on your PC.
3. Run the Setup and follow it's instructions. Should be fairly straight-forward.
4. Wait for the Setup to finish.
  * If you're performing a fresh installation, run the Driver Installer afterwards.
  * If you're upgrading from an older installation you may skip the Driver Installer.
5. When running the Driver Installer, choose your Bluetooth and controller devices you like to use with ScpToolkit.
6. The next step depends on your operating system:
 - Vista: check the Force Install option.
 - Win 7/8/8.1/10: leave it unchecked (or check if you're facing installation troubles, might help).
7. Click Install.
8. You're done!

![Install Screenshot](http://nefarius.at/wp-content/uploads/2013/12/31-10-_2015_13-27-55.png "Install Screenshot")

## To-Do list
 * Rewrite profile manager
 * Add support for fake PANHAI DS3 controllers
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
