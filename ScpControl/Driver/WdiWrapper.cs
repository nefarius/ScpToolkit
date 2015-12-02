using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ScpControl.Utilities;

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

#if DEBUG
            wdi_set_log_level(WdiLogLevel.WDI_LOG_LEVEL_DEBUG);
#else
            wdi_set_log_level(WdiLogLevel.WDI_LOG_LEVEL_INFO);
#endif
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
                Description = Encoding.UTF8.GetString(descBytes),
                DeviceId = info.device_id,
                HardwareId = info.hardware_id,
                CurrentDriver = Marshal.PtrToStringAnsi(info.driver)
            };

            return wdiDevice;
        }

        private static WdiErrorCode InstallDeviceDriver(string hardwareId, string deviceGuid, string driverPath,
            string infName,
            IntPtr hwnd, bool force, WdiDriverType driverType)
        {
            // regex to extract vendor ID and product ID from hardware ID string
            var regex = new Regex("VID_([0-9A-Z]{4})&PID_([0-9A-Z]{4})", RegexOptions.IgnoreCase);
            // matched groups
            var matches = regex.Match(hardwareId).Groups;

            // very basic check
            if (matches.Count < 3)
                throw new ArgumentException("Supplied Hardware-ID is malformed");

            // get values
            var vid = matches[1].Value.ToUpper();
            var pid = matches[2].Value.ToUpper();

            // default return value is no matching device found
            var result = WdiErrorCode.WDI_ERROR_NO_DEVICE;
            // pointer to write device list to
            var pList = IntPtr.Zero;
            // list all USB devices, not only driverless ones
            var listOpts = new wdi_options_create_list
            {
                list_all = true,
                list_hubs = false,
                trim_whitespaces = false
            };

            // use WinUSB and overrride device GUID
            var prepOpts = new wdi_options_prepare_driver
            {
                driver_type = driverType,
                device_guid = deviceGuid,
                vendor_name = "ScpToolkit compatible device"
            };

            // set parent window handle (may be IntPtr.Zero)
            var intOpts = new wdi_options_install_driver {hWnd = hwnd};

            // receive USB device list
            wdi_create_list(ref pList, ref listOpts);
            // save original pointer to free list
            var devices = pList;

            // loop through linked list until last element
            while (pList != IntPtr.Zero)
            {
                // translate device info to managed object
                var info = (wdi_device_info) Marshal.PtrToStructure(pList, typeof (wdi_device_info));
                var deviceInfo = NativeToManagedWdiUsbDevice(info);

                // extract VID and PID
                var currentMatches = regex.Match(deviceInfo.HardwareId).Groups;
                var currentVid = currentMatches[1].Value.ToUpper();
                var currentPid = currentMatches[2].Value.ToUpper();

                // does the HID of the current device match the desired HID
                if (vid == currentVid && pid == currentPid)
                {
                    var driverName = driverType.ToDescription();

                    // skip installation if device is currently using the desired driver
                    if (string.CompareOrdinal(deviceInfo.CurrentDriver, driverName) == 0 && !force)
                    {
                        result = WdiErrorCode.WDI_ERROR_EXISTS;
                        Log.WarnFormat("Device \"{0}\" ({1}) is already using {2}, installation aborted",
                            deviceInfo.Description,
                            hardwareId, driverName);
                        break;
                    }

                    Log.InfoFormat(
                        "Device with specified VID ({0}) and PID ({1}) found, preparing driver installation...",
                        vid, pid);

                    // prepare driver installation (generates the signed driver and installation helpers)
                    if ((result = wdi_prepare_driver(pList, driverPath, infName, ref prepOpts)) ==
                        WdiErrorCode.WDI_SUCCESS)
                    {
                        Log.InfoFormat("Driver \"{0}\" successfully created in directory \"{1}\"", infName, driverPath);

                        // install/replace the current devices driver
                        result = wdi_install_driver(pList, driverPath, infName, ref intOpts);

                        var resultLog = string.Format("Installation result: {0}",
                            Enum.GetName(typeof (WdiErrorCode), result));

                        if (result == WdiErrorCode.WDI_SUCCESS)
                        {
                            Log.Info(resultLog);
                        }
                        else
                        {
                            Log.Warn(resultLog);
                        }
                    }

                    break;
                }

                // continue with next device
                pList = info.next;
            }

            // free used memory
            wdi_destroy_list(devices);

            return result;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The USB driver solution to install.
        /// </summary>
        private enum WdiDriverType
        {
            [Description("WinUSB")] WDI_WINUSB,
            WDI_LIBUSB0,
            [Description("libusbK")] WDI_LIBUSBK,
            WDI_USER,
            WDI_NB_DRIVERS
        }

        #endregion

        #region Public properties

        public static uint WmLibwdiLogger
        {
            get { return 0x0400 + 1; }
        }

        public int WdfVersion
        {
            get { return wdi_get_wdf_version(); }
        }

        public IEnumerable<WdiDeviceInfo> UsbDeviceList
        {
            get
            {
                var wdiDevices = new List<WdiDeviceInfo>();

                // pointer to write device list to
                var pList = IntPtr.Zero;
                // list all USB devices, not only driverless ones
                var listOpts = new wdi_options_create_list
                {
                    list_all = true,
                    list_hubs = false,
                    trim_whitespaces = false
                };

                // receive USB device list
                wdi_create_list(ref pList, ref listOpts);
                // save original pointer to free list
                var devices = pList;

                // loop through linked list until last element
                while (pList != IntPtr.Zero)
                {
                    // translate device info to managed object
                    var info = (wdi_device_info) Marshal.PtrToStructure(pList, typeof (wdi_device_info));

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

        #region Public methods

        /// <summary>
        ///     Replaces the device driver of given device with WinUSB.
        /// </summary>
        /// <param name="hardwareId">Hardware-ID of the device to change the driver for.</param>
        /// <param name="deviceGuid">Device-GUID (with brackets) to register device driver with.</param>
        /// <param name="driverPath">Temporary path for driver auto-creation.</param>
        /// <param name="infName">Temporary .INF-name for driver auto-creation.</param>
        /// <param name="hwnd">Optional window handle to display installation progress dialog on.</param>
        /// <param name="force">Force driver installation even if the device is already using WinUSB.</param>
        /// <returns>The error code returned by libwdi.</returns>
        public WdiErrorCode InstallWinUsbDriver(string hardwareId, string deviceGuid, string driverPath, string infName,
            IntPtr hwnd, bool force = false)
        {
            return InstallDeviceDriver(hardwareId, deviceGuid, driverPath, infName, hwnd, force,
                WdiDriverType.WDI_WINUSB);
        }

        /// <summary>
        ///     Replaces the device driver of given device with libusbK.
        /// </summary>
        /// <param name="hardwareId">Hardware-ID of the device to change the driver for.</param>
        /// <param name="deviceGuid">Device-GUID (with brackets) to register device driver with.</param>
        /// <param name="driverPath">Temporary path for driver auto-creation.</param>
        /// <param name="infName">Temporary .INF-name for driver auto-creation.</param>
        /// <param name="hwnd">Optional window handle to display installation progress dialog on.</param>
        /// <param name="force">Force driver installation even if the device is already using libusbK.</param>
        /// <returns>The error code returned by libwdi.</returns>
        public WdiErrorCode InstallLibusbKDriver(string hardwareId, Guid deviceGuid, string driverPath, string infName,
            IntPtr hwnd, bool force = false)
        {
            return InstallDeviceDriver(hardwareId, deviceGuid.ToString("B"), driverPath, infName, hwnd, force,
                WdiDriverType.WDI_LIBUSBK);
        }

        /// <summary>
        ///     Replaces the device driver of given device with libusbK.
        /// </summary>
        /// <param name="hardwareId">Hardware-ID of the device to change the driver for.</param>
        /// <param name="deviceGuid">Device-GUID (with brackets) to register device driver with.</param>
        /// <param name="driverPath">Temporary path for driver auto-creation.</param>
        /// <param name="infName">Temporary .INF-name for driver auto-creation.</param>
        /// <param name="hwnd">Optional window handle to display installation progress dialog on.</param>
        /// <param name="force">Force driver installation even if the device is already using libusbK.</param>
        /// <returns>The error code returned by libwdi.</returns>
        public WdiErrorCode InstallLibusbKDriver(string hardwareId, string deviceGuid, string driverPath, string infName,
            IntPtr hwnd, bool force = false)
        {
            return InstallDeviceDriver(hardwareId, deviceGuid, driverPath, infName, hwnd, force,
                WdiDriverType.WDI_LIBUSBK);
        }

        public string GetErrorMessage(WdiErrorCode errcode)
        {
            var msgPtr = wdi_strerror((int) errcode);
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
            [MarshalAs(UnmanagedType.LPStr)] public readonly string device_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string hardware_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string compatible_id;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string upper_filter;
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
            [MarshalAs(UnmanagedType.I4)] public WdiDriverType driver_type;
            [MarshalAs(UnmanagedType.LPStr)] public string vendor_name;
            [MarshalAs(UnmanagedType.LPStr)] public string device_guid;
            public readonly bool disable_cat;
            public readonly bool disable_signing;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string cert_subject;
            public readonly bool use_wcid_driver;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct wdi_options_install_driver
        {
            public IntPtr hWnd;
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

        [DllImport("libwdi.dll", EntryPoint = "wdi_prepare_driver", ExactSpelling = false)]
        private static extern WdiErrorCode wdi_prepare_driver(IntPtr device_info,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] string inf_name,
            ref wdi_options_prepare_driver options);

        [DllImport("libwdi.dll", EntryPoint = "wdi_install_driver", ExactSpelling = false)]
        private static extern WdiErrorCode wdi_install_driver(IntPtr device_info,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            [MarshalAs(UnmanagedType.LPStr)] string inf_name,
            ref wdi_options_install_driver options);

        [DllImport("libwdi.dll", EntryPoint = "wdi_destroy_list", ExactSpelling = false)]
        private static extern WdiErrorCode wdi_destroy_list(IntPtr list);

        [DllImport("libwdi.dll", EntryPoint = "wdi_get_wdf_version", ExactSpelling = false)]
        private static extern int wdi_get_wdf_version();

        [DllImport("libwdi.dll", EntryPoint = "wdi_set_log_level", ExactSpelling = false)]
        private static extern int wdi_set_log_level(WdiLogLevel level);

        [DllImport("libwdi.dll", EntryPoint = "wdi_register_logger", ExactSpelling = false)]
        private static extern int wdi_register_logger(IntPtr hWnd, uint message, uint buffsize);

        [DllImport("libwdi.dll", EntryPoint = "wdi_read_logger", ExactSpelling = false)]
        private static extern int wdi_read_logger(IntPtr buffer, uint buffer_size,
            ref uint message_size);

        [DllImport("libwdi.dll", EntryPoint = "wdi_unregister_logger", ExactSpelling = false)]
        private static extern int wdi_unregister_logger(IntPtr hWnd);

        #endregion
    }

    /// <summary>
    ///     Managed wrapper for USB device properties.
    /// </summary>
    public class WdiDeviceInfo
    {
        public ushort VendorId { get; set; }
        public ushort ProductId { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public string HardwareId { get; set; }
        public string CurrentDriver { get; set; }
        public WdiUsbDeviceType DeviceType { get; set; }

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
        DualSHock4
    }
}