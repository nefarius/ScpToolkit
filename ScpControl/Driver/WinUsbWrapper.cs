using System;
using System.Runtime.InteropServices;

namespace ScpControl.Driver
{
    public enum USBD_PIPE_TYPE
    {
        UsbdPipeTypeControl = 0,
        UsbdPipeTypeIsochronous = 1,
        UsbdPipeTypeBulk = 2,
        UsbdPipeTypeInterrupt = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct USB_INTERFACE_DESCRIPTOR
    {
        internal byte bLength;
        internal byte bDescriptorType;
        internal byte bInterfaceNumber;
        internal byte bAlternateSetting;
        internal byte bNumEndpoints;
        internal byte bInterfaceClass;
        internal byte bInterfaceSubClass;
        internal byte bInterfaceProtocol;
        internal byte iInterface;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINUSB_PIPE_INFORMATION
    {
        internal USBD_PIPE_TYPE PipeType;
        internal byte PipeId;
        internal ushort MaximumPacketSize;
        internal byte Interval;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WINUSB_SETUP_PACKET
    {
        internal byte RequestType;
        internal byte Request;
        internal ushort Value;
        internal ushort Index;
        internal ushort Length;
    }

    public class WinUsbWrapper
    {
        private static readonly Lazy<WinUsbWrapper> LazyInstance = new Lazy<WinUsbWrapper>(() => new WinUsbWrapper());

        private WinUsbWrapper()
        {
        }

        public static WinUsbWrapper Instance
        {
            get { return LazyInstance.Value; }
        }

        public bool Initialize(IntPtr DeviceHandle, ref IntPtr InterfaceHandle)
        {
            return WinUsb_Initialize(DeviceHandle, ref InterfaceHandle);
        }

        public bool QueryInterfaceSettings(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor)
        {
            return WinUsb_QueryInterfaceSettings(InterfaceHandle, AlternateInterfaceNumber,
                ref UsbAltInterfaceDescriptor);
        }

        public bool QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation)
        {
            return WinUsb_QueryPipe(InterfaceHandle, AlternateInterfaceNumber, PipeIndex, ref PipeInformation);
        }

        public bool AbortPipe(IntPtr InterfaceHandle, byte PipeID)
        {
            return WinUsb_AbortPipe(InterfaceHandle, PipeID);
        }

        public bool FlushPipe(IntPtr InterfaceHandle, byte PipeID)
        {
            return WinUsb_FlushPipe(InterfaceHandle, PipeID);
        }

        public bool ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket,
            byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped)
        {
            return WinUsb_ControlTransfer(InterfaceHandle, SetupPacket, Buffer, BufferLength, ref LengthTransferred,
                Overlapped);
        }

        public bool ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped)
        {
            return WinUsb_ReadPipe(InterfaceHandle, PipeID, Buffer, BufferLength, ref LengthTransferred, Overlapped);
        }

        public bool WritePipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped)
        {
            return WinUsb_WritePipe(InterfaceHandle, PipeID, Buffer, BufferLength, ref LengthTransferred, Overlapped);
        }

        public bool Free(IntPtr InterfaceHandle)
        {
            return WinUsb_Free(InterfaceHandle);
        }

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_Initialize(IntPtr DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_AbortPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_FlushPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket,
            byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_WritePipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        private static extern bool WinUsb_Free(IntPtr InterfaceHandle);
    }
}