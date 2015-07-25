using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ScpControl 
{
    public partial class ScpDevice : Component 
    {
        public virtual Boolean IsActive 
        {
            get { return m_IsActive; }
        }

        public virtual String Path 
        {
            get { return m_Path; }
        }


        public ScpDevice() 
        {
            InitializeComponent();
        }

        public ScpDevice(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }

        public ScpDevice(String Class) 
        {
            InitializeComponent();

            this.m_Class = new Guid(Class);
        }


        public virtual Boolean Open(Int32 Instance = 0) 
        {
            String DevicePath = String.Empty;

            if (Find(m_Class, ref DevicePath, Instance))
            {
                Open(DevicePath);
            }

            return m_IsActive;
        }

        public virtual Boolean Open(String DevicePath)  
        {
            m_Path = DevicePath.ToUpper();

            if (GetDeviceHandle(m_Path))
            {
                if (WinUsb_Initialize(m_FileHandle, ref m_WinUsbHandle))
                {
                    if (InitializeDevice())
                    {
                        m_IsActive = true;
                    }
                    else
                    {
                        WinUsb_Free(m_WinUsbHandle);
                        m_WinUsbHandle = (IntPtr) INVALID_HANDLE_VALUE;
                    }
                }
                else
                {
                    CloseHandle(m_FileHandle);
                }
            }

            return m_IsActive;
        }


        public virtual Boolean Start() 
        {
            return m_IsActive;
        }

        public virtual Boolean Stop()  
        {
            m_IsActive = false;

            if (!(m_WinUsbHandle == (IntPtr) INVALID_HANDLE_VALUE))
            {
                WinUsb_AbortPipe(m_WinUsbHandle, m_IntIn);
                WinUsb_AbortPipe(m_WinUsbHandle, m_BulkIn);
                WinUsb_AbortPipe(m_WinUsbHandle, m_BulkOut);

                WinUsb_Free(m_WinUsbHandle);
                m_WinUsbHandle = (IntPtr) INVALID_HANDLE_VALUE;
            }

            if (m_FileHandle != IntPtr.Zero)
            {
                CloseHandle(m_FileHandle);

                m_FileHandle = IntPtr.Zero;
            }

            return true;
        }

        public virtual Boolean Close() 
        {
            return Stop();
        }


        public virtual Boolean ReadIntPipe  (Byte[] Buffer, Int32 Length, ref Int32 Transfered) 
        {
            if (!m_IsActive) return false;

            return WinUsb_ReadPipe(m_WinUsbHandle, m_IntIn, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual Boolean ReadBulkPipe (Byte[] Buffer, Int32 Length, ref Int32 Transfered) 
        {
            if (!m_IsActive) return false;

            return WinUsb_ReadPipe(m_WinUsbHandle, m_BulkIn, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual Boolean WriteIntPipe (Byte[] Buffer, Int32 Length, ref Int32 Transfered) 
        {
            if (!m_IsActive) return false;

            return WinUsb_WritePipe(m_WinUsbHandle, m_IntOut, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual Boolean WriteBulkPipe(Byte[] Buffer, Int32 Length, ref Int32 Transfered) 
        {
            if (!m_IsActive) return false;

            return WinUsb_WritePipe(m_WinUsbHandle, m_BulkOut, Buffer, Length, ref Transfered, IntPtr.Zero);
        }


        public virtual Boolean SendTransfer(Byte RequestType, Byte Request, UInt16 Value, Byte[] Buffer, ref Int32 Transfered) 
        {
            if (!m_IsActive) return false;

            WINUSB_SETUP_PACKET Setup = new WINUSB_SETUP_PACKET();

            Setup.RequestType = RequestType;
            Setup.Request     = Request;
            Setup.Value       = Value;
            Setup.Index       = 0;
            Setup.Length      = (UInt16) Buffer.Length;

            return WinUsb_ControlTransfer(m_WinUsbHandle, Setup, Buffer, Buffer.Length, ref Transfered, IntPtr.Zero);
        }


        #region Constant and Structure Definitions
        public const Int32 SERVICE_CONTROL_STOP                 = 0x00000001;
        public const Int32 SERVICE_CONTROL_SHUTDOWN             = 0x00000005;
        public const Int32 SERVICE_CONTROL_DEVICEEVENT          = 0x0000000B;
        public const Int32 SERVICE_CONTROL_POWEREVENT           = 0x0000000D;

        public const Int32 DBT_DEVICEARRIVAL                    = 0x8000;
        public const Int32 DBT_DEVICEQUERYREMOVE                = 0x8001;
        public const Int32 DBT_DEVICEREMOVECOMPLETE             = 0x8004;
        public const Int32 DBT_DEVTYP_DEVICEINTERFACE           = 0x0005;
        public const Int32 DBT_DEVTYP_HANDLE                    = 0x0006;

        public const Int32 PBT_APMRESUMEAUTOMATIC               = 0x0012;
        public const Int32 PBT_APMSUSPEND                       = 0x0004;

        public const Int32 DEVICE_NOTIFY_WINDOW_HANDLE          = 0x0000;
        public const Int32 DEVICE_NOTIFY_SERVICE_HANDLE         = 0x0001;
        public const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES  = 0x0004;

        public const Int32 WM_DEVICECHANGE                      = 0x0219;

        public const Int32 DIGCF_PRESENT                        = 0x0002;
        public const Int32 DIGCF_DEVICEINTERFACE                = 0x0010;

        public delegate Int32 ServiceControlHandlerEx(Int32 Control, Int32 Type, IntPtr Data, IntPtr Context);

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE 
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid  dbcc_classguid;
            internal Int16 dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class DEV_BROADCAST_DEVICEINTERFACE_M 
        {
            public Int32 dbcc_size;
            public Int32 dbcc_devicetype;
            public Int32 dbcc_reserved;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public Byte[] dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public Char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HDR 
        {
            public Int32 dbch_size;
            public Int32 dbch_devicetype;
            public Int32 dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_DEVICE_INTERFACE_DATA 
        {
            internal Int32  cbSize;
            internal Guid   InterfaceClassGuid;
            internal Int32  Flags;
            internal IntPtr Reserved;
        }

        protected const UInt32 FILE_ATTRIBUTE_NORMAL        = 0x80;
        protected const UInt32 FILE_FLAG_OVERLAPPED         = 0x40000000;
        protected const UInt32 FILE_SHARE_READ              = 1;
        protected const UInt32 FILE_SHARE_WRITE             = 2;
        protected const UInt32 GENERIC_READ                 = 0x80000000;
        protected const UInt32 GENERIC_WRITE                = 0x40000000;
        protected const  Int32 INVALID_HANDLE_VALUE         = -1;
        protected const UInt32 OPEN_EXISTING                = 3;
        protected const UInt32 DEVICE_SPEED                 = 1;
        protected const Byte   USB_ENDPOINT_DIRECTION_MASK  = 0x80;

        protected enum POLICY_TYPE 
        {
            SHORT_PACKET_TERMINATE = 1,
            AUTO_CLEAR_STALL       = 2,
            PIPE_TRANSFER_TIMEOUT  = 3,
            IGNORE_SHORT_PACKETS   = 4,
            ALLOW_PARTIAL_READS    = 5,
            AUTO_FLUSH             = 6,
            RAW_IO                 = 7,
        }

        protected enum USBD_PIPE_TYPE 
        {
            UsbdPipeTypeControl     = 0,
            UsbdPipeTypeIsochronous = 1,
            UsbdPipeTypeBulk        = 2,
            UsbdPipeTypeInterrupt   = 3,
        }

        protected enum USB_DEVICE_SPEED 
        {
            UsbLowSpeed  = 1,
            UsbFullSpeed = 2,
            UsbHighSpeed = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct USB_CONFIGURATION_DESCRIPTOR 
        {
            internal Byte   bLength;
            internal Byte   bDescriptorType;
            internal UInt16 wTotalLength;
            internal Byte   bNumInterfaces;
            internal Byte   bConfigurationValue;
            internal Byte   iConfiguration;
            internal Byte   bmAttributes;
            internal Byte   MaxPower;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct USB_INTERFACE_DESCRIPTOR 
        {
            internal Byte bLength;
            internal Byte bDescriptorType;
            internal Byte bInterfaceNumber;
            internal Byte bAlternateSetting;
            internal Byte bNumEndpoints;
            internal Byte bInterfaceClass;
            internal Byte bInterfaceSubClass;
            internal Byte bInterfaceProtocol;
            internal Byte iInterface;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct WINUSB_PIPE_INFORMATION 
        {
            internal USBD_PIPE_TYPE PipeType;
            internal Byte           PipeId;
            internal UInt16         MaximumPacketSize;
            internal Byte           Interval;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct WINUSB_SETUP_PACKET 
        {
            internal Byte   RequestType;
            internal Byte   Request;
            internal UInt16 Value;
            internal UInt16 Index;
            internal UInt16 Length;
        }

        protected const Int32 DIF_PROPERTYCHANGE = 0x12;
        protected const Int32 DICS_ENABLE        = 1;
        protected const Int32 DICS_DISABLE       = 2;
        protected const Int32 DICS_PROPCHANGE    = 3;
        protected const Int32 DICS_FLAG_GLOBAL   = 1;

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_CLASSINSTALL_HEADER 
        {
            internal Int32 cbSize;
            internal Int32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_PROPCHANGE_PARAMS 
        {
            internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal Int32 StateChange;
            internal Int32 Scope;
            internal Int32 HwProfile;
        }
        #endregion

        #region Protected Data Members
        protected Guid   m_Class = Guid.Empty;
        protected String m_Path  = String.Empty;

        protected IntPtr m_FileHandle   = IntPtr.Zero;
        private   IntPtr m_WinUsbHandle = (IntPtr) INVALID_HANDLE_VALUE;

        protected Byte m_IntIn   = 0xFF;
        protected Byte m_IntOut  = 0xFF;
        protected Byte m_BulkIn  = 0xFF;
        protected Byte m_BulkOut = 0xFF;

        protected Boolean m_IsActive = false;
        #endregion

        #region Static Helper Methods
        public enum Notified { Ignore = 0x0000, Arrival = 0x8000, QueryRemove = 0x8001, Removal = 0x8004 };

        public static Boolean RegisterNotify(IntPtr Form, Guid Class, ref IntPtr Handle, Boolean Window = true) 
        {
            IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;

            try
            {
                DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                Int32 Size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size       = Size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved   = 0;
                devBroadcastDeviceInterface.dbcc_classguid  = Class;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                Handle = RegisterDeviceNotification(Form, devBroadcastDeviceInterfaceBuffer, Window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                return Handle != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        public static Boolean UnregisterNotify(IntPtr Handle) 
        {
            try
            {
                return UnregisterDeviceNotification(Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }
        #endregion

        #region Protected Methods
        protected virtual Boolean Find(Guid Target, ref String Path, Int32 Instance = 0) 
        {
            IntPtr detailDataBuffer = IntPtr.Zero;
            IntPtr deviceInfoSet    = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(), da = new SP_DEVICE_INTERFACE_DATA();
                Int32 bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref Target, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                DeviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(DeviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref Target, memberIndex, ref DeviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, ref da))
                        {
                            IntPtr pDevicePathName = detailDataBuffer + 4;

                            Path = Marshal.PtrToStringAuto(pDevicePathName).ToUpper();
                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (memberIndex == Instance) return true;
                        }
                        else Marshal.FreeHGlobal(detailDataBuffer);
                    }

                    memberIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return false;
        }

        protected virtual Boolean GetDeviceInstance(ref String Instance) 
        {
            IntPtr detailDataBuffer = IntPtr.Zero;
            IntPtr deviceInfoSet    = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(), da = new SP_DEVICE_INTERFACE_DATA();
                Int32 bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref m_Class, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                DeviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(DeviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref m_Class, memberIndex, ref DeviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, ref da))
                        {
                            IntPtr pDevicePathName = detailDataBuffer + 4;

                            String Current = Marshal.PtrToStringAuto(pDevicePathName).ToUpper();
                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (Current == Path)
                            {
                                Int32  nBytes = 256;
                                IntPtr ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);

                                CM_Get_Device_ID(da.Flags, ptrInstanceBuf, nBytes, 0);
                                Instance = Marshal.PtrToStringAuto(ptrInstanceBuf).ToUpper();

                                Marshal.FreeHGlobal(ptrInstanceBuf);
                                return true;
                            }
                        }
                        else Marshal.FreeHGlobal(detailDataBuffer);
                    }

                    memberIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return false;
        }

        protected virtual Boolean GetDeviceHandle(String Path) 
        {
            Int32 LastError;

            m_FileHandle = CreateFile(Path, (GENERIC_WRITE | GENERIC_READ), FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, 0);

            if (m_FileHandle == IntPtr.Zero || m_FileHandle == (IntPtr) INVALID_HANDLE_VALUE)
            {
                m_FileHandle = IntPtr.Zero;
                LastError = GetLastError();
            }

            return !(m_FileHandle == IntPtr.Zero);
        }

        protected virtual Boolean UsbEndpointDirectionIn(Int32 addr) 
        {
            return (addr & 0x80) == 0x80;
        }

        protected virtual Boolean UsbEndpointDirectionOut(Int32 addr) 
        {
            return (addr & 0x80) == 0x00;
        }

        protected virtual Boolean InitializeDevice() 
        {
            try
            {
                USB_INTERFACE_DESCRIPTOR ifaceDescriptor = new USB_INTERFACE_DESCRIPTOR();
                WINUSB_PIPE_INFORMATION  pipeInfo = new WINUSB_PIPE_INFORMATION();

                if (WinUsb_QueryInterfaceSettings(m_WinUsbHandle, 0, ref ifaceDescriptor))
                {
                    for (Int32 i = 0; i < ifaceDescriptor.bNumEndpoints; i++)
                    {
                        WinUsb_QueryPipe(m_WinUsbHandle, 0, System.Convert.ToByte(i), ref pipeInfo);

                        if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) & UsbEndpointDirectionIn(pipeInfo.PipeId)))
                        {
                            m_BulkIn = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_BulkIn);
                        }
                        else if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) & UsbEndpointDirectionOut(pipeInfo.PipeId)))
                        {
                            m_BulkOut = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_BulkOut);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) & UsbEndpointDirectionIn(pipeInfo.PipeId))
                        {
                            m_IntIn = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_IntIn);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) & UsbEndpointDirectionOut(pipeInfo.PipeId))
                        {
                            m_IntOut = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_IntOut);
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        protected virtual Boolean RestartDevice(String InstanceId) 
        {
            IntPtr deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();

                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                deviceInfoSet = SetupDiGetClassDevs(ref m_Class, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (SetupDiOpenDeviceInfo(deviceInfoSet, InstanceId, IntPtr.Zero, 0, ref deviceInterfaceData))
                {
                    SP_PROPCHANGE_PARAMS props = new SP_PROPCHANGE_PARAMS();

                    props.ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
                    props.ClassInstallHeader.cbSize = Marshal.SizeOf(props.ClassInstallHeader);
                    props.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE;

                    props.Scope       = DICS_FLAG_GLOBAL;
                    props.StateChange = DICS_PROPCHANGE;
                    props.HwProfile   = 0x00;

                    if (SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInterfaceData, ref props, Marshal.SizeOf(props)))
                    {
                        return SetupDiChangeState(deviceInfoSet, ref deviceInterfaceData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return false;
        }
        #endregion

        #region Interop Definitions
        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern Boolean UnregisterDeviceNotification(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, UInt32 hTemplateFile);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_Initialize(IntPtr DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_QueryPipe(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, Byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_AbortPipe(IntPtr InterfaceHandle, Byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_FlushPipe(IntPtr InterfaceHandle, Byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, Byte[] Buffer, Int32 BufferLength, ref Int32 LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, Int32 BufferLength, ref Int32 LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_WritePipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, Int32 BufferLength, ref Int32 LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(String ServiceName, ServiceControlHandlerEx Callback, IntPtr Context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern Boolean DeviceIoControl(IntPtr DeviceHandle, Int32 IoControlCode, Byte[] InBuffer, Int32 InBufferSize, Byte[] OutBuffer, Int32 OutBufferSize, ref Int32 BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern Boolean CloseHandle(IntPtr Handle);

        [DllImport("kernel32.dll")]
        protected static extern Int32 GetLastError();

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Int32 CM_Get_Device_ID(Int32 dnDevInst, IntPtr Buffer, Int32 BufferLen, Int32 ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, String DeviceInstanceId, IntPtr hwndParent, Int32 Flags, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiChangeState(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_PROPCHANGE_PARAMS ClassInstallParams, Int32 ClassInstallParamsSize);
        #endregion
    }
}
