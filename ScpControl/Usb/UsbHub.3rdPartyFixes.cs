using ScpControl.Usb.Ds3.Replica;
using ScpControl.Usb.Gamepads;

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

            #region DragonRise Inc. USB Gamepad SNES

            if (current.VendorId == 0x0079 && current.ProductId == 0x0011)
            {
                Log.InfoFormat(
                    "DragonRise Inc. USB Gamepad SNES detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbSnesGamepad()
                {
                    PadId = padId
                };

                // open and continue plug-in procedure on success
                return (!string.IsNullOrEmpty(path)) ? current.Open(path) : current.Open(instance);
            }

            #endregion

            #region LSI Logic Gamepad

            if (current.VendorId == 0x0079 && current.ProductId == 0x0006)
            {
                Log.InfoFormat(
                    "LSI Logic Gamepad detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbLsiLogicGamepad()
                {
                    PadId = padId
                };

                // open and continue plug-in procedure on success
                return (!string.IsNullOrEmpty(path)) ? current.Open(path) : current.Open(instance);
            }

            #endregion

            #region ShanWan Wireless Gamepad

            if (current.VendorId == 0x2563 && current.ProductId == 0x0523)
            {
                Log.InfoFormat(
                    "ShanWan Wireless Gamepad detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbShanWanWirelessGamepad()
                {
                    PadId = padId
                };

                // open and continue plug-in procedure on success
                return (!string.IsNullOrEmpty(path)) ? current.Open(path) : current.Open(instance);
            }

            #endregion

            #region GameStop PC Advanced Controller

            if (current.VendorId == 0x11FF && current.ProductId == 0x3331)
            {
                Log.InfoFormat(
                    "GameStop PC Advanced Controller detected [VID: {0:X4}] [PID: {1:X4}], workaround applied",
                    current.VendorId, current.ProductId);
                // ...close device...
                current.Close();
                // ...and create customized object
                current = new UsbGameStopPcAdvanced()
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
