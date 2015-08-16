using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace ScpControl.Driver
{
    [Flags]
    public enum DifxFlags
    {
        DRIVER_PACKAGE_REPAIR = 0x00000001,
        DRIVER_PACKAGE_SILENT = 0x00000002,
        DRIVER_PACKAGE_FORCE = 0x00000004,
        DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT = 0x00000008,
        DRIVER_PACKAGE_LEGACY_MODE = 0x00000010,
        DRIVER_PACKAGE_DELETE_FILES = 0x00000020
    }

    public enum DifxLog
    {
        DIFXAPI_SUCCESS = 0,
        DIFXAPI_INFO = 1,
        DIFXAPI_WARNING = 2,
        DIFXAPI_ERROR = 3
    }

    /// <summary>
    ///     Driver Install Frameworks API (DIFxAPI)
    /// </summary>
    public class Difx
    {
        public delegate void DIFLOGCALLBACK(
            DifxLog eventType,
            int errorCode,
            [MarshalAs(UnmanagedType.LPTStr)] string eventDescription,
            IntPtr callbackContext
            );

        public delegate void LogEventHandler(DifxLog Event, int Error, string Description);

        private static readonly Lazy<Difx> LazyInstance = new Lazy<Difx>(() => new Difx());
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public LogEventHandler OnLogEvent;
        private DIFLOGCALLBACK _mLogCallback;

        private Difx()
        {
            _mLogCallback = new DIFLOGCALLBACK(Logger);

            Log.Debug("Preparing to load DIFxAPI");

            if (Environment.Is64BitProcess)
            {
                Log.InfoFormat("Running as 64-Bit process");

                var libwdi64 = Path.Combine(WorkingDirectory, @"DIFxApi\amd64\DIFxAPI.dll");
                Log.DebugFormat("DIFxAPI path: {0}", libwdi64);

                LoadLibrary(libwdi64);

                Log.DebugFormat("Loaded library: {0}", libwdi64);
            }
            else
            {
                Log.InfoFormat("Running as 32-Bit process");

                var libwdi32 = Path.Combine(WorkingDirectory, @"DIFxApi\x86\DIFxAPI.dll");
                Log.DebugFormat("DIFxAPI path: {0}", libwdi32);

                LoadLibrary(libwdi32);

                Log.DebugFormat("Loaded library: {0}", libwdi32);
            }

            SetDifxLogCallback(_mLogCallback, IntPtr.Zero);
        }

        public static Difx Instance
        {
            get { return LazyInstance.Value; }
        }

        public void Logger(
            DifxLog eventType,
            int errorCode,
            [MarshalAs(UnmanagedType.LPTStr)] string eventDescription,
            IntPtr callbackContext)
        {
            if (OnLogEvent != null) OnLogEvent(eventType, errorCode, eventDescription);
        }

        public uint Preinstall(string infPath, DifxFlags flags)
        {
            return DriverPackagePreinstall(infPath, (uint) flags);
        }

        public uint Install(string infPath, DifxFlags flags, out bool rebootRequired)
        {
            return DriverPackageInstall(infPath, (uint) flags, (IntPtr) 0, out rebootRequired);
        }

        public uint Uninstall(string infPath, DifxFlags flags, out bool rebootRequired)
        {
            return DriverPackageUninstall(infPath, (uint) flags, (IntPtr) 0, out rebootRequired);
        }

        #region P/Invoke

        [DllImport("DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.Winapi)]
        private static extern uint DriverPackagePreinstall(
            [MarshalAs(UnmanagedType.LPTStr)] string driverPackageInfPath,
            [MarshalAs(UnmanagedType.U4)] uint flags
            );

        [DllImport("DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.Winapi)]
        private static extern uint DriverPackageInstall(
            [MarshalAs(UnmanagedType.LPTStr)] string driverPackageInfPath,
            [MarshalAs(UnmanagedType.U4)] uint flags,
            IntPtr pInstallerInfo,
            [MarshalAs(UnmanagedType.Bool)] out bool pNeedReboot
            );

        [DllImport("DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.Winapi)]
        private static extern uint DriverPackageUninstall(
            [MarshalAs(UnmanagedType.LPTStr)] string driverPackageInfPath,
            [MarshalAs(UnmanagedType.U4)] uint flags,
            IntPtr pInstallerInfo,
            [MarshalAs(UnmanagedType.Bool)] out bool pNeedReboot
            );

        [DllImport("DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SetDifxLogCallback(DIFLOGCALLBACK logCallback, IntPtr callbackContext);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string librayName);

        #endregion
    }
}