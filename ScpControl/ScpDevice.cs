using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace ScpControl
{
    public partial class ScpDevice : Component
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ScpDevice()
        {
            InitializeComponent();
        }

        public ScpDevice(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public ScpDevice(string Class)
        {
            InitializeComponent();

            m_Class = new Guid(Class);
        }

        public virtual bool IsActive
        {
            get { return m_IsActive; }
        }

        public virtual string Path
        {
            get { return m_Path; }
        }

        public virtual bool Open(int instance = 0)
        {
            var devicePath = string.Empty;

            if (Find(m_Class, ref devicePath, instance))
            {
                Open(devicePath);
            }

            return m_IsActive;
        }

        public virtual bool Open(string devicePath)
        {
            m_Path = devicePath.ToUpper();

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

        public virtual bool Start()
        {
            return m_IsActive;
        }

        public virtual bool Stop()
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

        public virtual bool Close()
        {
            return Stop();
        }

        public virtual bool ReadIntPipe(byte[] Buffer, int Length, ref int Transfered)
        {
            if (!m_IsActive) return false;

            return WinUsb_ReadPipe(m_WinUsbHandle, m_IntIn, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual bool ReadBulkPipe(byte[] Buffer, int Length, ref int Transfered)
        {
            if (!m_IsActive) return false;

            return WinUsb_ReadPipe(m_WinUsbHandle, m_BulkIn, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual bool WriteIntPipe(byte[] Buffer, int Length, ref int Transfered)
        {
            if (!m_IsActive) return false;

            return WinUsb_WritePipe(m_WinUsbHandle, m_IntOut, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual bool WriteBulkPipe(byte[] Buffer, int Length, ref int Transfered)
        {
            if (!m_IsActive) return false;

            return WinUsb_WritePipe(m_WinUsbHandle, m_BulkOut, Buffer, Length, ref Transfered, IntPtr.Zero);
        }

        public virtual bool SendTransfer(byte RequestType, byte Request, ushort Value, byte[] Buffer, ref int Transfered)
        {
            if (!m_IsActive) return false;

            var Setup = new WINUSB_SETUP_PACKET();

            Setup.RequestType = RequestType;
            Setup.Request = Request;
            Setup.Value = Value;
            Setup.Index = 0;
            Setup.Length = (ushort) Buffer.Length;

            return WinUsb_ControlTransfer(m_WinUsbHandle, Setup, Buffer, Buffer.Length, ref Transfered, IntPtr.Zero);
        }

        #region Constant and Structure Definitions

        public const int SERVICE_CONTROL_STOP = 0x00000001;
        public const int SERVICE_CONTROL_SHUTDOWN = 0x00000005;
        public const int SERVICE_CONTROL_DEVICEEVENT = 0x0000000B;
        public const int SERVICE_CONTROL_POWEREVENT = 0x0000000D;

        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x0005;
        public const int DBT_DEVTYP_HANDLE = 0x0006;

        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_APMSUSPEND = 0x0004;

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x0000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x0001;
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x0004;

        public const int WM_DEVICECHANGE = 0x0219;

        public const int DIGCF_PRESENT = 0x0002;
        public const int DIGCF_DEVICEINTERFACE = 0x0010;

        public delegate int ServiceControlHandlerEx(int Control, int Type, IntPtr Data, IntPtr Context);

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal int dbcc_size;
            internal int dbcc_devicetype;
            internal int dbcc_reserved;
            internal Guid dbcc_classguid;
            internal short dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class DEV_BROADCAST_DEVICEINTERFACE_M
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)] public byte[]
                dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)] public char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        protected const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        protected const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        protected const uint FILE_SHARE_READ = 1;
        protected const uint FILE_SHARE_WRITE = 2;
        protected const uint GENERIC_READ = 0x80000000;
        protected const uint GENERIC_WRITE = 0x40000000;
        protected const int INVALID_HANDLE_VALUE = -1;
        protected const uint OPEN_EXISTING = 3;
        protected const uint DEVICE_SPEED = 1;
        protected const byte USB_ENDPOINT_DIRECTION_MASK = 0x80;

        protected enum POLICY_TYPE
        {
            SHORT_PACKET_TERMINATE = 1,
            AUTO_CLEAR_STALL = 2,
            PIPE_TRANSFER_TIMEOUT = 3,
            IGNORE_SHORT_PACKETS = 4,
            ALLOW_PARTIAL_READS = 5,
            AUTO_FLUSH = 6,
            RAW_IO = 7
        }

        protected enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl = 0,
            UsbdPipeTypeIsochronous = 1,
            UsbdPipeTypeBulk = 2,
            UsbdPipeTypeInterrupt = 3
        }

        protected enum USB_DEVICE_SPEED
        {
            UsbLowSpeed = 1,
            UsbFullSpeed = 2,
            UsbHighSpeed = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct USB_CONFIGURATION_DESCRIPTOR
        {
            internal byte bLength;
            internal byte bDescriptorType;
            internal ushort wTotalLength;
            internal byte bNumInterfaces;
            internal byte bConfigurationValue;
            internal byte iConfiguration;
            internal byte bmAttributes;
            internal byte MaxPower;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct USB_INTERFACE_DESCRIPTOR
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
        protected struct WINUSB_PIPE_INFORMATION
        {
            internal USBD_PIPE_TYPE PipeType;
            internal byte PipeId;
            internal ushort MaximumPacketSize;
            internal byte Interval;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct WINUSB_SETUP_PACKET
        {
            internal byte RequestType;
            internal byte Request;
            internal ushort Value;
            internal ushort Index;
            internal ushort Length;
        }

        protected const int DIF_PROPERTYCHANGE = 0x12;
        protected const int DICS_ENABLE = 1;
        protected const int DICS_DISABLE = 2;
        protected const int DICS_PROPCHANGE = 3;
        protected const int DICS_FLAG_GLOBAL = 1;

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_CLASSINSTALL_HEADER
        {
            internal int cbSize;
            internal int InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_PROPCHANGE_PARAMS
        {
            internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal int StateChange;
            internal int Scope;
            internal int HwProfile;
        }

        #endregion

        #region Protected Data Members

        protected Guid m_Class = Guid.Empty;
        protected string m_Path = string.Empty;

        protected IntPtr m_FileHandle = IntPtr.Zero;
        private IntPtr m_WinUsbHandle = (IntPtr) INVALID_HANDLE_VALUE;

        protected byte m_IntIn = 0xFF;
        protected byte m_IntOut = 0xFF;
        protected byte m_BulkIn = 0xFF;
        protected byte m_BulkOut = 0xFF;

        protected bool m_IsActive;

        #endregion

        #region Static Helper Methods

        public enum Notified
        {
            Ignore = 0x0000,
            Arrival = 0x8000,
            QueryRemove = 0x8001,
            Removal = 0x8004
        };

        public static bool RegisterNotify(IntPtr Form, Guid Class, ref IntPtr Handle, bool Window = true)
        {
            var devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;

            try
            {
                var devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                var Size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size = Size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = Class;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                Handle = RegisterDeviceNotification(Form, devBroadcastDeviceInterfaceBuffer,
                    Window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                return Handle != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
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

        public static bool UnregisterNotify(IntPtr Handle)
        {
            try
            {
                return UnregisterDeviceNotification(Handle);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual bool Find(Guid Target, ref string Path, int Instance = 0)
        {
            var detailDataBuffer = IntPtr.Zero;
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(),
                    da = new SP_DEVICE_INTERFACE_DATA();
                int bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref Target, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                DeviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(DeviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref Target, memberIndex,
                    ref DeviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0,
                        ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer,
                            (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, detailDataBuffer,
                            bufferSize, ref bufferSize, ref da))
                        {
                            var pDevicePathName = detailDataBuffer + 4;

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
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
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

        protected virtual bool GetDeviceInstance(ref string Instance)
        {
            var detailDataBuffer = IntPtr.Zero;
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(),
                    da = new SP_DEVICE_INTERFACE_DATA();
                int bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref m_Class, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                DeviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(DeviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref m_Class, memberIndex,
                    ref DeviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, IntPtr.Zero, 0,
                        ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer,
                            (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref DeviceInterfaceData, detailDataBuffer,
                            bufferSize, ref bufferSize, ref da))
                        {
                            var pDevicePathName = detailDataBuffer + 4;

                            var Current = Marshal.PtrToStringAuto(pDevicePathName).ToUpper();
                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (Current == Path)
                            {
                                var nBytes = 256;
                                var ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);

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
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
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

        protected virtual bool GetDeviceHandle(string Path)
        {
            m_FileHandle = CreateFile(Path, (GENERIC_WRITE | GENERIC_READ), FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, 0);

            if (m_FileHandle == IntPtr.Zero || m_FileHandle == (IntPtr) INVALID_HANDLE_VALUE)
            {
                m_FileHandle = IntPtr.Zero;
                var lastError = GetLastError();
                Log.DebugFormat("LastError = {0}", lastError);
            }

            return !(m_FileHandle == IntPtr.Zero);
        }

        protected virtual bool UsbEndpointDirectionIn(int addr)
        {
            return (addr & 0x80) == 0x80;
        }

        protected virtual bool UsbEndpointDirectionOut(int addr)
        {
            return (addr & 0x80) == 0x00;
        }

        protected virtual bool InitializeDevice()
        {
            try
            {
                var ifaceDescriptor = new USB_INTERFACE_DESCRIPTOR();
                var pipeInfo = new WINUSB_PIPE_INFORMATION();

                if (WinUsb_QueryInterfaceSettings(m_WinUsbHandle, 0, ref ifaceDescriptor))
                {
                    for (var i = 0; i < ifaceDescriptor.bNumEndpoints; i++)
                    {
                        WinUsb_QueryPipe(m_WinUsbHandle, 0, Convert.ToByte(i), ref pipeInfo);

                        if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                             UsbEndpointDirectionIn(pipeInfo.PipeId)))
                        {
                            m_BulkIn = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_BulkIn);
                        }
                        else if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                  UsbEndpointDirectionOut(pipeInfo.PipeId)))
                        {
                            m_BulkOut = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_BulkOut);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                 UsbEndpointDirectionIn(pipeInfo.PipeId))
                        {
                            m_IntIn = pipeInfo.PipeId;
                            WinUsb_FlushPipe(m_WinUsbHandle, m_IntIn);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                 UsbEndpointDirectionOut(pipeInfo.PipeId))
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
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        protected virtual bool RestartDevice(string InstanceId)
        {
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                var deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();

                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                deviceInfoSet = SetupDiGetClassDevs(ref m_Class, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (SetupDiOpenDeviceInfo(deviceInfoSet, InstanceId, IntPtr.Zero, 0, ref deviceInterfaceData))
                {
                    var props = new SP_PROPCHANGE_PARAMS();

                    props.ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
                    props.ClassInstallHeader.cbSize = Marshal.SizeOf(props.ClassInstallHeader);
                    props.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE;

                    props.Scope = DICS_FLAG_GLOBAL;
                    props.StateChange = DICS_PROPCHANGE;
                    props.HwProfile = 0x00;

                    if (SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInterfaceData, ref props,
                        Marshal.SizeOf(props)))
                    {
                        return SetupDiChangeState(deviceInfoSet, ref deviceInterfaceData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
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
        protected static extern int SetupDiCreateDeviceInfoList(ref Guid ClassGuid, int hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        protected static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent,
            int Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize, ref int RequiredSize, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter,
            int Flags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_Initialize(IntPtr DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber,
            byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_AbortPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_FlushPipe(IntPtr InterfaceHandle, byte PipeID);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket,
            byte[] Buffer, int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_WritePipe(IntPtr InterfaceHandle, byte PipeID, byte[] Buffer,
            int BufferLength, ref int LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        protected static extern bool WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(string ServiceName, ServiceControlHandlerEx Callback,
            IntPtr Context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool DeviceIoControl(IntPtr DeviceHandle, int IoControlCode, byte[] InBuffer,
            int InBufferSize, byte[] OutBuffer, int OutBufferSize, ref int BytesReturned, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool CloseHandle(IntPtr Handle);

        [DllImport("kernel32.dll")]
        protected static extern int GetLastError();

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern int CM_Get_Device_ID(int dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, string DeviceInstanceId,
            IntPtr hwndParent, int Flags, ref SP_DEVICE_INTERFACE_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiChangeState(IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_PROPCHANGE_PARAMS ClassInstallParams,
            int ClassInstallParamsSize);

        #endregion
    }
}