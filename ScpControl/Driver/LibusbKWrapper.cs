using System;
using System.Runtime.InteropServices;

namespace ScpControl.Driver
{
    public enum UsbKPowerPolicy : uint
    {
        AutoSuspend = 0x81,
        SuspendDelay = 0x83
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KISO_PACKET
    {
        public uint Offset;
        public ushort Length;
        public ushort Status;
    }

    public enum KISO_FLAG
    {
        KISO_FLAG_SET_START_FRAME
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KISO_CONTEXT
    {
        public KISO_FLAG Flags;
        public uint StartFrame;
        public short ErrorCount;
        public short NumberOfPackets;
        public uint UrbHdrStatus;
        public KISO_PACKET IsoPackets;
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

            try
            {
                Marshal.WriteByte(value, (byte) ((on) ? 0x01 : 0x00));

                return SetPowerPolicy(handle, UsbKPowerPolicy.AutoSuspend,
                    (uint) Marshal.SizeOf(typeof (byte)), value);
            }
            finally
            {
                Marshal.FreeHGlobal(value);
            }
        }

        private bool GetPowerPolicy(IntPtr InterfaceHandle, UsbKPowerPolicy PolicyType, ref uint ValueLength,
            IntPtr Value)
        {
            return UsbK_GetPowerPolicy(InterfaceHandle, PolicyType, ref ValueLength, Value);
        }

        private bool SetPowerPolicy(IntPtr InterfaceHandle, UsbKPowerPolicy PolicyType, uint ValueLength,
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

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool IsoK_Init(IntPtr IsoContext, int NumberOfPackets, int StartFrame);
    }
}