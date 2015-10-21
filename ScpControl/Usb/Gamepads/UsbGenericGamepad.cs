using System;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl.Usb.Gamepads
{
    public class UsbGenericGamepad : UsbDevice
    {
        /// <summary>
        ///     GUID_DEVINTERFACE_HID
        /// </summary>
        public static Guid DeviceClassGuid
        {
            // https://msdn.microsoft.com/en-us/library/windows/hardware/ff545860(v=vs.85).aspx
            get { return Guid.Parse("{4D1E55B2-F16F-11CF-88CB-001111000030}"); }
        }

        public static UsbDevice DeviceFactory(string devicePath)
        {
            short vid, pid;

            GetHardwareId(devicePath, out vid, out pid);

            #region DragonRise Inc. USB Gamepad SNES

            if (vid == 0x0079 && pid == 0x0011)
            {
                Log.InfoFormat(
                    "DragonRise Inc. USB Gamepad SNES detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);
                
                return new UsbSnesGamepad();
            }

            #endregion

            #region LSI Logic Gamepad

            if (vid == 0x0079 && pid == 0x0006)
            {
                Log.InfoFormat(
                    "LSI Logic Gamepad detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);
                
                return new UsbLsiLogicGamepad();
            }

            #endregion

            #region ShanWan Wireless Gamepad

            if (vid == 0x2563 && pid == 0x0523)
            {
                Log.InfoFormat(
                    "ShanWan Wireless Gamepad detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);
                
                return new UsbShanWanWirelessGamepad();
            }

            #endregion

            #region GameStop PC Advanced Controller

            if (vid == 0x11FF && pid == 0x3331)
            {
                Log.InfoFormat(
                    "GameStop PC Advanced Controller detected [VID: {0:X4}] [PID: {1:X4}]",
                    vid, pid);
                
                return new UsbGameStopPcAdvanced();
            }

            #endregion

            return null;
        }

        public override bool Open(string devicePath)
        {
            var retval = base.Open(devicePath);

            // since these devices have no MAC address, generate one
            m_Mac = MacAddressGenerator.NewMacAddress;

            return retval;
        }

        public override bool Stop()
        {
            var retval = base.Stop();

            // pad reservation not supported
            m_State = DsState.Disconnected;

            return retval;
        }

        protected override void Process(DateTime now)
        {
            // ignore
        }

        public override bool Pair(byte[] master)
        {
            return false; // ignore
        }

        public override bool Rumble(byte large, byte small)
        {
            return false; // ignore
        }
    }
}
