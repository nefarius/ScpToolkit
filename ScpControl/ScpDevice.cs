using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;
using ScpControl.Driver;
using ScpControl.Usb;

namespace ScpControl
{
    /// <summary>
    ///     Low-level representation of an Scp-compatible USB device.
    /// </summary>
    public partial class ScpDevice : Component
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly WinUsbWrapper Usb = WinUsbWrapper.Instance;

        #region Ctors

        protected ScpDevice()
        {
            InitializeComponent();
        }

        public ScpDevice(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected ScpDevice(Guid Class)
        {
            InitializeComponent();

            this._class = Class;
        }

        #endregion

        protected bool IsActive { get; set; }

        public string Path { get; protected set; }

        public short VendorId { get; protected set; }

        public short ProductId { get; protected set; }

        protected void GetHardwareId(string devicePath)
        {
            short vid, pid;

            GetHardwareId(devicePath, out vid, out pid);

            // get values
            VendorId = vid;
            ProductId = pid;
        }

        protected static void GetHardwareId(string devicePath, out short vendorId, out short productId)
        {
            // regex to extract vendor ID and product ID from hardware ID string
            var regex = new Regex("VID_([0-9A-Z]{4})&PID_([0-9A-Z]{4})", RegexOptions.IgnoreCase);
            // matched groups
            var matches = regex.Match(devicePath).Groups;

            // very basic check
            if (matches.Count < 3)
            {
                vendorId = productId = 0;
                return;
            }

            // get values
            vendorId = short.Parse(matches[1].Value, NumberStyles.HexNumber);
            productId = short.Parse(matches[2].Value, NumberStyles.HexNumber);
        }

        public virtual bool Open(int instance = 0)
        {
            var devicePath = string.Empty;

            if (FindDevice(_class, ref devicePath, instance))
            {
                Open(devicePath);
            }

            return IsActive;
        }

        public virtual bool Open(string devicePath)
        {
            GetHardwareId(devicePath);

            Path = devicePath.ToUpper();

            if (GetDeviceHandle(Path))
            {
                if (Usb.Initialize(FileHandle, ref _winUsbHandle))
                {
                    if (InitializeDevice())
                    {
                        if (!LibusbKWrapper.Instance.SetPowerPolicyAutoSuspend(_winUsbHandle))
                        {
                            Log.Warn("Couldn't alter power policy");
                        }

                        IsActive = true;
                    }
                    else
                    {
                        Usb.Free(_winUsbHandle);
                        _winUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;
                    }
                }
                else
                {
                    CloseHandle(FileHandle);
                }
            }

            return IsActive;
        }

        public virtual bool Start()
        {
            return IsActive;
        }

        public virtual bool Stop()
        {
            IsActive = false;

            if (!(_winUsbHandle == (IntPtr)INVALID_HANDLE_VALUE))
            {
                Usb.AbortPipe(_winUsbHandle, IntIn);
                Usb.AbortPipe(_winUsbHandle, BulkIn);
                Usb.AbortPipe(_winUsbHandle, BulkOut);

                Usb.Free(_winUsbHandle);
                _winUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;
            }

            if (FileHandle != IntPtr.Zero)
            {
                CloseHandle(FileHandle);

                FileHandle = IntPtr.Zero;
            }

            return true;
        }

        public virtual bool Close()
        {
            return Stop();
        }

        protected static ushort ToValue(UsbHidClassDescriptorType type, byte index = 0x00)
        {
            return BitConverter.ToUInt16(new[] { (byte)index, (byte)type }, 0);
        }

        protected static ushort ToValue(UsbHidReportRequestType type)
        {
            return (ushort) ((byte) type << 8 | (byte) 0x00);
        }

        protected static ushort ToValue(UsbHidReportRequestType type, UsbHidReportRequestId id)
        {
            return BitConverter.ToUInt16(new[] { (byte)id, (byte)type }, 0);
        }

        #region WinUSB wrapper methods

        protected bool ReadIntPipe(byte[] buffer, int length, ref int transfered)
        {
            return IsActive && Usb.ReadPipe(_winUsbHandle, IntIn, buffer, length, ref transfered, IntPtr.Zero);
        }

        protected bool ReadBulkPipe(byte[] buffer, int length, ref int transfered)
        {
            return IsActive && Usb.ReadPipe(_winUsbHandle, BulkIn, buffer, length, ref transfered, IntPtr.Zero);
        }

        protected bool WriteIntPipe(byte[] buffer, int length, ref int transfered)
        {
            return IsActive && Usb.WritePipe(_winUsbHandle, IntOut, buffer, length, ref transfered, IntPtr.Zero);
        }

        protected bool WriteBulkPipe(byte[] buffer, int length, ref int transfered)
        {
            return IsActive && Usb.WritePipe(_winUsbHandle, BulkOut, buffer, length, ref transfered, IntPtr.Zero);
        }

        protected bool SendTransfer(UsbHidRequestType requestType, UsbHidRequest request, ushort value, byte[] buffer,
            ref int transfered)
        {
            return SendTransfer((byte)requestType, (byte)request, value, buffer, ref transfered);
        }

        protected bool SendTransfer(byte requestType, byte request, ushort value, byte[] buffer, ref int transfered)
        {
            if (!IsActive) return false;

            var setup = new WINUSB_SETUP_PACKET
            {
                RequestType = requestType,
                Request = request,
                Value = value,
                Index = 0,
                Length = (ushort)buffer.Length
            };

            return Usb.ControlTransfer(_winUsbHandle, setup, buffer, buffer.Length, ref transfered, IntPtr.Zero);
        }

        #endregion

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

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[]
                dbcc_classguid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public char[] dbcc_name;
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

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const int INVALID_HANDLE_VALUE = -1;
        private const uint OPEN_EXISTING = 3;
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

        private Guid _class = Guid.Empty;

        protected IntPtr FileHandle = IntPtr.Zero;
        private IntPtr _winUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;

        protected byte IntIn = 0xFF;
        protected byte IntOut = 0xFF;
        protected byte BulkIn = 0xFF;
        protected byte BulkOut = 0xFF;

        #endregion

        #region Static Helper Methods

        public enum Notified
        {
            Ignore = 0x0000,
            Arrival = 0x8000,
            QueryRemove = 0x8001,
            Removal = 0x8004
        };

        public static bool RegisterNotify(IntPtr form, Guid Class, ref IntPtr handle, bool window = true)
        {
            var devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;

            try
            {
                var devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                var size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size = size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = Class;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                handle = RegisterDeviceNotification(form, devBroadcastDeviceInterfaceBuffer,
                    window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                return handle != IntPtr.Zero;
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

        public static bool UnregisterNotify(IntPtr handle)
        {
            try
            {
                return UnregisterDeviceNotification(handle);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual bool FindDevice(Guid target, ref string path, int instance = 0)
        {
            var detailDataBuffer = IntPtr.Zero;
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(),
                    da = new SP_DEVICE_INTERFACE_DATA();
                int bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref target, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                deviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(deviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref target, memberIndex,
                    ref deviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0,
                        ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer,
                            (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, detailDataBuffer,
                            bufferSize, ref bufferSize, ref da))
                        {
                            var pDevicePathName = detailDataBuffer + 4;

                            path = (Marshal.PtrToStringAuto(pDevicePathName) ?? "ERROR").ToUpper();
                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (memberIndex == instance) return true;
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

        protected virtual bool GetDeviceInstance(ref string instance)
        {
            var detailDataBuffer = IntPtr.Zero;
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(),
                    da = new SP_DEVICE_INTERFACE_DATA();
                int bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref _class, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                deviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(deviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref _class, memberIndex,
                    ref deviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0,
                        ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer,
                            (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, detailDataBuffer,
                            bufferSize, ref bufferSize, ref da))
                        {
                            var pDevicePathName = detailDataBuffer + 4;

                            var current = (Marshal.PtrToStringAuto(pDevicePathName) ?? "ERROR").ToUpper();
                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (current == Path)
                            {
                                const int nBytes = 256;
                                var ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);

                                CM_Get_Device_ID(da.Flags, ptrInstanceBuf, nBytes, 0);
                                instance = (Marshal.PtrToStringAuto(ptrInstanceBuf) ?? "ERROR").ToUpper();

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

        protected virtual bool GetDeviceHandle(string path)
        {
            FileHandle = CreateFile(path, (GENERIC_WRITE | GENERIC_READ), FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, 0);

            if (FileHandle == IntPtr.Zero || FileHandle == (IntPtr)INVALID_HANDLE_VALUE)
            {
                FileHandle = IntPtr.Zero;
                var lastError = GetLastError();
                Log.DebugFormat("LastError = {0}", lastError);
            }

            return !(FileHandle == IntPtr.Zero);
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

                if (Usb.QueryInterfaceSettings(_winUsbHandle, 0, ref ifaceDescriptor))
                {
                    for (var i = 0; i < ifaceDescriptor.bNumEndpoints; i++)
                    {
                        Usb.QueryPipe(_winUsbHandle, 0, Convert.ToByte(i), ref pipeInfo);

                        if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                             UsbEndpointDirectionIn(pipeInfo.PipeId)))
                        {
                            BulkIn = pipeInfo.PipeId;
                            Usb.FlushPipe(_winUsbHandle, BulkIn);
                        }
                        else if (((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                  UsbEndpointDirectionOut(pipeInfo.PipeId)))
                        {
                            BulkOut = pipeInfo.PipeId;
                            Usb.FlushPipe(_winUsbHandle, BulkOut);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                 UsbEndpointDirectionIn(pipeInfo.PipeId))
                        {
                            IntIn = pipeInfo.PipeId;
                            Usb.FlushPipe(_winUsbHandle, IntIn);
                        }
                        else if ((pipeInfo.PipeType == USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                 UsbEndpointDirectionOut(pipeInfo.PipeId))
                        {
                            IntOut = pipeInfo.PipeId;
                            Usb.FlushPipe(_winUsbHandle, IntOut);
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

        protected virtual bool RestartDevice(string instanceId)
        {
            var deviceInfoSet = IntPtr.Zero;

            try
            {
                var deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();

                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                deviceInfoSet = SetupDiGetClassDevs(ref _class, IntPtr.Zero, IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (SetupDiOpenDeviceInfo(deviceInfoSet, instanceId, IntPtr.Zero, 0, ref deviceInterfaceData))
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
    }
}