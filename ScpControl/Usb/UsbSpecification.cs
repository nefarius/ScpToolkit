namespace ScpControl.Usb
{
    public enum UsbRequestType : byte
    {
        HostToDevice = 0x00,
        DeviceToHost = 0x01
    }

    /// <summary>
    ///     <see href="http://www.usb.org/developers/hidpage/HID1_11.pdf">Class-Specific Requests</see>
    /// </summary>
    /// <remarks>
    /// Bits specifying characteristics of request. Valid values are 10100001 or 00100001 only based on the following description: 
    /// 7        Data transfer direction
    ///            0 = Host to device
    ///            1 = Device to host
    /// 6..5     Type
    ///            1 = Class
    /// 4..0     Recipient
    ///            1 = Interface
    /// </remarks>
    public enum UsbHidRequestType : byte
    {
        HostToDevice = 0x21,
        DeviceToHost = 0xA1
    }
}
