using System;
using System.Runtime.InteropServices;

namespace ScpDriver 
{
    [Flags]
    public enum DifxFlags 
    {
        DRIVER_PACKAGE_REPAIR                 = 0x00000001,
        DRIVER_PACKAGE_SILENT                 = 0x00000002,
        DRIVER_PACKAGE_FORCE                  = 0x00000004,
        DRIVER_PACKAGE_ONLY_IF_DEVICE_PRESENT = 0x00000008,
        DRIVER_PACKAGE_LEGACY_MODE            = 0x00000010,
        DRIVER_PACKAGE_DELETE_FILES           = 0x00000020,
    }

    public enum DifxLog 
    {
        DIFXAPI_SUCCESS = 0,
        DIFXAPI_INFO    = 1,
        DIFXAPI_WARNING = 2,
        DIFXAPI_ERROR   = 3,
    }

    public class Difx 
    {
        public delegate void DIFLOGCALLBACK(
                    DifxLog EventType, 
                    Int32 ErrorCode, 
                    [MarshalAs(UnmanagedType.LPTStr)] String EventDescription, 
                    IntPtr CallbackContext
                );

        public void Logger(
                    DifxLog EventType, 
                    Int32 ErrorCode,
                    [MarshalAs(UnmanagedType.LPTStr)] String EventDescription,
                    IntPtr CallbackContext) 
        {
            if (onLogEvent != null) onLogEvent(EventType, ErrorCode, EventDescription);
        }

        protected DIFLOGCALLBACK m_LogCallback;

        public delegate void LogEventHandler(DifxLog Event, Int32 Error, String Description);
        public LogEventHandler onLogEvent;

        protected Difx() 
        {
            m_LogCallback = new DIFLOGCALLBACK(Logger);
        }

        public virtual UInt32 Preinstall(String InfPath, DifxFlags Flags) 
        {
            return 0xFFFFFFFF;
        }

        public virtual UInt32 Install(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            RebootRequired = false;
            return 0xFFFFFFFF;
        }

        public virtual UInt32 Uninstall(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            RebootRequired = false;
            return 0xFFFFFFFF;
        }

        public static Difx Factory() 
        {
            Difx RetVal = null;

            if (Environment.Is64BitProcess)
            {
                RetVal = new Difx_64();
            }
            else
            {
                RetVal = new Difx_32();
            }

            return RetVal;
        }
    }

    public class Difx_32 : Difx 
    {
        [DllImport(@".\DIFxApi\x86\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackagePreinstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags
            );

        [DllImport(@".\DIFxApi\x86\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackageInstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags,
                IntPtr pInstallerInfo,
                [MarshalAs(UnmanagedType.Bool)] out Boolean pNeedReboot
            );

        [DllImport(@".\DIFxApi\x86\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackageUninstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags,
                IntPtr pInstallerInfo,
                [MarshalAs(UnmanagedType.Bool)] out Boolean pNeedReboot
            );

        [DllImport(@".\DIFxApi\x86\DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SetDifxLogCallback(DIFLOGCALLBACK LogCallback, IntPtr CallbackContext);

        public Difx_32() 
        {
            SetDifxLogCallback(m_LogCallback, IntPtr.Zero);
        }

        public override UInt32 Preinstall(String InfPath, DifxFlags Flags) 
        {
            return DriverPackagePreinstall(InfPath, (UInt32) Flags);
        }

        public override UInt32 Install(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            return DriverPackageInstall(InfPath, (UInt32) Flags, IntPtr.Zero, out RebootRequired);
        }

        public override UInt32 Uninstall(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            return DriverPackageUninstall(InfPath, (UInt32) Flags, IntPtr.Zero, out RebootRequired);
        }
    }

    public class Difx_64 : Difx 
    {
        [DllImport(@".\DIFxApi\amd64\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackagePreinstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags
            );

        [DllImport(@".\DIFxApi\amd64\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackageInstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags,
                IntPtr pInstallerInfo,
                [MarshalAs(UnmanagedType.Bool)] out Boolean pNeedReboot
            );

        [DllImport(@".\DIFxApi\amd64\DIFxAPI.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 DriverPackageUninstall(
                [MarshalAs(UnmanagedType.LPTStr)] String DriverPackageInfPath,
                [MarshalAs(UnmanagedType.U4)] UInt32 Flags,
                IntPtr pInstallerInfo,
                [MarshalAs(UnmanagedType.Bool)] out Boolean pNeedReboot
            );

        [DllImport(@".\DIFxApi\amd64\DIFxAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SetDifxLogCallback(DIFLOGCALLBACK LogCallback, IntPtr CallbackContext);

                
        public Difx_64() 
        {
            SetDifxLogCallback(m_LogCallback, IntPtr.Zero);
        }

        public override UInt32 Preinstall(String InfPath, DifxFlags Flags) 
        {
            return DriverPackagePreinstall(InfPath, (UInt32) Flags);
        }

        public override UInt32 Install(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            return DriverPackageInstall(InfPath, (UInt32) Flags, (IntPtr) 0, out RebootRequired);
        }

        public override UInt32 Uninstall(String InfPath, DifxFlags Flags, out Boolean RebootRequired) 
        {
            return DriverPackageUninstall(InfPath, (UInt32) Flags, (IntPtr) 0, out RebootRequired);
        }
    }
}
