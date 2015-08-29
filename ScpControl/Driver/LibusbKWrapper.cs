using System;
using System.Runtime.InteropServices;

namespace ScpControl.Driver
{
    public enum UsbKPowerPolicy : uint
    {
        AutoSuspend = 0x81,
        SuspendDelay = 0x83
    }

    public class LibusbKWrapper : NativeLibraryWrapper<LibusbKWrapper>
    {
        /// <summary>
        ///     Automatically loads the correct native library.
        /// </summary>
        private LibusbKWrapper()
        {
            LoadNativeLibrary("libusbK", @"libusbK\x86\libusbK.dll", @"libusbK\amd64\libusbK.dll");
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