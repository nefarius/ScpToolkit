using ScpControl.ScpCore;

namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region HCI Commands

        private int HCI_Command(HCI.Command Command, byte[] Buffer)
        {
            var Transfered = 0;

            Buffer[0] = (byte)(((uint)Command >> 0) & 0xFF);
            Buffer[1] = (byte)(((uint)Command >> 8) & 0xFF);
            Buffer[2] = (byte)(Buffer.Length - 3);

            SendTransfer(0x20, 0x00, 0x0000, Buffer, ref Transfered);

            Log.DebugFormat("<< {0} [{1:X4}]", Command, (ushort)Command);
            return Transfered;
        }

        private int HCI_Accept_Connection_Request(byte[] BD_Addr, byte Role)
        {
            var Buffer = new byte[10];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = Role;

            return HCI_Command(HCI.Command.HCI_Accept_Connection_Request, Buffer);
        }

        private int HCI_Reject_Connection_Request(byte[] BD_Addr, byte Reason)
        {
            var Buffer = new byte[10];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = Reason;

            return HCI_Command(HCI.Command.HCI_Reject_Connection_Request, Buffer);
        }

        private int HCI_Remote_Name_Request(byte[] BD_Addr)
        {
            var Buffer = new byte[13];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x01;
            Buffer[10] = 0x00;
            Buffer[11] = 0x00;
            Buffer[12] = 0x00;

            return HCI_Command(HCI.Command.HCI_Remote_Name_Request, Buffer);
        }

        private int HCI_Reset()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Reset, Buffer);
        }

        private int HCI_Write_Scan_Enable()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Scan_Enable, Buffer);
        }

        private int HCI_Read_Local_Version_Info()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Local_Version_Info, Buffer);
        }

        private int HCI_Read_BD_Addr()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_BD_ADDR, Buffer);
        }

        private int HCI_Read_Buffer_Size()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Buffer_Size, Buffer);
        }


        private int HCI_Link_Key_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[25];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            for (var Index = 0; Index < GlobalConfiguration.Instance.BdLink.Length; Index++) Buffer[Index + 9] = GlobalConfiguration.Instance.BdLink[Index];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Reply, Buffer);
        }

        private int HCI_Link_Key_Request_Negative_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[9];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, Buffer);
        }

        private int HCI_PIN_Code_Request_Negative_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[16];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, Buffer);
        }

        private int HCI_Set_Connection_Encryption(BthHandle Handle)
        {
            var Buffer = new byte[6];

            Buffer[3] = Handle.Bytes[0];
            Buffer[4] = (byte)(Handle.Bytes[1] ^ 0x20);
            Buffer[5] = 0x01;

            return HCI_Command(HCI.Command.HCI_Set_Connection_Encryption, Buffer);
        }

        private int HCI_User_Confirmation_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[9];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];

            return HCI_Command(HCI.Command.HCI_User_Confirmation_Request_Reply, Buffer);
        }

        private int HCI_IO_Capability_Request_Reply(byte[] BD_Addr)
        {
            var Buffer = new byte[12];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x01;
            Buffer[10] = 0x00;
            Buffer[11] = 0x05;

            return HCI_Command(HCI.Command.HCI_IO_Capability_Request_Reply, Buffer);
        }

        private int HCI_Create_Connection(byte[] BD_Addr, byte[] Offset)
        {
            var Buffer = new byte[16];

            Buffer[3] = BD_Addr[0];
            Buffer[4] = BD_Addr[1];
            Buffer[5] = BD_Addr[2];
            Buffer[6] = BD_Addr[3];
            Buffer[7] = BD_Addr[4];
            Buffer[8] = BD_Addr[5];
            Buffer[9] = 0x18;
            Buffer[10] = 0xCC;
            Buffer[11] = 0x01;
            Buffer[12] = 0x00;
            Buffer[13] = Offset[0];
            Buffer[14] = (byte)(Offset[1] | 0x80);
            Buffer[15] = 0x01;

            return HCI_Command(HCI.Command.HCI_Create_Connection, Buffer);
        }

        private int HCI_Set_Event_Mask()
        {
            var Buffer = new byte[11];
            // 00 25 5F FF FF FF FF FF
            Buffer[3] = 0xFF;
            Buffer[4] = 0xFF;
            Buffer[5] = 0xFF;
            Buffer[6] = 0xFF;
            Buffer[7] = 0xFF;
            Buffer[8] = 0x5F; // 0xFF;
            Buffer[9] = 0x25; // 0xBF;
            Buffer[10] = 0x00; // 0x3D;

            return HCI_Command(HCI.Command.HCI_Set_Event_Mask, Buffer);
        }

        private int HCI_Write_Local_Name()
        {
            var Buffer = new byte[251];

            Buffer[3] = 0x45;
            Buffer[4] = 0x4E;
            Buffer[5] = 0x54;
            Buffer[6] = 0x52;
            Buffer[7] = 0x4F;
            Buffer[8] = 0x50;
            Buffer[9] = 0x59;

            return HCI_Command(HCI.Command.HCI_Write_Local_Name, Buffer);
        }

        private int HCI_Write_Extended_Inquiry_Response()
        {
            var Buffer = new byte[244];

            Buffer[3] = 0x00;
            Buffer[4] = 0x08;
            Buffer[5] = 0x09;
            Buffer[6] = 0x45;
            Buffer[7] = 0x4E;
            Buffer[8] = 0x54;
            Buffer[9] = 0x52;
            Buffer[10] = 0x4F;
            Buffer[11] = 0x50;
            Buffer[12] = 0x59;
            Buffer[13] = 0x02;
            Buffer[14] = 0x0A;

            return HCI_Command(HCI.Command.HCI_Write_Extended_Inquiry_Response, Buffer);
        }

        private int HCI_Write_Class_of_Device()
        {
            var Buffer = new byte[6];

            Buffer[3] = 0x04;
            Buffer[4] = 0x02;
            Buffer[5] = 0x3E;

            return HCI_Command(HCI.Command.HCI_Write_Class_of_Device, Buffer);
        }

        private int HCI_Write_Inquiry_Scan_Type()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Type, Buffer);
        }

        private int HCI_Write_Inquiry_Scan_Activity()
        {
            var Buffer = new byte[7];

            Buffer[3] = 0x00;
            Buffer[4] = 0x08;
            Buffer[5] = 0x12;
            Buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Activity, Buffer);
        }

        private int HCI_Write_Page_Scan_Type()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Type, Buffer);
        }

        private int HCI_Write_Page_Scan_Activity()
        {
            var Buffer = new byte[7];

            Buffer[3] = 0x00;
            Buffer[4] = 0x04;
            Buffer[5] = 0x12;
            Buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Activity, Buffer);
        }

        private int HCI_Write_Page_Timeout()
        {
            var Buffer = new byte[5];

            Buffer[3] = 0x00;
            Buffer[4] = 0x20;

            return HCI_Command(HCI.Command.HCI_Write_Page_Timeout, Buffer);
        }

        private int HCI_Write_Authentication_Enable()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Authentication_Enable, Buffer);
        }

        private int HCI_Write_Simple_Pairing_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Mode, Buffer);
        }

        private int HCI_Write_Simple_Pairing_Debug_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode, Buffer);
        }

        private int HCI_Write_Inquiry_Mode()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Mode, Buffer);
        }

        private int HCI_Write_Inquiry_Transmit_Power_Level()
        {
            var Buffer = new byte[4];

            Buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Transmit_Power_Level, Buffer);
        }

        private int HCI_Inquiry()
        {
            var Buffer = new byte[8];

            Buffer[3] = 0x33;
            Buffer[4] = 0x8B;
            Buffer[5] = 0x9E;
            Buffer[6] = 0x18;
            Buffer[7] = 0x00;

            return HCI_Command(HCI.Command.HCI_Inquiry, Buffer);
        }

        private int HCI_Inquiry_Cancel()
        {
            var Buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Inquiry_Cancel, Buffer);
        }

        private int HCI_Delete_Stored_Link_Key(byte[] BD_Addr)
        {
            var Buffer = new byte[10];

            for (var Index = 0; Index < 6; Index++) Buffer[Index + 3] = BD_Addr[Index];
            Buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Delete_Stored_Link_Key, Buffer);
        }

        private int HCI_Write_Stored_Link_Key(byte[] BD_Addr, byte[] BD_Link)
        {
            var Buffer = new byte[26];

            Buffer[3] = 0x01;
            for (var Index = 0; Index < 6; Index++) Buffer[Index + 4] = BD_Addr[Index];
            for (var Index = 0; Index < 16; Index++) Buffer[Index + 10] = BD_Link[Index];

            return HCI_Command(HCI.Command.HCI_Write_Stored_Link_Key, Buffer);
        }

        private int HCI_Read_Stored_Link_Key(byte[] BD_Addr)
        {
            var Buffer = new byte[10];

            for (var Index = 0; Index < 6; Index++) Buffer[Index + 3] = BD_Addr[Index];
            Buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Read_Stored_Link_Key, Buffer);
        }

        public int HCI_Disconnect(BthHandle Handle)
        {
            var Buffer = new byte[6];

            Buffer[3] = Handle.Bytes[0];
            Buffer[4] = (byte)(Handle.Bytes[1] ^ 0x20);
            Buffer[5] = 0x13;

            return HCI_Command(HCI.Command.HCI_Disconnect, Buffer);
        }

        #endregion

    }
}
