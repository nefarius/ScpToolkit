using System;
using System.Runtime.InteropServices;

namespace ScpControl
{
    public partial class ScpDevice
    {
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
