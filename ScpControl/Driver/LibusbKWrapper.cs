using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace ScpControl.Driver
{
    public enum UsbKPowerPolicy : uint
    {
        AutoSuspend = 0x81,
        SuspendDelay = 0x83
    }

    public class LibusbKWrapper
    {
        private static readonly Lazy<LibusbKWrapper> LazyInstance = new Lazy<LibusbKWrapper>(() => new LibusbKWrapper());
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Automatically loads the correct native library.
        /// </summary>
        private LibusbKWrapper()
        {
            Log.Debug("Preparing to load libusbK");

            if (Environment.Is64BitProcess)
            {
                Log.InfoFormat("Running as 64-Bit process");

                var libwdi64 = Path.Combine(WorkingDirectory, @"libusbK\amd64\libusbK.dll");
                Log.DebugFormat("libusbK path: {0}", libwdi64);

                LoadLibrary(libwdi64);

                Log.DebugFormat("Loaded library: {0}", libwdi64);
            }
            else
            {
                Log.InfoFormat("Running as 32-Bit process");

                var libwdi32 = Path.Combine(WorkingDirectory, @"libusbK\x86\libusbK.dll");
                Log.DebugFormat("libusbK path: {0}", libwdi32);

                LoadLibrary(libwdi32);

                Log.DebugFormat("Loaded library: {0}", libwdi32);
            }
        }

        public static LibusbKWrapper Instance
        {
            get { return LazyInstance.Value; }
        }

        public bool SetPowerPolicyAutoSuspend(IntPtr handle, bool on = true)
        {
            var value = Marshal.AllocHGlobal(1);

            Marshal.WriteByte(value, (byte) ((on) ? 0x01 : 0x00));

            var retval = SetPowerPolicy(handle, UsbKPowerPolicy.AutoSuspend,
                1, value);

            Marshal.FreeHGlobal(value);

            return retval;
        }

        public bool GetPowerPolicy(IntPtr InterfaceHandle, UsbKPowerPolicy PolicyType, ref uint ValueLength,
            IntPtr Value)
        {
            return UsbK_GetPowerPolicy(InterfaceHandle, PolicyType, ref ValueLength, Value);
        }

        public bool SetPowerPolicy(IntPtr InterfaceHandle, UsbKPowerPolicy PolicyType, uint ValueLength,
            IntPtr Value)
        {
            return UsbK_SetPowerPolicy(InterfaceHandle, PolicyType, ValueLength, Value);
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string librayName);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_GetPowerPolicy(IntPtr InterfaceHandle,
            [MarshalAs(UnmanagedType.U4)] UsbKPowerPolicy PolicyType, ref uint ValueLength,
            IntPtr Value);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_SetPowerPolicy(IntPtr InterfaceHandle,
            [MarshalAs(UnmanagedType.U4)] UsbKPowerPolicy PolicyType, uint ValueLength,
            IntPtr Value);
    }
}