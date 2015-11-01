using System;

namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region L2CAP Commands

        /// <summary>
        ///     Writes a Logical Link Control and Adaption Layer Protocol command packet to a Bluetooth host device.
        /// </summary>
        /// <param name="handle">The USB device handle.</param>
        /// <param name="data">The payload (command) to send.</param>
        /// <returns></returns>
        private int L2CAP_Command(byte[] handle, byte[] data)
        {
            var transfered = 0;
            var buffer = new byte[64];

            buffer[0] = handle[0];
            buffer[1] = (byte)(handle[1] | 0x20);
            buffer[2] = (byte)(data.Length + 4);
            buffer[3] = 0x00;
            buffer[4] = (byte)(data.Length);
            buffer[5] = 0x00;
            buffer[6] = 0x01;
            buffer[7] = 0x00;

            // add payload to buffer
            Buffer.BlockCopy(data, 0, buffer, 8, data.Length);

            // send data to device
            WriteBulkPipe(buffer, data.Length + 8, ref transfered);

            // return transfered byte count
            return transfered;
        }

        /// <summary>
        ///     Connection request packets are sent to create a channel between two devices.
        /// </summary>
        /// <remarks>BLUETOOTH SPECIFICATION Version 1.0 A page 278</remarks>
        /// <param name="handle">The HCI handle.</param>
        /// <param name="id">The packet id.</param>
        /// <param name="dcid">The Destination Channel Identifier.</param>
        /// <param name="psm">The Protocol and Service Multiplexer to request.</param>
        /// <returns>The byte count sent to the Bluetooth host.</returns>
        private int L2CAP_Connection_Request(byte[] handle, byte id, byte[] dcid, L2CAP.PSM psm)
        {
            var buffer = new byte[8];

            buffer[0] = 0x02;
            buffer[1] = id;
            buffer[2] = 0x04;
            buffer[3] = 0x00;
            buffer[4] = (byte)psm;
            buffer[5] = 0x00;
            buffer[6] = dcid[0];
            buffer[7] = dcid[1];

            return L2CAP_Command(handle, buffer);
        }

        /// <summary>
        ///     When a unit receives a Connection Request packet, it must send a Connection Response packet.
        /// </summary>
        /// <remarks>BLUETOOTH SPECIFICATION Version 1.0 A page 279</remarks>
        /// <remarks>BLUETOOTH SPECIFICATION Version 1.0 A page 278</remarks>
        /// <param name="handle">The HCI handle.</param>
        /// <param name="id">The packet id.</param>
        /// <param name="dcid">The Destination Channel Identifier.</param>
        /// <param name="scid">The Source Channel Identifier.</param>
        /// <param name="result">The result of the connection request.</param>
        /// <param name="status">Only defined for Result = Pending. Indicates the status of the connection.</param>
        /// <returns>The byte count sent to the Bluetooth host.</returns>
        private int L2CAP_Connection_Response(byte[] handle, byte id, byte[] dcid, byte[] scid,
            L2CAP.ConnectionResponseResult result,
            L2CAP.ConnectionResponseStatus status = L2CAP.ConnectionResponseStatus.NoFurtherInformationAvailable)
        {
            var buffer = new byte[12];

            buffer[0] = 0x03;
            buffer[1] = id;
            buffer[2] = 0x08;
            buffer[3] = 0x00;
            buffer[4] = scid[0];
            buffer[5] = scid[1];
            buffer[6] = dcid[0];
            buffer[7] = dcid[1];
            buffer[8] = (byte) result;
            buffer[9] = 0x00;

            if (result == L2CAP.ConnectionResponseResult.ConnectionPending)
            {
                buffer[10] = (byte) status;
                buffer[11] = 0x00;
            }
            else
            {
                buffer[10] = 0x00;
                buffer[11] = 0x00;
            }

            return L2CAP_Command(handle, buffer);
        }

        private int L2CAP_Configuration_Request(byte[] handle, byte id, byte[] dcid, bool mtu = true)
        {
            var buffer = new byte[mtu ? 12 : 8];

            buffer[0] = 0x04;
            buffer[1] = id;
            buffer[2] = (byte)(mtu ? 0x08 : 0x04);
            buffer[3] = 0x00;
            buffer[4] = dcid[0];
            buffer[5] = dcid[1];
            buffer[6] = 0x00;
            buffer[7] = 0x00;

            if (mtu)
            {
                buffer[8] = 0x01;
                buffer[9] = 0x02;
                buffer[10] = 0x96;
                buffer[11] = 0x00;
            }

            return L2CAP_Command(handle, buffer);
        }

        private int L2CAP_Configuration_Response(byte[] handle, byte id, byte[] scid)
        {
            var buffer = new byte[10];

            buffer[0] = 0x05;
            buffer[1] = id;
            buffer[2] = 0x06;
            buffer[3] = 0x00;
            buffer[4] = scid[0];
            buffer[5] = scid[1];
            buffer[6] = 0x00;
            buffer[7] = 0x00;
            buffer[8] = 0x00;
            buffer[9] = 0x00;

            return L2CAP_Command(handle, buffer);
        }

        private int L2CAP_Disconnection_Request(byte[] handle, byte id, byte[] dcid, byte[] scid)
        {
            var buffer = new byte[8];

            buffer[0] = 0x06;
            buffer[1] = id;
            buffer[2] = 0x04;
            buffer[3] = 0x00;
            buffer[4] = dcid[0];
            buffer[5] = dcid[1];
            buffer[6] = scid[0];
            buffer[7] = scid[1];

            return L2CAP_Command(handle, buffer);
        }

        private int L2CAP_Disconnection_Response(byte[] handle, byte id, byte[] dcid, byte[] scid)
        {
            var buffer = new byte[8];

            buffer[0] = 0x07;
            buffer[1] = id;
            buffer[2] = 0x04;
            buffer[3] = 0x00;
            buffer[4] = dcid[0];
            buffer[5] = dcid[1];
            buffer[6] = scid[0];
            buffer[7] = scid[1];

            return L2CAP_Command(handle, buffer);
        }

        #endregion
    }
}
