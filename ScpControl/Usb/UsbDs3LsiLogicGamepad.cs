using System;

namespace ScpControl.Usb
{
    public class UsbDs3LsiLogicGamepad : UsbDs3
    {
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
