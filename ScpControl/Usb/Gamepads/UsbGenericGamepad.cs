using System;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpControl.Usb.Gamepads
{
    public class UsbGenericGamepad : UsbDs3
    {
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
