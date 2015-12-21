using System;
namespace ScpControl.Bluetooth
{
    /// <summary>
    ///     Logical link control and adaptation protocol (L2CAP)
    /// </summary>
    public static class L2CAP
    {
        /// <summary>
        ///     Signalling Command Codes
        /// </summary>
        public enum Code : byte
        {
            L2CAP_Reserved = 0x00,
            L2CAP_Command_Reject = 0x01,

            /// <summary>
            ///     A Connection Request packet has been received.
            /// </summary>
            L2CAP_Connection_Request = 0x02,

            /// <summary>
            ///     A Connection Response packet has been received with a positive result indicating that the connection has been
            ///     established.
            /// </summary>
            L2CAP_Connection_Response = 0x03,

            /// <summary>
            ///     A Configuration Request packet has been received indicating the remote endpoint wishes to engage in negotiations
            ///     concerning channel parameters.
            /// </summary>
            L2CAP_Configuration_Request = 0x04,

            /// <summary>
            ///     A Configuration Response packet has been received indicating the remote endpoint agrees with all the parameters
            ///     being negotiated.
            /// </summary>
            L2CAP_Configuration_Response = 0x05,

            /// <summary>
            ///     A Disconnection Request packet has been received and the channel must initiate the disconnection process. Following
            ///     the completion of an L2CAP channel disconnection process, an L2CAP entity should return the corresponding local CID
            ///     to the pool of “unassigned” CIDs.
            /// </summary>
            L2CAP_Disconnection_Request = 0x06,

            /// <summary>
            ///     A Disconnection Response packet has been received. Following the receipt of this signal, the receiving L2CAP entity
            ///     may return the corresponding local CID to the pool of unassigned CIDs. There is no corresponding negative response
            ///     because the Disconnect Request must succeed.
            /// </summary>
            L2CAP_Disconnection_Response = 0x07,
            L2CAP_Echo_Request = 0x08,
            L2CAP_Echo_Response = 0x09,
            L2CAP_Information_Request = 0x0A,
            L2CAP_Information_Response = 0x0B
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
        ///     Only defined for Result = Pending. Indicates the status of the connection.
        /// </summary>
        /// <remarks>According to the specification, this should be a 2-byte value but currently only the LSB is used.</remarks>
        public enum ConnectionResponseStatus : byte
        {
            /// <summary>
            ///     No further information available.
            /// </summary>
            NoFurtherInformationAvailable = 0x0000,
            /// <summary>
            ///     Authentication pending.
            /// </summary>
            AuthenticationPending = 0x0001,
            /// <summary>
            ///     Authorisation pending.
            /// </summary>
            AuthorisationPending = 0x0002
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
    }

    /// <summary>
    ///     Wrapper class for L2CAP packets.
    /// </summary>
    public class L2CapDataPacket
    {
        /// <summary>
        ///     Native (raw) byte buffer.
        /// </summary>
        public byte[] RawBytes { get; private set; }

        public L2CapDataPacket(byte[] buffer)
        {
            RawBytes = buffer;
            Handle = new BthHandle(RawBytes[0], RawBytes[1]);
        }

        public BthHandle Handle { get; private set; }

        /// <summary>
        ///     True if this packet is for the control channel, false otherwise.
        /// </summary>
        public bool IsControlChannel
        {
            get { return (RawBytes[6] == 0x01 && RawBytes[7] == 0x00); }
        }

        /// <summary>
        ///     True if the current <see cref="SignallingCommandCode">SignallingCommandCode</see> is implemented, false otherwise.
        /// </summary>
        public bool IsValidSignallingCommandCode
        {
            get { return Enum.IsDefined(typeof(L2CAP.Code), RawBytes[8]); }
        }

        /// <summary>
        ///     True if the current packet resembles a HID Input Report, false otherwise.
        /// </summary>
        public bool IsHidInputReport
        {
            get { return (RawBytes[8] == 0xA1 && RawBytes[9] == 0x01); }
        }

        /// <summary>
        ///     The current packets Signalling Command Code.
        /// </summary>
        public L2CAP.Code SignallingCommandCode
        {
            get { return (L2CAP.Code)RawBytes[8]; }
        }

        /// <summary>
        ///     The current packets Protocol Service Multiplexer.
        /// </summary>
        public L2CAP.PSM ProtocolServiceMultiplexer
        {
            get
            {
                switch (SignallingCommandCode)
                {
                    case L2CAP.Code.L2CAP_Connection_Request:
                        return (L2CAP.PSM)RawBytes[12];
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        ///     The current packets Source Channel Identifier.
        /// </summary>
        public byte[] SourceChannelIdentifier
        {
            get
            {
                switch (SignallingCommandCode)
                {
                    case L2CAP.Code.L2CAP_Connection_Request:
                    case L2CAP.Code.L2CAP_Disconnection_Request:
                        return new byte[2] { RawBytes[14], RawBytes[15] };
                    case L2CAP.Code.L2CAP_Connection_Response:
                    case L2CAP.Code.L2CAP_Configuration_Request:
                        return new byte[2] { RawBytes[12], RawBytes[13] };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        ///     The current packets Destination Channel Identifier.
        /// </summary>
        public byte[] DestinationChannelIdentifier
        {
            get
            {
                switch (SignallingCommandCode)
                {
                    case L2CAP.Code.L2CAP_Connection_Response:
                        return new byte[2] { RawBytes[14], RawBytes[15] };
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        ///     The current packets Destination Channel Identifier as an unsigned 16-bit integer.
        /// </summary>
        public ushort DestinationChannelIdentifierUInt16
        {
            get { return (ushort)(DestinationChannelIdentifier[1] << 8 | DestinationChannelIdentifier[0]); }
        }

        /// <summary>
        ///     The current packets reference identifier.
        /// </summary>
        public byte ChannelId
        {
            get
            {
                switch (SignallingCommandCode)
                {
                    case L2CAP.Code.L2CAP_Connection_Request:
                    case L2CAP.Code.L2CAP_Connection_Response:
                    case L2CAP.Code.L2CAP_Configuration_Request:
                    case L2CAP.Code.L2CAP_Configuration_Response:
                    case L2CAP.Code.L2CAP_Disconnection_Request:
                        return RawBytes[9];
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        ///     The current commands result.
        /// </summary>
        public byte Result
        {
            get
            {
                switch (SignallingCommandCode)
                {
                    case L2CAP.Code.L2CAP_Connection_Response:
                        return RawBytes[16];
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        ///     Maximum Transmission Unit for each packet.
        /// </summary>
        public ushort MaximumTransmissionUnit
        {
            get
            {
                if (SignallingCommandCode == L2CAP.Code.L2CAP_Configuration_Response
                    && RawBytes[18] == 0x01 && RawBytes[19] == 0x02)
                {
                    return ToShort(RawBytes[20], RawBytes[21]);
                }
                
                return default(ushort);
            }
        }

        /// <summary>
        ///     Converts an unsigned 16-bit integer to a byte array.
        /// </summary>
        /// <param name="source">The 16-bit integer.</param>
        /// <returns>The byte array.</returns>
        public static byte[] UInt16ToBytes(ushort source)
        {
            return new byte[2] { (byte)((source >> 0) & 0xFF), (byte)((source >> 8) & 0xFF) };
        }

        /// <summary>
        ///     Converts two bytes to an unsigned 16-bit integer.
        /// </summary>
        /// <param name="lsb">The Least Significant Byte.</param>
        /// <param name="msb">The Most Significant Byte.</param>
        /// <returns>The resulting value.</returns>
        private static ushort ToShort(byte lsb, byte msb)
        {
            return (ushort) ((msb << 8) | lsb);
        }
    }
}