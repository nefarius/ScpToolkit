using System;
using System.Text;
using System.Runtime.InteropServices;

namespace ScpDriver 
{
    public class Devcon 
    {
        public static Boolean Find(Guid Target, ref String Path, ref String InstanceId, Int32 Instance = 0) 
        {
            IntPtr detailDataBuffer = IntPtr.Zero;
            IntPtr deviceInfoSet    = IntPtr.Zero;

            try
            {
                SP_DEVINFO_DATA deviceInterfaceData = new SP_DEVINFO_DATA(), da = new SP_DEVINFO_DATA();
                Int32 bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref Target, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                deviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(deviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref Target, memberIndex, ref deviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, ref da))
                        {
                            IntPtr pDevicePathName = detailDataBuffer + 4;

                            Path = Marshal.PtrToStringAuto(pDevicePathName).ToUpper();

                            if (memberIndex == Instance)
                            {
                                Int32  nBytes = 256;
                                IntPtr ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);

                                CM_Get_Device_ID(da.Flags, ptrInstanceBuf, nBytes, 0);
                                InstanceId = Marshal.PtrToStringAuto(ptrInstanceBuf).ToUpper();

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

        public static Boolean Create(String ClassName, Guid ClassGuid, String Node) 
        {
            IntPtr DeviceInfoSet = (IntPtr)(-1);
            SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();

            try
            {
                DeviceInfoSet = SetupDiCreateDeviceInfoList(ref ClassGuid, IntPtr.Zero);

                if (DeviceInfoSet == (IntPtr)(-1))
                {
                    return false;
                }

                DeviceInfoData.cbSize = Marshal.SizeOf(DeviceInfoData);

                if (!SetupDiCreateDeviceInfo(DeviceInfoSet, ClassName, ref ClassGuid, null, IntPtr.Zero, DICD_GENERATE_ID, ref DeviceInfoData))
                {
                    return false;
                }

                if (!SetupDiSetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, SPDRP_HARDWAREID, Node, Node.Length * 2))
                {
                    return false;
                }

                if (!SetupDiCallClassInstaller(DIF_REGISTERDEVICE, DeviceInfoSet, ref DeviceInfoData))
                {
                    return false;
                }
            }
            catch { }
            finally
            {
                if (DeviceInfoSet != (IntPtr)(-1))
                {
                    SetupDiDestroyDeviceInfoList(DeviceInfoSet);
                }
            }

            return true;
        }

        public static Boolean Remove(Guid ClassGuid, String Path, String InstanceId) 
        {
            IntPtr deviceInfoSet = IntPtr.Zero;

            try
            {
                SP_DEVINFO_DATA deviceInterfaceData = new SP_DEVINFO_DATA();

                deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                deviceInfoSet = SetupDiGetClassDevs(ref ClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if (SetupDiOpenDeviceInfo(deviceInfoSet, InstanceId, IntPtr.Zero, 0, ref deviceInterfaceData))
                {
                    SP_REMOVEDEVICE_PARAMS props = new SP_REMOVEDEVICE_PARAMS();

                    props.ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
                    props.ClassInstallHeader.cbSize = Marshal.SizeOf(props.ClassInstallHeader);
                    props.ClassInstallHeader.InstallFunction = DIF_REMOVE;

                    props.Scope     = DI_REMOVEDEVICE_GLOBAL;
                    props.HwProfile = 0x00;

                    if (SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInterfaceData, ref props, Marshal.SizeOf(props)))
                    {
                        return SetupDiCallClassInstaller(DIF_REMOVE, deviceInfoSet, ref deviceInterfaceData);
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

        #region Constant and Structure Definitions
        protected const Int32 DIGCF_PRESENT          = 0x0002;
        protected const Int32 DIGCF_DEVICEINTERFACE  = 0x0010;

        protected const Int32 DICD_GENERATE_ID       = 0x0001;
        protected const Int32 SPDRP_HARDWAREID       = 0x0001;

        protected const Int32 DIF_REMOVE             = 0x0005;
        protected const Int32 DIF_REGISTERDEVICE     = 0x0019;

        protected const Int32 DI_REMOVEDEVICE_GLOBAL = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_DEVINFO_DATA 
        {
            internal Int32  cbSize;
            internal Guid   ClassGuid;
            internal Int32  Flags;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_CLASSINSTALL_HEADER 
        {
            internal Int32 cbSize;
            internal Int32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct SP_REMOVEDEVICE_PARAMS 
        {
            internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal Int32 Scope;
            internal Int32 HwProfile;
        }
        #endregion

        #region Interop Definitions
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiCreateDeviceInfoList(ref Guid ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiCreateDeviceInfo(IntPtr DeviceInfoSet, String DeviceName, ref Guid ClassGuid, String DeviceDescription, IntPtr hwndParent, Int32 CreationFlags, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiSetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, Int32 Property, [MarshalAs(UnmanagedType.LPWStr)] String PropertyBuffer, Int32 PropertyBufferSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiCallClassInstaller(Int32 InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVINFO_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Int32 CM_Get_Device_ID(Int32 DevInst, IntPtr Buffer, Int32 BufferLen, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, String DeviceInstanceId, IntPtr hwndParent, Int32 Flags, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        protected static extern Boolean SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInterfaceData, ref SP_REMOVEDEVICE_PARAMS ClassInstallParams, Int32 ClassInstallParamsSize);
        #endregion
    }
}
