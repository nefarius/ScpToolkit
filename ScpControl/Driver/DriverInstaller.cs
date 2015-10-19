using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using ScpControl.Utilities;

namespace ScpControl.Driver
{
    /// <summary>
    ///     Automated Windows driver (un)installer utility class.
    /// </summary>
    public static class DriverInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DriverDirectory = Path.Combine(WorkingDirectory, "Driver");

        public static uint InstallBluetoothDongles(IEnumerable<WdiUsbDevice> usbDevices, IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var bthDrivers = IniConfig.Instance.BthDongleDriver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallLibusbKDriver(usbDevice.HardwareId, bthDrivers.DeviceGuid,
                    "Driver", string.Format("BthDongle_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                installed++;
                Log.InfoFormat("Installed driver for Bluetooth dongle {0}", usbDevice);
            }

            return installed;
        }

        public static uint InstallBluetoothDongles(IntPtr hWnd = default(IntPtr), bool force = false)
        {
            // install compatible bluetooth dongles
            var bthDrivers = IniConfig.Instance.BthDongleDriver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in bthDrivers.HardwareIds
                let result = WdiWrapper.Instance.InstallLibusbKDriver(hardwareId, bthDrivers.DeviceGuid, "Driver",
                    string.Format("BthDongle_{0}.inf", Guid.NewGuid()),
                    hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for Bluetooth dongle {0}", hardwareId);
            }

            return installed;
        }

        public static uint UninstallBluetoothDongles(ref bool rebootRequired)
        {
            uint uninstalled = 0;

            if (!Directory.Exists(DriverDirectory))
                return uninstalled;

            foreach (
                var file in
                    Directory.GetFiles(DriverDirectory)
                        .Where(
                            f =>
                                Path.GetFileName(f).StartsWith("BthDongle_") &&
                                Path.GetExtension(f).ToLower().Equals(".inf")))
            {
                Difx.Instance.Uninstall(file, DifxFlags.DRIVER_PACKAGE_DELETE_FILES, out rebootRequired);
                uninstalled++;
            }

            return uninstalled;
        }

        public static uint InstallDualShock3Controllers(IEnumerable<WdiUsbDevice> usbDevices,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var ds3Drivers = IniConfig.Instance.Ds3Driver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallLibusbKDriver(usbDevice.HardwareId, ds3Drivers.DeviceGuid,
                    "Driver", string.Format("Ds3Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 3 controller {0}", usbDevice);
            }

            return installed;
        }

        public static uint InstallDualShock3Controllers(IntPtr hWnd = default(IntPtr), bool force = false)
        {
            // install compatible DS3 controllers
            var ds3Drivers = IniConfig.Instance.Ds3Driver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in ds3Drivers.HardwareIds
                let result = WdiWrapper.Instance.InstallLibusbKDriver(hardwareId, ds3Drivers.DeviceGuid, "Driver",
                    string.Format("Ds3Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 3 controller {0}", hardwareId);
            }

            return installed;
        }

        public static uint UninstallDualShock3Controllers(ref bool rebootRequired)
        {
            uint uninstalled = 0;

            if (!Directory.Exists(DriverDirectory))
                return uninstalled;

            foreach (
                var file in
                    Directory.GetFiles(DriverDirectory)
                        .Where(
                            f =>
                                Path.GetFileName(f).StartsWith("Ds3Controller_") &&
                                Path.GetExtension(f).ToLower().Equals(".inf")))
            {
                Difx.Instance.Uninstall(file, DifxFlags.DRIVER_PACKAGE_DELETE_FILES, out rebootRequired);
                uninstalled++;
            }

            return uninstalled;
        }

        public static uint InstallDualShock4Controllers(IEnumerable<WdiUsbDevice> usbDevices,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var ds4Drivers = IniConfig.Instance.Ds4Driver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallLibusbKDriver(usbDevice.HardwareId, ds4Drivers.DeviceGuid,
                    "Driver", string.Format("Ds4Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 4 controller {0}", usbDevice);
            }

            return installed;
        }

        public static uint InstallDualShock4Controllers(IntPtr hWnd = default(IntPtr), bool force = false)
        {
            // install compatible DS4 controllers
            var ds4Drivers = IniConfig.Instance.Ds4Driver;
            uint installed = 0;

            foreach (var hardwareId in from hardwareId in ds4Drivers.HardwareIds
                let result = WdiWrapper.Instance.InstallLibusbKDriver(hardwareId, ds4Drivers.DeviceGuid, "Driver",
                    string.Format("Ds4Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select hardwareId)
            {
                installed++;
                Log.InfoFormat("Installed driver for DualShock 4 controller {0}", hardwareId);
            }

            return installed;
        }

        public static uint UninstallDualShock4Controllers(ref bool rebootRequired)
        {
            uint uninstalled = 0;

            if (!Directory.Exists(DriverDirectory))
                return uninstalled;

            foreach (
                var file in
                    Directory.GetFiles(DriverDirectory)
                        .Where(
                            f =>
                                Path.GetFileName(f).StartsWith("Ds4Controller_") &&
                                Path.GetExtension(f).ToLower().Equals(".inf")))
            {
                Difx.Instance.Uninstall(file, DifxFlags.DRIVER_PACKAGE_DELETE_FILES, out rebootRequired);
                uninstalled++;
            }

            return uninstalled;
        }
    }
}