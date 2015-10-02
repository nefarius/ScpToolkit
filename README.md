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
  * See [**Compatible Bluetooth Dongles**](#compatible-bluetooth-dongles)
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

## Compatible Bluetooth Devices
To be filled...

### USB Dongles
Hardware ID | Information | Name | Shop
----------- | ----------- | ---- | ----
`USB\VID_0461&PID_4D75` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0461%26PID_4D75+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0461&pid_4D75) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0461/4D75) | [Rocketfish™ Bluetooth USB Adapter RF-FLBTAD](http://deutsch.rocketfishproducts.de/products/computer-accessories/RF-FLBTAD.html) | [Google](https://encrypted.google.com/search?q=RF-FLBTAD&tbm=shop)
`USB\VID_050D&PID_065A` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_050D%26PID_065A+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_050D&pid_065A) / [usb.ids](https://usb-ids.gowdy.us/read/UD/050D/065A) | [Belkin F8T065bf](http://www.belkin.com/us/support-product?pid=01t80000003Hgu9AAC) | [geizhals](http://geizhals.eu/belkin-f8t065bf-a1046310.html) [Google](https://encrypted.google.com/search?q=Belkin%20F8T065bf&tbm=shop)
`USB\VID_07D1&PID_F101` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_07D1%26PID_F101+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_07D1&pid_F101) / [usb.ids](https://usb-ids.gowdy.us/read/UD/07D1/F101) | [DBT-122 Wireless USB Bluetooth Adapter](http://www.dlink.com/uk/en/support/product/dbt-122-wireless-usb-bluetooth-adapter) | [geizhals](http://geizhals.eu/d-link-dbt-122-a161528.html) [Google](https://encrypted.google.com/search?q=D-Link+DBT-122&tbm=shop)
`USB\VID_0B05&PID_17CB` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_17CB+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_17CB) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/17CB) | [ASUS USB-BT400](https://www.asus.com/Networking/USBBT400/) | [geizhals](http://geizhals.eu/asus-usb-bt400-90ig0070-bw0600-a939960.html) [Google](https://encrypted.google.com/search?q=ASUS%20USB-BT400&tbm=shop)
`USB\VID_0DF6&PID_2200` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0DF6%26PID_2200+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0DF6&pid_2200) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0DF6/2200) | Sitecom CN-512 [v1001](http://www.sitecomlearningcentre.com/products/cn-512v1001/bluetooth-2-0-usb-adapter/downloads) / [v1002](http://www.sitecomlearningcentre.com/products/cn-512v1002/bluetooth-2-0-usb-adapter/downloads) | [geizhals](http://geizhals.eu/sitecom-cn-512-a306835.html)
`USB\VID_0E5E&PID_6622` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0E5E%26PID_6622+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0E5E&pid_6622) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0E5E/6622) | Conwise CW6622
`USB\VID_1131&PID_1001` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_1131%26PID_1001+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_1131&pid_1001) / [usb.ids](https://usb-ids.gowdy.us/read/UD/1131/1001) | ISSC KY-BT100

### Integrated Modules/Chips in Notebooks or other Devices
Hardware ID | Information | Name
----------- | ----------- | ----
`USB\VID_03F0&PID_231D` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_03F0%26PID_231D+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_03F0&pid_231D) / [usb.ids](https://usb-ids.gowdy.us/read/UD/03F0/231D) | HP Integrated module with Bluetooth wireless technology (Broadcom BCM2070)
`USB\VID_044E&PID_3010` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_044E%26PID_3010+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_044E&pid_3010) / [usb.ids](https://usb-ids.gowdy.us/read/UD/044E/3010) | ALPS-UGPZ9-BCM2046
`USB\VID_046D&PID_C709` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_046D%26PID_C709+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_046D&pid_C709) / [usb.ids](https://usb-ids.gowdy.us/read/UD/046D/C709) | HP Bluetooth Module with trace filter
`USB\VID_047D&PID_105E` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_047D%26PID_105E+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_047D&pid_105E) / [usb.ids](https://usb-ids.gowdy.us/read/UD/047D/105E) | Kensington Bluetooth EDR Dongle
`USB\VID_0489&PID_E011` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0489%26PID_E011+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0489&pid_E011) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0489/E011) | Broadcom BCM2046
`USB\VID_0489&PID_E027` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0489%26PID_E027+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0489&pid_E027) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0489/E027) | Atheros AR3011 Bluetooth(R) Adapter
`USB\VID_0489&PID_E042` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0489%26PID_E042+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0489&pid_E042) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0489/E042) | Broadcom BCM20702
`USB\VID_0489&PID_E04D` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0489%26PID_E04D+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0489&pid_E04D) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0489/E04D) | Atheros AR3012 Bluetooth(R) Adapter
`USB\VID_0489&PID_E04E` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0489%26PID_E04E+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0489&pid_E04E) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0489/E04E) | Bluetooth USB Module
`USB\VID_04CA&PID_3006` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_04CA%26PID_3006+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_04CA&pid_3006) / [usb.ids](https://usb-ids.gowdy.us/read/UD/04CA/3006) | BlueSoleil Generic Bluetooth Driver
`USB\VID_050D&PID_016A` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_050D%26PID_016A+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_050D&pid_016A) / [usb.ids](https://usb-ids.gowdy.us/read/UD/050D/016A) | Broadcom BCM2046B1 (Belkin)
`USB\VID_05AC&PID_8216` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_05AC%26PID_8216+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_05AC&pid_8216) / [usb.ids](https://usb-ids.gowdy.us/read/UD/05AC/8216) | Broadcom Bluetooth 2.1 (MacBookAir2)
`USB\VID_05AC&PID_821A` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_05AC%26PID_821A+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_05AC&pid_821A) / [usb.ids](https://usb-ids.gowdy.us/read/UD/05AC/821A) | Apple Broadcom Built-in Bluetooth (MacBookPro8)
`USB\VID_05AC&PID_821D` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_05AC%26PID_821D+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_05AC&pid_821D) / [usb.ids](https://usb-ids.gowdy.us/read/UD/05AC/821D) | Apple Broadcom Built-in Bluetooth (MacBookPro9)
`USB\VID_05AC&PID_821F` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_05AC%26PID_821F+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_05AC&pid_821F) / [usb.ids](https://usb-ids.gowdy.us/read/UD/05AC/821F) | Apple Broadcom Built-in Bluetooth (MacBookAir4)
`USB\VID_05AC&PID_8286` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_05AC%26PID_8286+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_05AC&pid_8286) / [usb.ids](https://usb-ids.gowdy.us/read/UD/05AC/8286) | Apple Broadcom Built-in Bluetooth (MacBookPro10)
`USB\VID_0930&PID_0214` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0930%26PID_0214+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0930&pid_0214) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0930/0214) | Bluetooth USB Controller-9 from TOSHIBA (Broadcom BCM2070)
`USB\VID_0930&PID_0215` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0930%26PID_0215+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0930&pid_0215) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0930/0215) | Bluetooth USB Controller-10 from TOSHIBA
`USB\VID_0A12&PID_0001` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A12%26PID_0001+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A12&pid_0001) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A12/0001) | CSR Bluetooth Device
`USB\VID_0A5C&PID_200A` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_200A+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_200A) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/200A) | Broadcom BCM2035
`USB\VID_0A5C&PID_2021` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2021+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2021) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2021) | Broadcom BCM2035B3
`USB\VID_0A5C&PID_2100` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2100+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2100) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2100) | Broadcom BCM2045
`USB\VID_0A5C&PID_2101` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2101+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2101) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2101) | Broadcom BCM2045
`USB\VID_0A5C&PID_2146` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2146+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2146) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2146) | Broadcom BCM2046
`USB\VID_0A5C&PID_2148` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2148+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2148) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2148) | Broadcom BCM92046DG
`USB\VID_0A5C&PID_2150` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2150+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2150) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2150) | Broadcom BCM2046
`USB\VID_0A5C&PID_2153` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2153+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2153) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2153) | Broadcom BCM2046
`USB\VID_0A5C&PID_2154` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2154+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2154) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2154) | Broadcom BCM92046DG-CL1ROM Bluetooth 2.1 UHE Dongle
`USB\VID_0A5C&PID_217D` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_217D+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_217D) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/217D) | HP Bluetooth module
`USB\VID_0A5C&PID_2190` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2190+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2190) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2190) | Broadcom BCM2070
`USB\VID_0A5C&PID_2198` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_2198+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_2198) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/2198) | Broadcom BCM2070
`USB\VID_0A5C&PID_21B4` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_21B4+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_21B4) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/21B4) | Broadcom BCM2070
`USB\VID_0A5C&PID_21E1` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_21E1+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_21E1) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/21E1) | Broadcom BCM20702A0 (Driver for Hewlett-Packard)
`USB\VID_0A5C&PID_21E3` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_21E3+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_21E3) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/21E3) | Broadcom BCM20702A0 (Driver for Hewlett-Packard)
`USB\VID_0A5C&PID_21E8` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0A5C%26PID_21E8+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0A5C&pid_21E8) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0A5C/21E8) | Broadcom BCM20702A0
`USB\VID_0B05&PID_1715` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_1715+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_1715) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/1715) | ASUS Bluetooth Dongle (Broadcom BCM2045)
`USB\VID_0B05&PID_1783` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_1783+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_1783) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/1783) | ASUS Bluetooth v2.1 USB Adapter
`USB\VID_0B05&PID_1788` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_1788+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_1788) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/1788) | BT-270 (ASUS)
`USB\VID_0B05&PID_179C` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_179C+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_179C) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/179C) | Bluetooth (ASUS)
`USB\VID_0B05&PID_17B5` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_17B5+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_17B5) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/17B5) | Bluetooth (ASUS)
`USB\VID_0B05&PID_B700` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_B700+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_B700) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/B700) | BT-253 (ASUS)
`USB\VID_0BDA&PID_0724` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0BDA%26PID_0724+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0BDA&pid_0724) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0BDA/0724) | Realtek Bluetooth 4.0
`USB\VID_0BDA&PID_8723` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0BDA%26PID_8723+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0BDA&pid_8723) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0BDA/8723) | Realtek Bluetooth 4.0
`USB\VID_0CF3&PID_3002` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0CF3%26PID_3002+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0CF3&pid_3002) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0CF3/3002) | Atheros AR3011
`USB\VID_0CF3&PID_3004` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0CF3%26PID_3004+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0CF3&pid_3004) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0CF3/3004) | Atheros AR3012
`USB\VID_0CF3&PID_3005` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0CF3%26PID_3005+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0CF3&pid_3005) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0CF3/3005) | Atheros AR3011
`USB\VID_0DB0&PID_3801` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0DB0%26PID_3801+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0DB0&pid_3801) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0DB0/3801) | Motorola Bluetooth 2.1+EDR Device (MSI)
`USB\VID_1131&PID_1004` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_1131%26PID_1004+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_1131&pid_1004) / [usb.ids](https://usb-ids.gowdy.us/read/UD/1131/1004) | ISSC (EDR) Bluetooth USB Adapter
`USB\VID_1286&PID_2044&MI_00` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com%20%22USB%5CVID_1286%26PID_2044%26MI_00%20Device%20ID%20matches%22) [driverlookup](https://driverlookup.com/hardware-id/usb/vid_1286&pid_2044&mi_00) / [usb.ids](https://usb-ids.gowdy.us/read/UD/1286/2044) | Marvell AVASTAR Bluetooth Radio Adapter (Microsoft Surface)
`USB\VID_13D3&PID_3304` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_13D3%26PID_3304+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_13D3&pid_3304) / [usb.ids](https://usb-ids.gowdy.us/read/UD/13D3/3304) | Atheros AR3011 (Azurewave Janus 3304)
`USB\VID_13D3&PID_3315` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_13D3%26PID_3315+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_13D3&pid_3315) / [usb.ids](https://usb-ids.gowdy.us/read/UD/13D3/3315) | Bluetooth module (ASUS)
`USB\VID_413C&PID_8126` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_413C%26PID_8126+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_413C&pid_8126) / [usb.ids](https://usb-ids.gowdy.us/read/UD/413C/8126) | Dell Wireless 355 Module with Bluetooth 2.0 + EDR Technology
`USB\VID_413C&PID_8197` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_413C%26PID_8197+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_413C&pid_8197) / [usb.ids](https://usb-ids.gowdy.us/read/UD/413C/8197) | Dell Wireless 380 Bluetooth 4.0 Module (Broadcom BCM20702A0)
`USB\VID_8086&PID_0189` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_8086%26PID_0189+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_8086&pid_0189) / [usb.ids](https://usb-ids.gowdy.us/read/UD/8086/0189) | [Intel Centrino Advanced-N 6230 Bluetooth adapter](http://geizhals.at/?fs=intel+centrino+advanced-n+6230)
`USB\VID_8087&PID_07DA` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_8087%26PID_07DA+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_8087&pid_07DA) / [usb.ids](https://usb-ids.gowdy.us/read/UD/8087/07DA) | Intel Centrino Wireless Bluetooth 4.0 + High Speed Adapter

### Unknown Devices
Hardware ID | Information | Name
----------- | ----------- | ----
`USB\VID_045E&PID_3500` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_045E%26PID_3500+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_045E&pid_3500) / [usb.ids](https://usb-ids.gowdy.us/read/UD/045E/3500) |
`USB\VID_0B05&PID_1785` | [driveridentifier](https://encrypted.google.com/search?q=site%3Adriveridentifier.com+%22USB%5CVID_0B05%26PID_1785+Device+ID+matches%22) / [driverlookup](https://driverlookup.com/hardware-id/usb/vid_0B05&pid_1785) / [usb.ids](https://usb-ids.gowdy.us/read/UD/0B05/1785) |
