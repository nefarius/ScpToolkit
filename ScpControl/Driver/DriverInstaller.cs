using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using ScpControl.Database;
using ScpControl.ScpCore;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;
using ScpControl.Utilities;

namespace ScpControl.Driver
{
    /// <summary>
    ///     Automated Windows driver (un)installer utility class.
    /// </summary>
    public static class DriverInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string DriverDirectory = Path.Combine(GlobalConfiguration.AppDirectory, "Driver");

        public static uint InstallBluetoothDongles(IEnumerable<WdiDeviceInfo> usbDevices, IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var bthDrivers = IniConfig.Instance.BthDongleDriver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallLibusbKDriver(usbDevice.DeviceId, bthDrivers.DeviceGuid,
                    DriverDirectory, string.Format("BthDongle_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                usbDevice.DeviceType = WdiUsbDeviceType.BluetoothHost;
                using (var db = new ScpDb())
                {
                    db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
                }

                installed++;
                Log.InfoFormat("Installed driver for Bluetooth dongle {0}", usbDevice);
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

        public static bool InstallDualShock3Controller(WdiDeviceInfo usbDevice,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            usbDevice.InfFile = string.Format("Ds3Controller_{0}.inf", Guid.NewGuid());

            var result = WdiWrapper.Instance.InstallWinUsbDriver(usbDevice.DeviceId, UsbDs3.DeviceClassGuid,
                DriverDirectory, usbDevice.InfFile, hWnd, force);

            if (result != WdiErrorCode.WDI_SUCCESS)
            {
                Log.ErrorFormat("Installing DualShock 3 Controller ({0}) failed: {1}", usbDevice.DeviceId, result);
                return false;
            }

            usbDevice.DeviceType = WdiUsbDeviceType.DualShock3;
            using (var db = new ScpDb())
            {
                db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
            }

            return true;
        }

        public static uint InstallDualShock3Controllers(IEnumerable<WdiDeviceInfo> usbDevices,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var ds3Drivers = IniConfig.Instance.Ds3Driver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallWinUsbDriver(usbDevice.DeviceId, ds3Drivers.DeviceGuid,
                    DriverDirectory, string.Format("Ds3Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                usbDevice.DeviceType = WdiUsbDeviceType.DualShock3;
                using (var db = new ScpDb())
                {
                    db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
                }

                installed++;
                Log.InfoFormat("Installed driver for DualShock 3 controller {0}", usbDevice);
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

        public static bool InstallDualShock4Controller(WdiDeviceInfo usbDevice,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            usbDevice.InfFile = string.Format("Ds4Controller_{0}.inf", Guid.NewGuid());

            var result = WdiWrapper.Instance.InstallWinUsbDriver(usbDevice.DeviceId, UsbDs4.DeviceClassGuid,
                DriverDirectory, usbDevice.InfFile, hWnd, force);

            if (result != WdiErrorCode.WDI_SUCCESS)
            {
                Log.ErrorFormat("Installing DualShock 4 Controller ({0}) failed: {1}", usbDevice.DeviceId, result);
                return false;
            }

            usbDevice.DeviceType = WdiUsbDeviceType.DualShock4;
            using (var db = new ScpDb())
            {
                db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
            }

            return true;
        }

        public static uint InstallDualShock4Controllers(IEnumerable<WdiDeviceInfo> usbDevices,
            IntPtr hWnd = default(IntPtr),
            bool force = false)
        {
            // install compatible bluetooth dongles
            var ds4Drivers = IniConfig.Instance.Ds4Driver;
            uint installed = 0;

            foreach (var usbDevice in from usbDevice in usbDevices
                let result = WdiWrapper.Instance.InstallLibusbKDriver(usbDevice.DeviceId, ds4Drivers.DeviceGuid,
                    DriverDirectory, string.Format("Ds4Controller_{0}.inf", Guid.NewGuid()), hWnd, force)
                where result == WdiErrorCode.WDI_SUCCESS
                select usbDevice)
            {
                usbDevice.DeviceType = WdiUsbDeviceType.DualShock4;
                using (var db = new ScpDb())
                {
                    db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
                }

                installed++;
                Log.InfoFormat("Installed driver for DualShock 4 controller {0}", usbDevice);
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