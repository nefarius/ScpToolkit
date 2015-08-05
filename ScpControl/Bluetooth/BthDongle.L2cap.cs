using ScpControl.ScpCore;

namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region L2CAP Commands

        private int L2CAP_Command(byte[] Handle, byte[] Data)
        {
            var Transfered = 0;
            var Buffer = new byte[64];

            Buffer[0] = Handle[0];
            Buffer[1] = (byte)(Handle[1] | 0x20);
            Buffer[2] = (byte)(Data.Length + 4);
            Buffer[3] = 0x00;
            Buffer[4] = (byte)(Data.Length);
            Buffer[5] = 0x00;
            Buffer[6] = 0x01;
            Buffer[7] = 0x00;

            for (var i = 0; i < Data.Length; i++) Buffer[i + 8] = Data[i];

            WriteBulkPipe(Buffer, Data.Length + 8, ref Transfered);
            return Transfered;
        }

        private int L2CAP_Connection_Request(byte[] Handle, byte Id, byte[] DCID, L2CAP.PSM Psm)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x02;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = (byte)Psm;
            Buffer[5] = 0x00;
            Buffer[6] = DCID[0];
            Buffer[7] = DCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Connection_Response(byte[] Handle, byte Id, byte[] DCID, byte[] SCID, byte Result)
        {
            var Buffer = new byte[12];

            Buffer[0] = 0x03;
            Buffer[1] = Id;
            Buffer[2] = 0x08;
            Buffer[3] = 0x00;
            Buffer[4] = SCID[0];
            Buffer[5] = SCID[1];
            Buffer[6] = DCID[0];
            Buffer[7] = DCID[1];
            Buffer[8] = Result;
            Buffer[9] = 0x00;
            Buffer[10] = 0x00;
            Buffer[11] = 0x00;

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Configuration_Request(byte[] Handle, byte Id, byte[] DCID, bool MTU = true)
        {
            var Buffer = new byte[MTU ? 12 : 8];

            Buffer[0] = 0x04;
            Buffer[1] = Id;
            Buffer[2] = (byte)(MTU ? 0x08 : 0x04);
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = 0x00;
            Buffer[7] = 0x00;

            if (MTU)
            {
                Buffer[8] = 0x01;
                Buffer[9] = 0x02;
                Buffer[10] = 0x96;
                Buffer[11] = 0x00;
            }

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Configuration_Response(byte[] Handle, byte Id, byte[] SCID)
        {
            var Buffer = new byte[10];

            Buffer[0] = 0x05;
            Buffer[1] = Id;
            Buffer[2] = 0x06;
            Buffer[3] = 0x00;
            Buffer[4] = SCID[0];
            Buffer[5] = SCID[1];
            Buffer[6] = 0x00;
            Buffer[7] = 0x00;
            Buffer[8] = 0x00;
            Buffer[9] = 0x00;

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Disconnection_Request(byte[] Handle, byte Id, byte[] DCID, byte[] SCID)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x06;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = SCID[0];
            Buffer[7] = SCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        private int L2CAP_Disconnection_Response(byte[] Handle, byte Id, byte[] DCID, byte[] SCID)
        {
            var Buffer = new byte[8];

            Buffer[0] = 0x07;
            Buffer[1] = Id;
            Buffer[2] = 0x04;
            Buffer[3] = 0x00;
            Buffer[4] = DCID[0];
            Buffer[5] = DCID[1];
            Buffer[6] = SCID[0];
            Buffer[7] = SCID[1];

            return L2CAP_Command(Handle, Buffer);
        }

        #endregion
    }
}
