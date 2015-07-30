namespace ScpControl.ScpCore
{
    /// <summary>
    ///     Logical link control and adaptation protocol (L2CAP)
    /// </summary>
    public static class L2CAP
    {
        public enum Code : byte
        {
            L2CAP_Reserved = 0x00,
            L2CAP_Command_Reject = 0x01,
            L2CAP_Connection_Request = 0x02,
            L2CAP_Connection_Response = 0x03,
            L2CAP_Configuration_Request = 0x04,
            L2CAP_Configuration_Response = 0x05,
            L2CAP_Disconnection_Request = 0x06,
            L2CAP_Disconnection_Response = 0x07,
            L2CAP_Echo_Request = 0x08,
            L2CAP_Echo_Response = 0x09,
            L2CAP_Information_Request = 0x0A,
            L2CAP_Information_Response = 0x0B
        }

        public enum PSM
        {
            HID_Service = 0x01,
            HID_Command = 0x11,
            HID_Interrupt = 0x13
        }
    }

}
