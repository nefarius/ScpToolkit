using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds3.Replica;

namespace ScpControl.Usb
{
    public partial class UsbHub
    {
        /// <summary>
        ///     Checks if the given USB device is a 3rd party device and applies common workarounds.
        /// </summary>
        /// <param name="current">The device to check.</param>
        /// <param name="instance">The device instance.</param>
        /// <param name="path">The device path.</param>
        /// <returns></returns>
        private static bool Apply3RdPartyWorkaroundsForDs3(ref UsbDevice current, byte instance = 0x00, string path = default(string))
        {
            if (!(current is UsbDs3))
                return true;

            var padId = current.PadId;

            #region Afterglow AP.2 Wireless Controller for PS3 workaround

            // if Afterglow AP.2 Wireless Controller for PS3 is detected...
            if (current.VendorId == 0x0E6F && current.ProductId == 0x0214)
            {
                Log.InfoFormat(
                    "Afterglow AP.2 Wireless Controller for PS3 detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbDs3Afterglow()
                {
                    PadId = padId
                };

                // open and continue plug-in procedure on success
                return (!string.IsNullOrEmpty(path)) ? current.Open(path) : current.Open(instance);
            }

            #endregion

            #region Quad Stick workaround

            // if Quad Stick is detected...
            if (current.VendorId == 0x16D0 && current.ProductId == 0x092B)
            {
                Log.InfoFormat(
                    "Quad Stick detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbDs3QuadStick()
                {
                    PadId = padId
                };

                // open and continue plug-in procedure on success
                return (!string.IsNullOrEmpty(path)) ? current.Open(path) : current.Open(instance);
            }

            #endregion

            return true;
        }
    }
}
