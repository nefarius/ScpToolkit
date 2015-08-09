# ScpServer
XInput Wrapper for DS3 and Play.com USB Dual DS2 Controller

## Credits
 * Major props to [original author Scarlet.Crush](http://forums.pcsx2.net/User-Scarlet-Crush) for developing this awesome toolkit!
 * [Original PCSX2 forums thread](http://forums.pcsx2.net/Thread-XInput-Wrapper-for-DS3-and-Play-com-USB-Dual-DS2-Controller)
 * [MadMilkman.Ini parsing library by Mario Z.](https://github.com/MarioZ/MadMilkman.Ini)
 * [reactivesockets library by Daniel Cazzulino](https://github.com/clariuslabs/reactivesockets)
 * [Windows Driver Installer library for USB devices](https://github.com/pbatard/libwdi)
 * To be extended...

## Installation requirements
 * [Microsoft .NET Framework 4.5](https://www.microsoft.com/en-US/download/details.aspx?id=42642)
 * [Microsoft Visual C++ 2013 Runtime](https://www.microsoft.com/en-US/download/details.aspx?id=40784)
 * [DirectX Runtime](https://www.microsoft.com/en-US/download/details.aspx?id=35)
 * [Xbox 360 Controller driver](https://www.microsoft.com/hardware/en-US/d/xbox-360-controller-for-windows)
  * Already integrated in Windows 8.x or greater
 * At least one supported Sony DualShock 3/4 controller (see **Compatible Controllers**)
 * Administrator rights *during driver setup*

### Optional
 * A supported Bluetooth 2.0 (or higher) compatible USB dongle **with EDR** (**E**nhanced **D**ata **R**ate)
  * See **Compatible Bluetooth Dongles**
 * For DS4s to be useable the minimal supported Bluetooth version is **2.1**!

## Installation How-To
1. Connect your Dongle (only needed if you want to use it wirelessly) and DS3 via USB and let Windows install it's default drivers.
2. Download the [latest release of ScpServer](https://github.com/nefarius/ScpServer/releases/latest) to an arbitrary location on your PC.
3. Right click on the archive and select `Properties` (depends on your native OS language).
4. Click the `Unblock` button if it is displayed on the `General` tab.
 - **Don't skip this step!** The driver setup may fail because Windows won't install driver files tagged as "unsafe" (e.g. downloaded from the big bad Internet).
5. Extract the archive to a location of your choice.
6. Create a directory where you want the Service to run from. (e.g `C:\Program Files\Scarlet.Crush Productions`)
7. Copy the contents of the `bin` directory to the location you created.
8. Run `ScpDriver.exe`. You may be propted to permit execution as administrator. Please accept or the installation will fail.
9. The next step depends on your operating system:
 - XP/Vista: check the Force Install option.
 - Win 7/8/8.1: leave it unchecked.
10. Click Install.

![Install Screenshot](http://nefarius.at/wp-content/uploads/2015/07/30-07-_2015_14-58-03.png "Install Screenshot")

## To-Do list
 * Increase supported controller count from 4 to 8

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

## Compatible Bluetooth Dongles
To be filled...
 * `USB\VID_03F0&PID_231D`
 * `USB\VID_045E&PID_3500`
 * `USB\VID_0461&PID_4D75`
 * `USB\VID_046D&PID_C709`
 * `USB\VID_047D&PID_105E`
 * `USB\VID_0489&PID_E011`
 * `USB\VID_0489&PID_E042`
 * `USB\VID_0489&PID_E04E`
 * `USB\VID_04CA&PID_3006`
 * `USB\VID_050D&PID_016A`
 * `USB\VID_05AC&PID_821A`
 * `USB\VID_05AC&PID_821F`
 * `USB\VID_07D1&PID_F101`
 * `USB\VID_0930&PID_0215`
 * `USB\VID_0A12&PID_0001`
 * `USB\VID_0A5C&PID_200A`
 * `USB\VID_0A5C&PID_2101`
 * `USB\VID_0A5C&PID_2146`
 * `USB\VID_0A5C&PID_2148`
 * `USB\VID_0A5C&PID_2150`
 * `USB\VID_0A5C&PID_2153`
 * `USB\VID_0A5C&PID_217D`
 * `USB\VID_0A5C&PID_2198`
 * `USB\VID_0A5C&PID_21E1`
 * `USB\VID_0A5C&PID_21E8`
 * `USB\VID_0B05&PID_1715`
 * `USB\VID_0B05&PID_1783`
 * `USB\VID_0B05&PID_1785`
 * `USB\VID_0B05&PID_179C`
 * `USB\VID_0B05&PID_17B5`
 * `USB\VID_0B05&PID_B700`
 * `USB\VID_0C10&PID_0000`
 * `USB\VID_0CF3&PID_3002`
 * `USB\VID_0CF3&PID_3004`
 * `USB\VID_0CF3&PID_3005`
 * `USB\VID_0DB0&PID_3801`
 * `USB\VID_0DF6&PID_2200`
 * `USB\VID_0E5E&PID_6622`
 * `USB\VID_1131&PID_1001`
 * `USB\VID_1131&PID_1004`
 * `USB\VID_1286&PID_2044&MI_00`
 * `USB\VID_13D3&PID_3304`
 * `USB\VID_413C&PID_8126` 
 * `USB\VID_8086&PID_0189`
 * `USB\VID_8087&PID_07DA`
 * `USB\VID_0930&PID_0214`
 * `USB\VID_0A5C&PID_2154`
 * `USB\VID_0489&PID_E04D`
 * `USB\VID_413C&PID_8197`
 * `USB\VID_0A5C&PID_2021`
 * `USB\VID_05AC&PID_8286`
 * `USB\VID_0A5C&PID_2100`
 * `USB\VID_0BDA&PID_8723`
 * `USB\VID_044E&PID_3010`
 * `USB\VID_0B05&PID_1788`
 * `USB\VID_0A5C&PID_2190`
 * `USB\VID_13D3&PID_3315`
 * `USB\VID_0489&PID_E027`
 * `USB\VID_05AC&PID_821D`
 * `USB\VID_0BDA&PID_0724`
 * `USB\VID_050D&PID_065A`
 * `USB\VID_0A5C&PID_21E3`
 * `USB\VID_05AC&PID_8216`
 * `USB\VID_0A5C&PID_21B4`


