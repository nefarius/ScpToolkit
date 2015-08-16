using System;
using System.Linq;
using System.Reflection;
using log4net;
using ScpControl.Utilities;

namespace ScpControl.Driver
{
    /// <summary>
    ///     Automated Windows driver installer utility class.
    /// </summary>
    public static class DriverInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static uint InstallBluetoothDongles()
        {
            // install compatible bluetooth dongles
            var bthDrivers = IniConfig.Instance.BthDongleDriver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in bthDrivers.HardwareIds
                let result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, bthDrivers.DeviceGuid, "Driver",
                    string.Format("BthDongle_{0}.inf", Guid.NewGuid()),
                    IntPtr.Zero)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for Bluetooth dongle {0}", hardwareId);
            }

            return installed;
        }

        public static uint InstallDualShock3Controllers()
        {
            // install compatible DS3 controllers
            var ds3Drivers = IniConfig.Instance.Ds3Driver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in ds3Drivers.HardwareIds
                let result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, ds3Drivers.DeviceGuid, "Driver",
                    string.Format("Ds3Controller_{0}.inf", Guid.NewGuid()), IntPtr.Zero)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 3 controller {0}", hardwareId);
            }

            return installed;
        }

        public static uint InstallDualShock4Controllers()
        {
            // install compatible DS4 controllers
            var ds4Drivers = IniConfig.Instance.Ds4Driver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in ds4Drivers.HardwareIds
                let result = WdiWrapper.Instance.InstallWinUsbDriver(hardwareId, ds4Drivers.DeviceGuid, "Driver",
                    string.Format("Ds4Controller_{0}.inf", Guid.NewGuid()), IntPtr.Zero)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 4 controller {0}", hardwareId);
            }

            return installed;
        }
    }
}