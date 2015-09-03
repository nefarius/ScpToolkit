using System;
using ScpControl.ScpCore;

namespace ScpControl.Bluetooth
{
    public partial class BthDongle
    {
        #region HCI Commands

        private int HCI_Command(HCI.Command command, byte[] buffer)
        {
            var transfered = 0;

            buffer[0] = (byte)(((uint)command >> 0) & 0xFF);
            buffer[1] = (byte)(((uint)command >> 8) & 0xFF);
            buffer[2] = (byte)(buffer.Length - 3);

            SendTransfer(0x20, 0x00, 0x0000, buffer, ref transfered);

            Log.DebugFormat("<< {0} [{1:X4}]", command, (ushort)command);

            return transfered;
        }

        private int HCI_Accept_Connection_Request(byte[] bdAddr, byte role)
        {
            var buffer = new byte[10];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];
            buffer[9] = role;

            return HCI_Command(HCI.Command.HCI_Accept_Connection_Request, buffer);
        }

        private int HCI_Reject_Connection_Request(byte[] bdAddr, byte reason)
        {
            var buffer = new byte[10];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];
            buffer[9] = reason;

            return HCI_Command(HCI.Command.HCI_Reject_Connection_Request, buffer);
        }

        private int HCI_Remote_Name_Request(byte[] bdAddr)
        {
            var buffer = new byte[13];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];
            buffer[9] = 0x01;
            buffer[10] = 0x00;
            buffer[11] = 0x00;
            buffer[12] = 0x00;

            return HCI_Command(HCI.Command.HCI_Remote_Name_Request, buffer);
        }

        private int HCI_Reset()
        {
            var buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Reset, buffer);
        }

        private int HCI_Write_Scan_Enable()
        {
            var buffer = new byte[4];

            buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Scan_Enable, buffer);
        }

        private int HCI_Read_Local_Version_Info()
        {
            var buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Local_Version_Info, buffer);
        }

        private int HCI_Read_BD_Addr()
        {
            var buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_BD_ADDR, buffer);
        }

        private int HCI_Read_Buffer_Size()
        {
            var buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Read_Buffer_Size, buffer);
        }
        
        private int HCI_Link_Key_Request_Reply(byte[] bdAddr)
        {
            var buffer = new byte[25];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];

            Buffer.BlockCopy(GlobalConfiguration.Instance.BdLink, 0, buffer, 9,
                GlobalConfiguration.Instance.BdLink.Length);

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Reply, buffer);
        }

        private int HCI_Link_Key_Request_Negative_Reply(byte[] bdAddr)
        {
            var buffer = new byte[9];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, buffer);
        }

        private int HCI_PIN_Code_Request_Negative_Reply(byte[] bdAddr)
        {
            var buffer = new byte[16];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];

            return HCI_Command(HCI.Command.HCI_Link_Key_Request_Negative_Reply, buffer);
        }

        private int HCI_Set_Connection_Encryption(BthHandle Handle)
        {
            var buffer = new byte[6];

            buffer[3] = Handle.Bytes[0];
            buffer[4] = (byte)(Handle.Bytes[1] ^ 0x20);
            buffer[5] = 0x01;

            return HCI_Command(HCI.Command.HCI_Set_Connection_Encryption, buffer);
        }

        private int HCI_User_Confirmation_Request_Reply(byte[] bdAddr)
        {
            var buffer = new byte[9];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];

            return HCI_Command(HCI.Command.HCI_User_Confirmation_Request_Reply, buffer);
        }

        private int HCI_IO_Capability_Request_Reply(byte[] bdAddr)
        {
            var buffer = new byte[12];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];
            buffer[9] = 0x01;
            buffer[10] = 0x00;
            buffer[11] = 0x05;

            return HCI_Command(HCI.Command.HCI_IO_Capability_Request_Reply, buffer);
        }

        private int HCI_Create_Connection(byte[] bdAddr, byte[] offset)
        {
            var buffer = new byte[16];

            buffer[3] = bdAddr[0];
            buffer[4] = bdAddr[1];
            buffer[5] = bdAddr[2];
            buffer[6] = bdAddr[3];
            buffer[7] = bdAddr[4];
            buffer[8] = bdAddr[5];
            buffer[9] = 0x18;
            buffer[10] = 0xCC;
            buffer[11] = 0x01;
            buffer[12] = 0x00;
            buffer[13] = offset[0];
            buffer[14] = (byte)(offset[1] | 0x80);
            buffer[15] = 0x01;

            return HCI_Command(HCI.Command.HCI_Create_Connection, buffer);
        }

        private int HCI_Set_Event_Mask()
        {
            var buffer = new byte[11];
            // 00 25 5F FF FF FF FF FF
            buffer[3] = 0xFF;
            buffer[4] = 0xFF;
            buffer[5] = 0xFF;
            buffer[6] = 0xFF;
            buffer[7] = 0xFF;
            buffer[8] = 0x5F; // 0xFF;
            buffer[9] = 0x25; // 0xBF;
            buffer[10] = 0x00; // 0x3D;

            return HCI_Command(HCI.Command.HCI_Set_Event_Mask, buffer);
        }

        private int HCI_Write_Local_Name()
        {
            var buffer = new byte[251];

            buffer[3] = 0x45;
            buffer[4] = 0x4E;
            buffer[5] = 0x54;
            buffer[6] = 0x52;
            buffer[7] = 0x4F;
            buffer[8] = 0x50;
            buffer[9] = 0x59;

            return HCI_Command(HCI.Command.HCI_Write_Local_Name, buffer);
        }

        private int HCI_Write_Extended_Inquiry_Response()
        {
            var buffer = new byte[244];

            buffer[3] = 0x00;
            buffer[4] = 0x08;
            buffer[5] = 0x09;
            buffer[6] = 0x45;
            buffer[7] = 0x4E;
            buffer[8] = 0x54;
            buffer[9] = 0x52;
            buffer[10] = 0x4F;
            buffer[11] = 0x50;
            buffer[12] = 0x59;
            buffer[13] = 0x02;
            buffer[14] = 0x0A;

            return HCI_Command(HCI.Command.HCI_Write_Extended_Inquiry_Response, buffer);
        }

        private int HCI_Write_Class_of_Device()
        {
            var buffer = new byte[6];

            buffer[3] = 0x04;
            buffer[4] = 0x02;
            buffer[5] = 0x3E;

            return HCI_Command(HCI.Command.HCI_Write_Class_of_Device, buffer);
        }

        private int HCI_Write_Inquiry_Scan_Type()
        {
            var buffer = new byte[4];

            buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Type, buffer);
        }

        private int HCI_Write_Inquiry_Scan_Activity()
        {
            var buffer = new byte[7];

            buffer[3] = 0x00;
            buffer[4] = 0x08;
            buffer[5] = 0x12;
            buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Scan_Activity, buffer);
        }

        private int HCI_Write_Page_Scan_Type()
        {
            var buffer = new byte[4];

            buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Type, buffer);
        }

        private int HCI_Write_Page_Scan_Activity()
        {
            var buffer = new byte[7];

            buffer[3] = 0x00;
            buffer[4] = 0x04;
            buffer[5] = 0x12;
            buffer[6] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Page_Scan_Activity, buffer);
        }

        private int HCI_Write_Page_Timeout()
        {
            var buffer = new byte[5];

            buffer[3] = 0x00;
            buffer[4] = 0x20;

            return HCI_Command(HCI.Command.HCI_Write_Page_Timeout, buffer);
        }

        private int HCI_Write_Authentication_Enable()
        {
            var buffer = new byte[4];

            buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Authentication_Enable, buffer);
        }

        private int HCI_Write_Simple_Pairing_Mode()
        {
            var buffer = new byte[4];

            buffer[3] = 0x01;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Mode, buffer);
        }

        private int HCI_Write_Simple_Pairing_Debug_Mode()
        {
            var buffer = new byte[4];

            buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Simple_Pairing_Debug_Mode, buffer);
        }

        private int HCI_Write_Inquiry_Mode()
        {
            var buffer = new byte[4];

            buffer[3] = 0x02;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Mode, buffer);
        }

        private int HCI_Write_Inquiry_Transmit_Power_Level()
        {
            var buffer = new byte[4];

            buffer[3] = 0x00;

            return HCI_Command(HCI.Command.HCI_Write_Inquiry_Transmit_Power_Level, buffer);
        }

        private int HCI_Inquiry()
        {
            var buffer = new byte[8];

            buffer[3] = 0x33;
            buffer[4] = 0x8B;
            buffer[5] = 0x9E;
            buffer[6] = 0x18;
            buffer[7] = 0x00;

            return HCI_Command(HCI.Command.HCI_Inquiry, buffer);
        }

        private int HCI_Inquiry_Cancel()
        {
            var buffer = new byte[3];

            return HCI_Command(HCI.Command.HCI_Inquiry_Cancel, buffer);
        }

        private int HCI_Delete_Stored_Link_Key(byte[] bdAddr)
        {
            var buffer = new byte[10];

            Buffer.BlockCopy(bdAddr, 0, buffer, 3, 6);

            buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Delete_Stored_Link_Key, buffer);
        }

        private int HCI_Write_Stored_Link_Key(byte[] bdAddr, byte[] bdLink)
        {
            var buffer = new byte[26];

            buffer[3] = 0x01;
            
            Buffer.BlockCopy(bdAddr, 0, buffer, 4, 6);
            Buffer.BlockCopy(bdLink, 0, buffer, 10, 16);

            return HCI_Command(HCI.Command.HCI_Write_Stored_Link_Key, buffer);
        }

        private int HCI_Read_Stored_Link_Key(byte[] bdAddr)
        {
            var buffer = new byte[10];

            Buffer.BlockCopy(bdAddr, 0, buffer, 3, 6);

            buffer[9] = 0x00;

            return HCI_Command(HCI.Command.HCI_Read_Stored_Link_Key, buffer);
        }

        public int HCI_Disconnect(BthHandle handle)
        {
            var buffer = new byte[6];

            buffer[3] = handle.Bytes[0];
            buffer[4] = (byte)(handle.Bytes[1] ^ 0x20);
            buffer[5] = 0x13;

            return HCI_Command(HCI.Command.HCI_Disconnect, buffer);
        }

        #endregion
    }
}
