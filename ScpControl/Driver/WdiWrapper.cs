using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ScpControl.ScpCore;

namespace ScpControl.Driver
{

    #region Public enums

    public enum WdiErrorCode
    {
        WDI_SUCCESS = 0,
        WDI_ERROR_IO = -1,
        WDI_ERROR_INVALID_PARAM = -2,
        WDI_ERROR_ACCESS = -3,
        WDI_ERROR_NO_DEVICE = -4,
        WDI_ERROR_NOT_FOUND = -5,
        WDI_ERROR_BUSY = -6,
        WDI_ERROR_TIMEOUT = -7,
        WDI_ERROR_OVERFLOW = -8,
        WDI_ERROR_PENDING_INSTALLATION = -9,
        WDI_ERROR_INTERRUPTED = -10,
        WDI_ERROR_RESOURCE = -11,
        WDI_ERROR_NOT_SUPPORTED = -12,
        WDI_ERROR_EXISTS = -13,
        WDI_ERROR_USER_CANCEL = -14,
        WDI_ERROR_NEEDS_ADMIN = -15,
        WDI_ERROR_WOW64 = -16,
        WDI_ERROR_INF_SYNTAX = -17,
        WDI_ERROR_CAT_MISSING = -18,
        WDI_ERROR_UNSIGNED = -19,
        WDI_ERROR_OTHER = -99
    }

    public enum WdiLogLevel
    {
        WDI_LOG_LEVEL_DEBUG,
        WDI_LOG_LEVEL_INFO,
        WDI_LOG_LEVEL_WARNING,
        WDI_LOG_LEVEL_ERROR,
        WDI_LOG_LEVEL_NONE
    }

    #endregion

    /// <summary>
    ///     Managed wrapper class for <see href="https://github.com/pbatard/libwdi">libwdi</see>.
    /// </summary>
    public class WdiWrapper : NativeLibraryWrapper<WdiWrapper>
    {
        #region Ctor

        /// <summary>
        ///     Automatically loads the correct native library.
        /// </summary>
        private WdiWrapper()
        {
            LoadNativeLibrary("libwdi", @"libwdi\x86\libwdi.dll", @"libwdi\amd64\libwdi.dll");
        }

        #endregion

        #region Public properties

        public IEnumerable<WdiDeviceInfo> UsbDeviceList
        {
            get
            {
                var wdiDevices = new List<WdiDeviceInfo>();

                // pointer to write device list to
                var pList = IntPtr.Zero;
                // list all Usb devices, not only driverless ones
                var listOpts = new wdi_options_create_list
                {
                    list_all = true,
                    list_hubs = false,
                    trim_whitespaces = true
                };

                // receive Usb device list
                wdi_create_list(ref pList, ref listOpts);
                // save original pointer to free list
                var devices = pList;

                // loop through linked list until last element
                while (pList != IntPtr.Zero)
                {
                    // translate device info to managed object
                    var info = (wdi_device_info)Marshal.PtrToStructure(pList, typeof(wdi_device_info));

                    var wdiDevice = NativeToManagedWdiUsbDevice(info);

                    wdiDevices.Add(wdiDevice);

                    // continue with next device
                    pList = info.next;
                }

                // free used memory
                wdi_destroy_list(devices);

                return wdiDevices;
            }
        }

        #endregion

        #region Private methods

        private static WdiDeviceInfo NativeToManagedWdiUsbDevice(wdi_device_info info)
        {
            // get raw bytes from description pointer
            var descSize = 0;
            while (Marshal.ReadByte(info.desc, descSize) != 0) ++descSize;
            var descBytes = new byte[descSize];
            Marshal.Copy(info.desc, descBytes, 0, descSize);

            // put info in managed object
            var wdiDevice = new WdiDeviceInfo
            {
                VendorId = info.vid,
                ProductId = info.pid,
                InterfaceId = (byte)info.mi,
                Description = Encoding.UTF8.GetString(descBytes),
                DeviceId = info.device_id,
                HardwareId = info.hardware_id,
                CurrentDriver = Marshal.PtrToStringAnsi(info.driver)
            };

            return wdiDevice;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The Usb driver solution to install.
        /// </summary>
        private enum WdiDriverType
        {
            [Description("WinUSB")]
            WDI_WINUSB,
            WDI_LIBUSB0,
            [Description("libusbK")]
            WDI_LIBUSBK,
            WDI_USER,
            WDI_NB_DRIVERS
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Equipes a given device with the WinUSB driver.
        /// </summary>
        /// <param name="device">The device to perform the driver installation on.</param>
        /// <param name="deviceGuid">The device class GUID of the driver.</param>
        /// <param name="driverPath">The filesystem path to extract the driver and helper files to.</param>
        /// <param name="infName">The name of the *.INF file to create.</param>
        /// <param name="hwnd">The handle of the parent window to relate the progress dialog to.</param>
        /// <returns>The error code (0 if succeeded).</returns>
        public static WdiErrorCode InstallWinUsbDriver(WdiDeviceInfo device, Guid deviceGuid, string driverPath,
            string infName, IntPtr hwnd = default(IntPtr))
        {
            // build CLI args
            var cliArgs = new StringBuilder();
            switch (device.DeviceType)
            {
                case WdiUsbDeviceType.BluetoothHost:
                    cliArgs.AppendFormat("--name \"Bluetooth Host (ScpToolkit)\" ");
                    break;
                case WdiUsbDeviceType.DualShock3:
                    cliArgs.AppendFormat("--name \"DualShock 3 Controller (ScpToolkit)\" ");
                    break;
                case WdiUsbDeviceType.DualShock4:
                    cliArgs.AppendFormat("--name \"DualShock 4 Controller (ScpToolkit)\" ");
                    break;
            }
            cliArgs.AppendFormat("--inf \"{0}\" ", infName);
            cliArgs.AppendFormat("--manufacturer \"ScpToolkit compatible device\" ");
            cliArgs.AppendFormat("--vid 0x{0:X4} --pid 0x{1:X4} ", device.VendorId, device.ProductId);
            cliArgs.AppendFormat("--type 0 ");
            cliArgs.AppendFormat("--dest \"{0}\" ", driverPath);
            cliArgs.AppendFormat("--stealth-cert ");
            if (hwnd != default(IntPtr)) cliArgs.AppendFormat("--progressbar={0:D} ", hwnd.ToInt64());
            cliArgs.AppendFormat("--timeout 120000 ");
            cliArgs.AppendFormat("--device-guid \"{0}\" ", deviceGuid.ToString("B"));

            // build path to install helper
            var wdiSimplePath = Path.Combine(GlobalConfiguration.AppDirectory, "libwdi",
                Environment.Is64BitProcess ? "amd64" : "x86", "wdi-simple.exe");

            // set-up installer process
            var wdiProc = new Process
            {
                StartInfo = new ProcessStartInfo(wdiSimplePath, cliArgs.ToString())
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // start & wait
            wdiProc.Start();
            wdiProc.WaitForExit();

            // return code of application is possible error code
            return (WdiErrorCode)wdiProc.ExitCode;
        }

        public string GetErrorMessage(WdiErrorCode errcode)
        {
            var msgPtr = wdi_strerror((int)errcode);
            return Marshal.PtrToStringAnsi(msgPtr);
        }

        public string GetVendorName(ushort vendorId)
        {
            var namePtr = wdi_get_vendor_name(vendorId);
            return Marshal.PtrToStringAnsi(namePtr);
        }

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct wdi_device_info
        {
            public readonly IntPtr next;
            public readonly ushort vid;
            public readonly ushort pid;
            public readonly bool is_composite;
            public readonly char mi;
            public readonly IntPtr desc;
            public readonly IntPtr driver;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string device_id;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string hardware_id;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string compatible_id;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string upper_filter;
            public readonly ulong driver_version;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_create_list
        {
            public bool list_all;
            public bool list_hubs;
            public bool trim_whitespaces;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_prepare_driver
        {
            [MarshalAs(UnmanagedType.I4)]
            public readonly WdiDriverType driver_type;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string vendor_name;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string device_guid;
            public readonly bool disable_cat;
            public readonly bool disable_signing;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string cert_subject;
            public readonly bool use_wcid_driver;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_install_driver
        {
            public readonly IntPtr hWnd;
            public readonly bool install_filter_driver;
            public readonly uint pending_install_timeout;
        }

        #endregion

        #region P/Invoke

        [DllImport("libwdi.dll", EntryPoint = "wdi_strerror", ExactSpelling = false)]
        private static extern IntPtr wdi_strerror(int errcode);

        [DllImport("libwdi.dll", EntryPoint = "wdi_get_vendor_name", ExactSpelling = false)]
        private static extern IntPtr wdi_get_vendor_name(ushort vid);

        [DllImport("libwdi.dll", EntryPoint = "wdi_create_list", ExactSpelling = false)]
        private static extern int wdi_create_list(ref IntPtr list,
            ref wdi_options_create_list options);

        [DllImport("libwdi.dll", EntryPoint = "wdi_destroy_list", ExactSpelling = false)]
        private static extern WdiErrorCode wdi_destroy_list(IntPtr list);

        #endregion
    }

    /// <summary>
    ///     Managed wrapper for Usb device properties.
    /// </summary>
    public class WdiDeviceInfo
    {
        public ushort VendorId { get; set; }
        public ushort ProductId { get; set; }
        public byte InterfaceId { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public string HardwareId { get; set; }
        public string CurrentDriver { get; set; }
        public WdiUsbDeviceType DeviceType { get; set; }
        public string InfFile { get; set; }

        public override string ToString()
        {
            return string.Format("{0} (VID: {1:X4}, PID: {2:X4})", Description, VendorId, ProductId);
        }
    }

    public enum WdiUsbDeviceType
    {
        Unknown,
        BluetoothHost,
        DualShock3,
        DualShock4
    }
}