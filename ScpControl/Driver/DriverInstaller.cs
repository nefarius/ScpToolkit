using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using ScpControl.Bluetooth;
using ScpControl.Database;
using ScpControl.ScpCore;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;

namespace ScpControl.Driver
{
    /// <summary>
    ///     Automated Windows driver (un)installer utility class.
    /// </summary>
    public static class DriverInstaller
    {
        #region Private static fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string DriverDirectory = Path.Combine(GlobalConfiguration.AppDirectory, "Driver");

        #endregion

        public static WdiErrorCode InstallBluetoothHost(WdiDeviceInfo usbDevice, IntPtr hWnd = default(IntPtr))
        {
            usbDevice.InfFile = string.Format("BluetoothHost_{0:X4}_{1:X4}.inf", usbDevice.VendorId, usbDevice.ProductId);
            usbDevice.DeviceType = WdiUsbDeviceType.BluetoothHost;

            var result = WdiWrapper.InstallWinUsbDriver(usbDevice, BthDongle.DeviceClassGuid, DriverDirectory,
                usbDevice.InfFile, hWnd);

            if (result != WdiErrorCode.WDI_SUCCESS)
            {
                Log.ErrorFormat("Installing Bluetooth Host ({0}) failed: {1}", usbDevice.DeviceId, result);
                return result;
            }
            
            using (var db = new ScpDb())
            {
                db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
            }

            return result;
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

        public static WdiErrorCode InstallDualShock3Controller(WdiDeviceInfo usbDevice, IntPtr hWnd = default(IntPtr))
        {
            usbDevice.InfFile = string.Format("Ds3Controller_{0:X4}_{1:X4}.inf", usbDevice.VendorId, usbDevice.ProductId);
            usbDevice.DeviceType = WdiUsbDeviceType.DualShock3;

            var result = WdiWrapper.InstallWinUsbDriver(usbDevice, UsbDs3.DeviceClassGuid,
                DriverDirectory, usbDevice.InfFile, hWnd);

            if (result != WdiErrorCode.WDI_SUCCESS)
            {
                Log.ErrorFormat("Installing DualShock 3 Controller ({0}) failed: {1}", usbDevice.DeviceId, result);
                return result;
            }

            
            using (var db = new ScpDb())
            {
                db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
            }

            return result;
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
        
        public static WdiErrorCode InstallDualShock4Controller(WdiDeviceInfo usbDevice, IntPtr hWnd = default(IntPtr))
        {
            usbDevice.InfFile = string.Format("Ds4Controller_{0:X4}_{1:X4}.inf", usbDevice.VendorId, usbDevice.ProductId);
            usbDevice.DeviceType = WdiUsbDeviceType.DualShock4;

            var result = WdiWrapper.InstallWinUsbDriver(usbDevice, UsbDs4.DeviceClassGuid, DriverDirectory, usbDevice.InfFile, hWnd);

            if (result != WdiErrorCode.WDI_SUCCESS)
            {
                Log.ErrorFormat("Installing DualShock 4 Controller ({0}) failed: {1}", usbDevice.DeviceId, result);
                return result;
            }
            
            using (var db = new ScpDb())
            {
                db.Engine.PutDbEntity(ScpDb.TableDevices, usbDevice.DeviceId, usbDevice);
            }

            return result;
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
