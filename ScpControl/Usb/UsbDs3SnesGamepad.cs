using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScpControl.Usb
{
    /// <summary>
    ///     DragonRise Inc. USB Gamepad SNES
    /// </summary>
    public class UsbDs3SnesGamepad : UsbDs3
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

        protected override void Parse(byte[] report)
        {
            base.Parse(report);
        }
    }
}
