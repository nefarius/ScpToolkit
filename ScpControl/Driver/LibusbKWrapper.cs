using System;
using System.Runtime.InteropServices;

namespace ScpControl.Driver
{
    using KOVL_HANDLE = IntPtr;
    using KLIB_HANDLE = IntPtr;
    using KOVL_POOL_HANDLE = IntPtr;
    using KUSB_HANDLE = IntPtr;
    using HANDLE = IntPtr;

    public enum UsbKPowerPolicy : uint
    {
        AutoSuspend = 0x81,
        SuspendDelay = 0x83
    }

    public enum UsbKPipePolicy : uint
    {
        SHORT_PACKET_TERMINATE = 0x01,
        AUTO_CLEAR_STALL = 0x02,
        PIPE_TRANSFER_TIMEOUT = 0x03,
        IGNORE_SHORT_PACKETS = 0x04,
        ALLOW_PARTIAL_READS = 0x05,
        AUTO_FLUSH = 0x06,
        RAW_IO = 0x07,
        MAXIMUM_TRANSFER_SIZE = 0x08,
        RESET_PIPE_ON_RESUME = 0x09
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

    public enum KOVL_WAIT_FLAG
    {
        KOVL_WAIT_FLAG_NONE = 0x0000,
        KOVL_WAIT_FLAG_RELEASE_ON_SUCCESS = 0x0001,
        KOVL_WAIT_FLAG_RELEASE_ON_FAIL = 0x0002,
        KOVL_WAIT_FLAG_RELEASE_ON_SUCCESS_FAIL = 0x0003,
        KOVL_WAIT_FLAG_CANCEL_ON_TIMEOUT = 0x0004,
        KOVL_WAIT_FLAG_RELEASE_ON_TIMEOUT = 0x000C,
        KOVL_WAIT_FLAG_RELEASE_ALWAYS = 0x000F,
        KOVL_WAIT_FLAG_ALERTABLE = 0x0010
    }

    public enum KOVL_POOL_FLAG
    {
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
            var value = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)));

            try
            {
                Marshal.WriteByte(value, (byte)((on) ? 0x01 : 0x00));

                return SetPowerPolicy(handle, UsbKPowerPolicy.AutoSuspend,
                    (uint)Marshal.SizeOf(typeof(byte)), value);
            }
            finally
            {
                Marshal.FreeHGlobal(value);
            }
        }

        public bool OverlappedAcquire(ref KOVL_HANDLE OverlappedK, KOVL_POOL_HANDLE PoolHandle)
        {
            return OvlK_Acquire(ref OverlappedK, PoolHandle);
        }

        public bool OverlappedRelease(KOVL_HANDLE OverlappedK)
        {
            return OvlK_Release(OverlappedK);
        }

        public bool OverlappedInit(ref KOVL_POOL_HANDLE PoolHandle, KUSB_HANDLE UsbHandle,
            Int32 MaxOverlappedCount,
            KOVL_POOL_FLAG Flags)
        {
            return OvlK_Init(ref PoolHandle, UsbHandle, MaxOverlappedCount, Flags);
        }

        public bool OverlappedFree(KOVL_POOL_HANDLE PoolHandle)
        {
            return OvlK_Free(PoolHandle);
        }

        public HANDLE OverlappedGetEventHandle(KOVL_HANDLE OverlappedK)
        {
            return OvlK_GetEventHandle(OverlappedK);
        }

        public bool OverlappedWait(KOVL_HANDLE OverlappedK, Int32 TimeoutMS, KOVL_WAIT_FLAG WaitFlags,
            ref UInt32 TransferredLength)
        {
            return OvlK_Wait(OverlappedK, TimeoutMS, WaitFlags, ref TransferredLength);
        }

        public bool OverlappedWaitOldest(KOVL_POOL_HANDLE PoolHandle, ref KOVL_HANDLE OverlappedK,
            Int32 TimeoutMS, KOVL_WAIT_FLAG WaitFlags, ref UInt32 TransferredLength)
        {
            return OvlK_WaitOldest(PoolHandle, ref OverlappedK, TimeoutMS, WaitFlags, ref TransferredLength);
        }

        public bool OverlappedWaitOrCancel(KOVL_HANDLE OverlappedK, Int32 TimeoutMS,
            ref UInt32 TransferredLength)
        {
            return OvlK_WaitOrCancel(OverlappedK, TimeoutMS, ref TransferredLength);
        }

        public bool OverlappedReUse(KOVL_HANDLE OverlappedK)
        {
            return OvlK_ReUse(OverlappedK);
        }

        public bool GetPipePolicy(KUSB_HANDLE InterfaceHandle, byte PipeID,
            [MarshalAs(UnmanagedType.U4)] UsbKPipePolicy PolicyType,
            ref UInt32 ValueLength, IntPtr Value)
        {
            return UsbK_GetPipePolicy(InterfaceHandle, PipeID, PolicyType, ref ValueLength, Value);
        }
        
        public bool SetPipePolicy(KUSB_HANDLE InterfaceHandle, byte PipeID,
            [MarshalAs(UnmanagedType.U4)] UsbKPipePolicy PolicyType,
            UInt32 ValueLength, IntPtr Value)
        {
            return UsbK_SetPipePolicy(InterfaceHandle, PipeID, PolicyType, ValueLength, Value);
        }

        #region Private wrapper methods

        private bool GetPowerPolicy(KUSB_HANDLE InterfaceHandle, UsbKPowerPolicy PolicyType, ref uint ValueLength,
            IntPtr Value)
        {
            return UsbK_GetPowerPolicy(InterfaceHandle, PolicyType, ref ValueLength, Value);
        }

        private bool SetPowerPolicy(KUSB_HANDLE InterfaceHandle, UsbKPowerPolicy PolicyType, uint ValueLength,
            IntPtr Value)
        {
            return UsbK_SetPowerPolicy(InterfaceHandle, PolicyType, ValueLength, Value);
        }

        #endregion

        #region Usb Core

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_GetPowerPolicy(KUSB_HANDLE InterfaceHandle,
            [MarshalAs(UnmanagedType.U4)] UsbKPowerPolicy PolicyType, ref uint ValueLength,
            IntPtr Value);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_SetPowerPolicy(KUSB_HANDLE InterfaceHandle,
            [MarshalAs(UnmanagedType.U4)] UsbKPowerPolicy PolicyType, uint ValueLength,
            IntPtr Value);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_GetPipePolicy(KUSB_HANDLE InterfaceHandle, byte PipeID,
            [MarshalAs(UnmanagedType.U4)] UsbKPipePolicy PolicyType,
            ref UInt32 ValueLength, IntPtr Value);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool UsbK_SetPipePolicy(KUSB_HANDLE InterfaceHandle, byte PipeID,
            [MarshalAs(UnmanagedType.U4)] UsbKPipePolicy PolicyType,
            UInt32 ValueLength, IntPtr Value);

        #endregion

        #region Overlapped I/O

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_Acquire(ref KOVL_HANDLE OverlappedK, KOVL_POOL_HANDLE PoolHandle);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_Release(KOVL_HANDLE OverlappedK);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_Init(ref KOVL_POOL_HANDLE PoolHandle, KUSB_HANDLE UsbHandle,
            Int32 MaxOverlappedCount,
            KOVL_POOL_FLAG Flags);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_Free(KOVL_POOL_HANDLE PoolHandle);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern HANDLE OvlK_GetEventHandle(KOVL_HANDLE OverlappedK);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_Wait(KOVL_HANDLE OverlappedK, Int32 TimeoutMS, KOVL_WAIT_FLAG WaitFlags,
            ref UInt32 TransferredLength);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_WaitOldest(KOVL_POOL_HANDLE PoolHandle, ref KOVL_HANDLE OverlappedK,
            Int32 TimeoutMS, KOVL_WAIT_FLAG WaitFlags, ref UInt32 TransferredLength);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_WaitOrCancel(KOVL_HANDLE OverlappedK, Int32 TimeoutMS,
            ref UInt32 TransferredLength);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_WaitAndRelease(KOVL_HANDLE OverlappedK, Int32 TimeoutMS,
            ref UInt32 TransferredLength);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_IsComplete(KOVL_HANDLE OverlappedK);

        [DllImport("libusbK.dll", SetLastError = true)]
        private static extern bool OvlK_ReUse(KOVL_HANDLE OverlappedK);

        #endregion
    }
}