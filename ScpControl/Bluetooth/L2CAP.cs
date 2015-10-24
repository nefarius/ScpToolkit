namespace ScpControl.Bluetooth
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

        /// <summary>
        ///     Protocol Service Multiplexer
        /// </summary>
        public enum PSM
        {
            HID_Service = 0x01,
            HID_Command = 0x11,
            HID_Interrupt = 0x13
        }

        /// <summary>
        ///     Possible values of Result field in CONNECTION RESPONSE (CODE 0x03)
        /// </summary>
        /// <remarks>According to the specification, this should be a 2-byte value but currently only the LSB is used.</remarks>
        public enum ConnectionResponseResult : byte
        {
            /// <summary>
            ///     Connection successful.
            /// </summary>
            ConnectionSuccessful = 0x0000,
            /// <summary>
            ///     Connection pending.
            /// </summary>
            ConnectionPending = 0x0001,
            /// <summary>
            ///     Connection refused – PSM not supported.
            /// </summary>
            ConnectionRefusedPsmNotNupported = 0x0002,
            /// <summary>
            ///     Connection refused – security block.
            /// </summary>
            ConnectionRefusedSecurityBlock = 0x0003,
            /// <summary>
            ///     Connection refused – no resources available.
            /// </summary>
            ConnectionRefusedNoResourcesAvailable = 0x0004
        }

        /// <summary>
        ///     Configuration Response Result codes
        /// </summary>
        public enum ConfigurationResponseResult : byte
        {
            /// <summary>
            ///     Success
            /// </summary>
            Success = 0x0000,
            /// <summary>
            ///     Failure – unacceptable parameters
            /// </summary>
            FailureUnacceptableParameters = 0x0001,
            /// <summary>
            ///     Failure – rejected (no reason provided)
            /// </summary>
            FailureRejected = 0x0002,
            /// <summary>
            ///     Failure – unknown options
            /// </summary>
            FailureUnknownOptions = 0x0003
        }
    }
}
