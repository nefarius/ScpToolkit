using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Reflection;
using System.Xml;
using System.Runtime.InteropServices;

using System.Configuration;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Specialized;

namespace ScpControl 
{
    public enum DsOffset   : int  { Pad = 0, State = 1, Battery = 2, Connection = 3, Model = 89, Address = 90 };
    public enum DsState           { Disconnected = 0x00, Reserved = 0x01, Connected = 0x02 };
    public enum DsConnection      { None = 0x00, USB = 0x01, BTH = 0x02 };
    public enum DsBattery  : byte { None = 0x00, Dieing = 0x01, Low = 0x02, Medium = 0x03, High = 0x04, Full = 0x05, Charging = 0xEE, Charged = 0xEF };
    public enum DsPadId    : byte { None = 0xFF, One = 0x00, Two = 0x01, Three = 0x02, Four = 0x03, All = 0x04 };
    public enum DsModel    : byte { None = 0, DS3 = 1, DS4 = 2, }
    public enum DsMatch           { None = 0, Global = 1, Pad = 2, Mac = 3, }

    [Flags]
    public enum X360Button : uint 
    {
        None     = 0,

        Up       = 1 <<  0,
        Down     = 1 <<  1,
        Left     = 1 <<  2,
        Right    = 1 <<  3,

        Start    = 1 <<  4,
        Back     = 1 <<  5,
        LS       = 1 <<  6,
        RS       = 1 <<  7,

        LB       = 1 <<  8,
        RB       = 1 <<  9,

        Guide    = 1 << 10,

        A        = 1 << 12,
        B        = 1 << 13,
        X        = 1 << 14,
        Y        = 1 << 15,
    }
    public enum X360Axis 
    {
        BT_Lo    = 10,
        BT_Hi    = 11,

        LT       = 12,
        RT       = 13,

        LX_Lo    = 14,
        LX_Hi    = 15,
        LY_Lo    = 16,
        LY_Hi    = 17,

        RX_Lo    = 18,
        RX_Hi    = 19,
        RY_Lo    = 20,
        RY_Hi    = 21,
    }

    [Flags]
    public enum Ds3Button : uint 
    {
        None     = 0,

        Select   = 1 <<  0,
        L3       = 1 <<  1,
        R3       = 1 <<  2,
        Start    = 1 <<  3,

        Up       = 1 <<  4,
        Right    = 1 <<  5,
        Down     = 1 <<  6,
        Left     = 1 <<  7,

        L2       = 1 <<  8,
        R2       = 1 <<  9,
        L1       = 1 << 10,
        R1       = 1 << 11,

        Triangle = 1 << 12,
        Circle   = 1 << 13,
        Cross    = 1 << 14,
        Square   = 1 << 15,

        PS       = 1 << 16,
    }
    public enum Ds3Axis 
    {
        None     =  0,

        LX       = 14,
        LY       = 15,
        RX       = 16,
        RY       = 17,

        Up       = 22,
        Right    = 23,
        Down     = 24,
        Left     = 25,

        L2       = 26,
        R2       = 27,
        L1       = 28,
        R1       = 29,

        Triangle = 30,
        Circle   = 31,
        Cross    = 32,
        Square   = 33,
    }

    [Flags]
    public enum Ds4Button : uint 
    {
        None     = 0,

        Up       = 1 <<  0,
        Right    = 1 <<  1,
        Down     = 1 <<  2,
        Left     = 1 <<  3,

        Square   = 1 <<  4,
        Cross    = 1 <<  5,
        Circle   = 1 <<  6,
        Triangle = 1 <<  7,

        L1       = 1 <<  8,
        R1       = 1 <<  9,
        L2       = 1 << 10,
        R2       = 1 << 11,

        Share    = 1 << 12,
        Options  = 1 << 13,
        L3       = 1 << 14,
        R3       = 1 << 15,

        PS       = 1 << 16,
        TouchPad = 1 << 17,
    }
    public enum Ds4Axis 
    {
        None   =  0,

        LX     =  9,
        LY     = 10,
        RX     = 11,
        RY     = 12,

        L2     = 16,
        R2     = 17,
    }

    public class Ds3ButtonMap     : SortedDictionary<Ds3Button, Ds3Button> { }
    public class Ds3AxisMap       : SortedDictionary<Ds3Axis,   Ds3Axis>   { }
    public class Ds3ButtonAxisMap : SortedDictionary<Ds3Button, Ds3Axis>   { }

    public class Ds4ButtonMap     : SortedDictionary<Ds4Button, Ds4Button> { }
    public class Ds4AxisMap       : SortedDictionary<Ds4Axis,   Ds4Axis>   { }
    public class Ds4ButtonAxisMap : SortedDictionary<Ds4Button, Ds4Axis>   { }

    public class ProfileMap       : SortedDictionary<String,    Profile>   { }

    public class Profile 
    {
        protected String       m_Name, m_Type, m_Pad = String.Empty, m_Mac = String.Empty;
        protected DsMatch      m_Match   = DsMatch.Global;
        protected Boolean      m_Default = false;

        protected Ds3ButtonMap m_Ds3ButtonMap = new Ds3ButtonMap();
        protected Ds3AxisMap   m_Ds3AxisMap   = new Ds3AxisMap();
        protected Ds4ButtonMap m_Ds4ButtonMap = new Ds4ButtonMap();
        protected Ds4AxisMap   m_Ds4AxisMap   = new Ds4AxisMap();

        public Profile(String Name) 
        {
            m_Name = Name;
        }

        public Profile(Boolean Default, String Name, String Type, String Qualifier) 
        {
            m_Name = Name;
            m_Type = Type;

            m_Default = Default;
            m_Match   = (DsMatch) Enum.Parse(typeof(DsMatch), Type, true);

            switch(m_Match)
            {
                case DsMatch.Pad: m_Pad = Qualifier; break;
                case DsMatch.Mac: m_Mac = Qualifier; break;
            }
        }


        public String Name 
        {
            get { return m_Name; }
        }

        public String Type 
        {
            get { return m_Type; }
        }

        public DsMatch Match    
        {
            get { return m_Match; }
        }

        public String Qualifier 
        {
            get 
            {
                String Qualifier = String.Empty;

                switch(m_Match)
                {
                    case DsMatch.Pad: Qualifier = m_Pad; break;
                    case DsMatch.Mac: Qualifier = m_Mac; break;
                }

                return Qualifier;
            }
        }

        public Boolean Default  
        {
            get { return m_Default; }
            set { m_Default = value; }
        }


        public Ds3ButtonMap Ds3Button 
        {
            get { return m_Ds3ButtonMap; }
        }

        public Ds3AxisMap   Ds3Axis   
        {
            get { return m_Ds3AxisMap; }
        }

        public Ds4ButtonMap Ds4Button 
        {
            get { return m_Ds4ButtonMap; }
        }

        public Ds4AxisMap   Ds4Axis   
        {
            get { return m_Ds4AxisMap; }
        }


        public DsMatch Usage(String Pad, String Mac) 
        {
            DsMatch Matched = DsMatch.None;

            switch (m_Match)
            {
                case DsMatch.Mac:
                    if (Mac == m_Mac) Matched = DsMatch.Mac;
                    break;
                case DsMatch.Pad:
                    if (Pad == m_Pad) Matched = DsMatch.Pad;
                    break;
                case DsMatch.Global:
                    if (m_Default) Matched = DsMatch.Global;
                    break;
            }

            return Matched;
        }
    }

    public interface IDsDevice 
    {
        DsPadId PadId 
        {
            get;
            set;
        }

        DsConnection Connection 
        {
            get;
        }

        DsState State 
        {
            get;
        }

        DsBattery Battery 
        {
            get;
        }

        DsModel Model 
        {
            get;
        }

        Byte[] BD_Address 
        {
            get;
        }

        String Local 
        {
            get;
        }

        String Remote 
        {
            get;
        }

        Boolean Start();

        Boolean Rumble(Byte Large, Byte Small);

        Boolean Pair(Byte[] Master);

        Boolean Disconnect();
    }

    public interface IBthDevice 
    {
        Int32 HCI_Disconnect(BthHandle Handle);

        Int32 HID_Command(Byte[] Handle, Byte[] Channel, Byte[] Data);
    }
        
    public class BthHandle : IEquatable<BthHandle>, IComparable<BthHandle> 
    {
        protected Byte[] m_Handle = new Byte[2] { 0x00, 0x00 };
        protected UInt16 m_Value;

        public BthHandle(Byte Lsb, Byte Msb) 
        {
            m_Handle[0] = Lsb;
            m_Handle[1] = Msb;

            m_Value = (UInt16)(m_Handle[0] | (UInt16)(m_Handle[1] << 8));
        }

        public BthHandle(Byte[] Handle) : this(Handle[0], Handle[1]) 
        {
        }

        public BthHandle(UInt16 Short) : this((Byte)((Short >> 0) & 0xFF), (Byte)((Short >> 8) & 0xFF)) 
        {
        }

        public virtual Byte[] Bytes 
        {
            get { return m_Handle; }
        }

        public virtual UInt16 Short 
        {
            get { return m_Value; }
        }

        public override String ToString() 
        {
            return String.Format("{0:X4}", m_Value);
        }

        #region IEquatable<BthHandle> Members

        public virtual bool Equals(BthHandle other) 
        {
            return m_Value == other.m_Value;
        }

        public virtual bool Equals(Byte Lsb, Byte Msb) 
        {
            return m_Handle[0] == Lsb && m_Handle[1] == Msb;
        }

        public virtual bool Equals(Byte[] other) 
        {
            return Equals(other[0], other[1]);
        }

        #endregion

        #region IComparable<BthHandle> Members

        public virtual int CompareTo(BthHandle other) 
        {
            return m_Value.CompareTo(other.m_Value);
        }

        #endregion
    }

    public class DsNull : IDsDevice 
    {
        protected DsPadId m_PadId = DsPadId.None;

        public DsNull(DsPadId PadId) 
        {
            m_PadId = PadId;
        }

        public DsPadId PadId 
        {
            get { return m_PadId; }
            set { m_PadId = value; }
        }

        public DsConnection Connection 
        {
            get { return DsConnection.None; }
        }

        public DsState State 
        {
            get { return DsState.Disconnected; }
        }

        public DsBattery Battery 
        {
            get { return DsBattery.None; }
        }

        public DsModel Model 
        {
            get { return DsModel.None; }
        }

        public Byte[] BD_Address 
        {
            get { return new Byte[6]; }
        }

        public string Local 
        {
            get { return "00:00:00:00:00:00"; }
        }

        public string Remote 
        {
            get { return "00:00:00:00:00:00"; }
        }

        public bool Start() 
        {
            return true;
        }

        public bool Rumble(Byte Left, Byte Right) 
        {
            return true;
        }

        public bool Pair(Byte[] Master) 
        {
            return true;
        }

        public bool Disconnect() 
        {
            return true;
        }

        public override String ToString() 
        {
            return String.Format("Pad {0} : {1}", 1 + (Int32) PadId, DsState.Disconnected);
        }
    }

    public class ArrivalEventArgs : EventArgs 
    {
        protected IDsDevice m_Device = null;
        protected Boolean m_Handled = false;

        public ArrivalEventArgs(IDsDevice Device) 
        {
            m_Device = Device;
        }

        public IDsDevice Device 
        {
            get { return m_Device; }
            set { m_Device = value; }
        }

        public Boolean Handled 
        {
            get { return m_Handled; }
            set { m_Handled = value; }
        }
    }

    public class DebugEventArgs   : EventArgs 
    {
        protected DateTime m_Time = DateTime.Now;
        protected String m_Data = String.Empty;

        public DebugEventArgs(String Data) 
        {
            m_Data = Data;
        }

        public DateTime Time 
        {
            get { return m_Time; }
        }

        public String Data 
        {
            get { return m_Data; }
        }
    }

    public class ReportEventArgs  : EventArgs 
    {
        public const Int32 Length = 96;

        protected DsPadId m_Pad = DsPadId.None;
        protected volatile Byte[] m_Report = new Byte[Length];

        public ReportEventArgs() 
        {
        }

        public ReportEventArgs(DsPadId Pad) 
        {
            m_Pad = Pad;
        }

        public DsPadId Pad 
        {
            get { return m_Pad; }
            set { m_Pad = value; }
        }

        public Byte[] Report 
        {
            get { return m_Report; }
        }
    }

    public class HCI 
    {
        public enum Event : byte 
        {
            HCI_Inquiry_Complete_EV                         = 0x01,
            HCI_Inquiry_Result_EV                           = 0x02,
            HCI_Connection_Complete_EV                      = 0x03,
            HCI_Connection_Request_EV                       = 0x04,
            HCI_Disconnection_Complete_EV                   = 0x05,
            HCI_Authentication_Complete_EV                  = 0x06,
            HCI_Remote_Name_Request_Complete_EV             = 0x07,
            HCI_Encryption_Change_EV                        = 0x08,
            HCI_Change_Connection_Link_Key_Complete_EV      = 0x09,
            HCI_Master_Link_Key_Complete_EV                 = 0x0A,
            HCI_Read_Remote_Supported_Features_Complete_EV  = 0x0B,
            HCI_Read_Remote_Version_Information_Complete_EV = 0x0C,
            HCI_QoS_Setup_Complete_EV                       = 0x0D,
            HCI_Command_Complete_EV                         = 0x0E,
            HCI_Command_Status_EV                           = 0x0F,
            HCI_Hardware_Error_EV                           = 0x10,
            HCI_Flush_Occurred_EV                           = 0x11,
            HCI_Role_Change_EV                              = 0x12,
            HCI_Number_Of_Completed_Packets_EV              = 0x13,
            HCI_Mode_Change_EV                              = 0x14,
            HCI_Return_Link_Keys_EV                         = 0x15,
            HCI_PIN_Code_Request_EV                         = 0x16,
            HCI_Link_Key_Request_EV                         = 0x17,
            HCI_Link_Key_Notification_EV                    = 0x18,
            HCI_Loopback_Command_EV                         = 0x19,
            HCI_Data_Buffer_Overflow_EV                     = 0x1A,
            HCI_Max_Slots_Change_EV                         = 0x1B,
            HCI_Read_Clock_Offset_Complete_EV               = 0x1C,
            HCI_Connection_Packet_Type_Changed_EV           = 0x1D,
            HCI_QoS_Violation_EV                            = 0x1E,
            HCI_Page_Scan_Repetition_Mode_Change_EV         = 0x20,
            HCI_Flow_Specification_Complete_EV              = 0x21,
            HCI_Inquiry_Result_With_RSSI_EV                 = 0x22,
            HCI_Read_Remote_Extended_Features_Complete_EV   = 0x23,
            HCI_Synchronous_Connection_Complete_EV          = 0x2C,
            HCI_Synchronous_Connection_Changed_EV           = 0x2D,
            HCI_Sniff_Subrating_EV                          = 0x2E,
            HCI_Extended_Inquiry_Result_EV                  = 0x2F,
            HCI_IO_Capability_Request_EV                    = 0x31,
            HCI_IO_Capability_Response_EV                   = 0x32,
            HCI_User_Confirmation_Request_EV                = 0x33,
            HCI_Simple_Pairing_Complete_EV                  = 0x36,
        }

        public enum Command : ushort 
        {
            HCI_Null                               = 0x0000,
            HCI_Accept_Connection_Request          = 0x0409,
            HCI_Reject_Connection_Request          = 0x040A,
            HCI_Remote_Name_Request                = 0x0419,
            HCI_Reset                              = 0x0C03,
            HCI_Write_Scan_Enable                  = 0x0C1A,
            HCI_Read_Buffer_Size                   = 0x1005,
            HCI_Read_BD_ADDR                       = 0x1009,
            HCI_Read_Local_Version_Info            = 0x1001,
            HCI_Create_Connection                  = 0x0405,
            HCI_Disconnect                         = 0x0406,
            HCI_Link_Key_Request_Reply             = 0x040B,
            HCI_Link_Key_Request_Negative_Reply    = 0x040C,
            HCI_PIN_Code_Request_Reply             = 0x040D,
            HCI_PIN_Code_Request_Negative_Reply    = 0x040E,
            HCI_Inquiry                            = 0x0401,
            HCI_Inquiry_Cancel                     = 0x0402,
            HCI_Write_Inquiry_Transmit_Power_Level = 0x0C59,
            HCI_Write_Inquiry_Mode                 = 0x0C45,
            HCI_Write_Simple_Pairing_Mode          = 0x0C56,
            HCI_Write_Simple_Pairing_Debug_Mode    = 0x1804,
            HCI_Write_Authentication_Enable        = 0x0C20,
            HCI_Write_Page_Timeout                 = 0x0C18,
            HCI_Write_Page_Scan_Activity           = 0x0C1C,
            HCI_Write_Page_Scan_Type               = 0x0C47,
            HCI_Write_Inquiry_Scan_Activity        = 0x0C1E,
            HCI_Write_Inquiry_Scan_Type            = 0x0C43,
            HCI_Write_Class_of_Device              = 0x0C24,
            HCI_Write_Extended_Inquiry_Response    = 0x0C52,
            HCI_Write_Local_Name                   = 0x0C13,
            HCI_Set_Event_Mask                     = 0x0C01,
            HCI_IO_Capability_Request_Reply        = 0x042B,
            HCI_User_Confirmation_Request_Reply    = 0x042C,
            HCI_Set_Connection_Encryption          = 0x0413,
            HCI_Authentication_Requested           = 0x0411,
            HCI_Change_Connection_Link_Key         = 0x0415,
            HCI_Read_Stored_Link_Key               = 0x0C0D,
            HCI_Write_Stored_Link_Key              = 0x0C11,
            HCI_Delete_Stored_Link_Key             = 0x0C12,
        }
    }

    public class L2CAP 
    {
        public enum PSM 
        {
            HID_Service   = 0x01,
            HID_Command   = 0x11,
            HID_Interrupt = 0x13,
        }

        public enum Code : byte 
        {
            L2CAP_Reserved               = 0x00,
            L2CAP_Command_Reject         = 0x01,
            L2CAP_Connection_Request     = 0x02,
            L2CAP_Connection_Response    = 0x03,
            L2CAP_Configuration_Request  = 0x04,
            L2CAP_Configuration_Response = 0x05,
            L2CAP_Disconnection_Request  = 0x06,
            L2CAP_Disconnection_Response = 0x07,
            L2CAP_Echo_Request           = 0x08,
            L2CAP_Echo_Response          = 0x09,
            L2CAP_Information_Request    = 0x0A,
            L2CAP_Information_Response   = 0x0B,
        }
    }

    public class Global 
    {
        protected static BackingStore m_Config = new BackingStore();
        protected static Byte[] m_BD_Link = { 0x56, 0xE8, 0x81, 0x38, 0x08, 0x06, 0x51, 0x41, 0xC0, 0x7F, 0x12, 0xAA, 0xD9, 0x66, 0x3C, 0xCE };

        protected static Int32 m_IdleTimeout = 60000;
        protected static Int32 m_Latency = 16;

        public static Boolean FlipLX 
        {
            get { return m_Config.LX; }
            set { m_Config.LX = value; }
        }

        public static Boolean FlipLY 
        {
            get { return m_Config.LY; }
            set { m_Config.LY = value; }
        }

        public static Boolean FlipRX 
        {
            get { return m_Config.RX; }
            set { m_Config.RX = value; }
        }

        public static Boolean FlipRY 
        {
            get { return m_Config.RY; }
            set { m_Config.RY = value; }
        }

        public static Boolean DisableLED 
        {
            get { return m_Config.LED; }
            set { m_Config.LED = value; }
        }

        public static Boolean DisableRumble 
        {
            get { return m_Config.Rumble; }
            set { m_Config.Rumble = value; }
        }

        public static Boolean SwapTriggers 
        {
            get { return m_Config.Triggers; }
            set { m_Config.Triggers = value; }
        }

        public static Boolean DisableLightBar 
        {
            get { return m_Config.Brightness == 0; }
        }

        public static Boolean IdleDisconnect 
        {
            get { return m_Config.Idle != 0; }
        }

        public static Int32 IdleTimeout 
        {
            get { return m_Config.Idle; }
            set { m_Config.Idle = value * m_IdleTimeout; }
        }

        public static Int32 Latency 
        {
            get { return m_Config.Latency; }
            set { m_Config.Latency = value * m_Latency; }
        }

        public static Byte DeadZoneL 
        {
            get { return m_Config.DeadL; }
            set { m_Config.DeadL = value; }
        }

        public static Byte DeadZoneR 
        {
            get { return m_Config.DeadR; }
            set { m_Config.DeadR = value; }
        }

        public static Boolean DisableNative 
        {
            get { return m_Config.Native; }
            set { m_Config.Native = value; }
        }

        public static Boolean DisableSSP 
        {
            get { return m_Config.SSP; }
            set { m_Config.SSP = value; }
        }

        public static Byte Brightness 
        {
            get { return m_Config.Brightness; }
            set { m_Config.Brightness = value; }
        }

        public static Int32 Bus 
        {
            get { return m_Config.Bus; }
            set { m_Config.Bus = value; }
        }

        public static Boolean Repair 
        {
            get { return m_Config.Repair; }
            set { m_Config.Repair = value; }
        }

        public static Byte[] Packed 
        {
            get 
            {
                Byte[] Buffer = new Byte[17];

                Buffer[ 1] = 0x03;
                Buffer[ 2] = (Byte)(IdleTimeout / m_IdleTimeout);
                Buffer[ 3] = (Byte)(FlipLX ? 0x01 : 0x00);
                Buffer[ 4] = (Byte)(FlipLY ? 0x01 : 0x00);
                Buffer[ 5] = (Byte)(FlipRX ? 0x01 : 0x00);
                Buffer[ 6] = (Byte)(FlipRY ? 0x01 : 0x00);
                Buffer[ 7] = (Byte)(DisableLED    ? 0x01 : 0x00);
                Buffer[ 8] = (Byte)(DisableRumble ? 0x01 : 0x00);
                Buffer[ 9] = (Byte)(SwapTriggers  ? 0x01 : 0x00);
                Buffer[10] = (Byte)(Latency / m_Latency);
                Buffer[11] = DeadZoneL;
                Buffer[12] = DeadZoneR;
                Buffer[13] = (Byte)(DisableNative ? 0x01 : 0x00);
                Buffer[14] = (Byte)(DisableSSP    ? 0x01 : 0x00);
                Buffer[15] = Brightness;
                Buffer[16] = (Byte)(Repair         ? 0x01 : 0x00); ;

                return Buffer;
            }
            set 
            {
                try
                {
                    IdleTimeout   = value[2];
                    FlipLX        = value[3] == 0x01;
                    FlipLY        = value[4] == 0x01;
                    FlipRX        = value[5] == 0x01;
                    FlipRY        = value[6] == 0x01;
                    DisableLED    = value[7] == 0x01;
                    DisableRumble = value[8] == 0x01;
                    SwapTriggers  = value[9] == 0x01;
                    Latency       = value[10];
                    DeadZoneL     = value[11];
                    DeadZoneR     = value[12];
                    DisableNative = value[13] == 0x01;
                    DisableSSP    = value[14] == 0x01;
                    Brightness    = value[15];
                    Repair         = value[16] == 0x01;
                }
                catch { }
            }
        }

        public static Byte[] BD_Link 
        {
            get { return m_BD_Link; }
        }

        public static void Load() 
        {
            m_Config.Load();
        }

        public static void Save() 
        {
            m_Config.Save();
        }
    }

    public class BackingStore 
    {
        protected String      m_File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".xml";
        protected XmlDocument m_Doc  = new XmlDocument();

        protected virtual void CreateTextNode(XmlNode Node, String Name, String Text) 
        {
            XmlNode Item = m_Doc.CreateNode(XmlNodeType.Element, Name, null);

            if (Text.Length > 0)
            {
                XmlNode Elem = m_Doc.CreateNode(XmlNodeType.Text, Name, null);

                Elem.Value = Text;
                Item.AppendChild(Elem);
            }
            Node.AppendChild(Item);
        }

        public Boolean Load() 
        {
            Boolean Loaded = true;

            try
            {
                m_Doc.Load(m_File);

                try
                {
                    XmlNode Node = m_Doc.SelectSingleNode("/ScpControl");

                    try { XmlNode Item = Node.SelectSingleNode("Idle"); Int32.TryParse(Item.FirstChild.Value, out m_Idle); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("LX"); Boolean.TryParse(Item.FirstChild.Value, out m_LX); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("LY"); Boolean.TryParse(Item.FirstChild.Value, out m_LY); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("RX"); Boolean.TryParse(Item.FirstChild.Value, out m_RX); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("RY"); Boolean.TryParse(Item.FirstChild.Value, out m_RY); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("LED"); Boolean.TryParse(Item.FirstChild.Value, out m_LED); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Rumble"); Boolean.TryParse(Item.FirstChild.Value, out m_Rumble); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Triggers"); Boolean.TryParse(Item.FirstChild.Value, out m_Triggers); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Latency"); Int32.TryParse(Item.FirstChild.Value, out m_Latency); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("DeadL"); Byte.TryParse(Item.FirstChild.Value, out m_DeadL); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("DeadR"); Byte.TryParse(Item.FirstChild.Value, out m_DeadR); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Native"); Boolean.TryParse(Item.FirstChild.Value, out m_Native); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("SSP"); Boolean.TryParse(Item.FirstChild.Value, out m_SSP); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Brightness"); Byte.TryParse(Item.FirstChild.Value, out m_Brightness); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Bus"); Int32.TryParse(Item.FirstChild.Value, out m_Bus); }
                    catch { }

                    try { XmlNode Item = Node.SelectSingleNode("Force"); Boolean.TryParse(Item.FirstChild.Value, out m_Repair); }
                    catch { }
                }
                catch { }
            }
            catch { Loaded = false; }

            return Loaded;
        }

        public Boolean Save() 
        {
            Boolean Saved = true;

            try
            {
                XmlNode Node;

                m_Doc.RemoveAll();

                Node = m_Doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                m_Doc.AppendChild(Node);

                Node = m_Doc.CreateComment(String.Format(" ScpControl Configuration Data. {0} ", DateTime.Now));
                m_Doc.AppendChild(Node);

                Node = m_Doc.CreateWhitespace("\r\n");
                m_Doc.AppendChild(Node);

                Node = m_Doc.CreateNode(XmlNodeType.Element, "ScpControl", null);
                {
                    CreateTextNode(Node, "Idle", Idle.ToString());

                    CreateTextNode(Node, "LX", LX.ToString());
                    CreateTextNode(Node, "LY", LY.ToString());
                    CreateTextNode(Node, "RX", RX.ToString());
                    CreateTextNode(Node, "RY", RY.ToString());

                    CreateTextNode(Node, "LED",      LED.ToString());
                    CreateTextNode(Node, "Rumble",   Rumble.ToString());
                    CreateTextNode(Node, "Triggers", Triggers.ToString());

                    CreateTextNode(Node, "Latency", Latency.ToString());
                    CreateTextNode(Node, "DeadL",   DeadL.ToString());
                    CreateTextNode(Node, "DeadR",   DeadR.ToString());

                    CreateTextNode(Node, "Native", Native.ToString());
                    CreateTextNode(Node, "SSP",    SSP.ToString());

                    CreateTextNode(Node, "Brightness", Brightness.ToString());
                    CreateTextNode(Node, "Bus",        Bus.ToString());
                    CreateTextNode(Node, "Force",      Repair.ToString());
                }
                m_Doc.AppendChild(Node);

                m_Doc.Save(m_File);
            }
            catch { Saved = false; }

            return Saved;
        }

        protected Boolean m_LX = false;
        public Boolean LX 
        {
            get { return m_LX; }
            set { m_LX = value; }
        }

        protected Boolean m_LY = false;
        public Boolean LY 
        {
            get { return m_LY; }
            set { m_LY = value; }
        }

        protected Boolean m_RX = false;
        public Boolean RX 
        {
            get { return m_RX; }
            set { m_RX = value; }
        }

        protected Boolean m_RY = false;
        public Boolean RY 
        {
            get { return m_RY; }
            set { m_RY = value; }
        }

        protected Int32 m_Idle = 600000;
        public Int32 Idle 
        {
            get { return m_Idle; }
            set { m_Idle = value; }
        }

        protected Boolean m_LED = false;
        public Boolean LED 
        {
            get { return m_LED; }
            set { m_LED = value; }
        }

        protected Boolean m_Rumble = false;
        public Boolean Rumble 
        {
            get { return m_Rumble; }
            set { m_Rumble = value; }
        }

        protected Boolean m_Triggers = false;
        public Boolean Triggers 
        {
            get { return m_Triggers; }
            set { m_Triggers = value; }
        }

        protected Int32 m_Latency = 128;
        public Int32 Latency 
        {
            get { return m_Latency; }
            set { m_Latency = value; }
        }

        protected Byte m_DeadL = 0;
        public Byte DeadL 
        {
            get { return m_DeadL; }
            set { m_DeadL = value; }
        }

        protected Byte m_DeadR = 0;
        public Byte DeadR 
        {
            get { return m_DeadR; }
            set { m_DeadR = value; }
        }

        protected Boolean m_Native = false;
        public Boolean Native 
        {
            get { return m_Native; }
            set { m_Native = value; }
        }

        protected Boolean m_SSP = true;
        public Boolean SSP 
        {
            get { return m_SSP; }
            set { m_SSP = value; }
        }

        protected Byte m_Brightness = 0x80;
        public Byte Brightness 
        {
            get { return m_Brightness; }
            set { m_Brightness = value; }
        }

        protected Int32 m_Bus = 0;
        public Int32 Bus 
        {
            get { return m_Bus; }
            set { m_Bus = value; }
        }

        protected Boolean m_Repair = false;
        public Boolean Repair 
        {
            get { return m_Repair; }
            set { m_Repair = value; }
        }
    }

    public class ThemeUtil 
    {
        [DllImport("UxTheme", CharSet = CharSet.Auto)]
        private static extern Int32 SetWindowTheme(IntPtr hWnd, String appName, String partList);

        [DllImport("User32", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, Int32 wParam, Int32 lParam);

        private const Int32 WM_CHANGEUISTATE = 0x127;
        private const Int32 HIDEFOCUS = 0x10001;

        public static void SetTheme(ListView lv) 
        {
            try
            {
                SetWindowTheme(lv.Handle, "Explorer", null);
                SendMessage(lv.Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch { }
        }

        public static void SetTheme(TreeView tv) 
        {
            try
            {
                SetWindowTheme(tv.Handle, "Explorer", null);
                SendMessage(tv.Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch { }
        }

        public static void UpdateFocus(IntPtr Handle) 
        {
            try
            {
                SendMessage(Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch { }
        }
    }

    public class KbmPost 
    {
        public enum MouseButtons { Left = 0x0002, Right = 0x0008, Middle = 0x0020 };

        protected const Int32 MOUSE_VWHEEL = 0x0800;
        protected const Int32 MOUSE_HWHEEL = 0x1000;
        protected const Int32 WHEEL_DELTA  = 120;

        protected const Int32 MOUSE_MOVE  = 1;

        protected const Int32 VK_STANDARD = 0;
        protected const Int32 VK_EXTENDED = 1;

        protected const Int32 VK_KEYDOWN  = 0;
        protected const Int32 VK_KEYUP    = 2;

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern UInt32 MapVirtualKeyW(UInt32 uCode, UInt32 uMapType);

        public static void Key(Keys Key, Boolean bExtended, Boolean bDown) 
        {
            keybd_event((Byte) Key, (Byte) MapVirtualKeyW((UInt32) Key, 0), (bDown ? VK_KEYDOWN : VK_KEYUP) | (bExtended ? VK_EXTENDED : VK_STANDARD), IntPtr.Zero);
        }

        public static void Move(Int32 dx, Int32 dy) 
        {
            mouse_event(MOUSE_MOVE, dx, dy, 0, IntPtr.Zero);
        }

        public static void Button(MouseButtons Button, Boolean bDown) 
        {
            mouse_event(bDown ? (Int32) Button : (Int32) Button << 1, 0, 0, 0, IntPtr.Zero);
        }

        public static void Wheel(Boolean bVertical, Int32 Clicks) 
        {
            mouse_event(bVertical ? MOUSE_VWHEEL : MOUSE_HWHEEL, 0, 0, Clicks * WHEEL_DELTA, IntPtr.Zero);
        }
    }

    public class RegistryProvider : SettingsProvider 
    {
        public RegistryProvider() { }

        public override string ApplicationName
        {
            get { return Application.ProductName; }
            set { }
        }

        public override void Initialize(String name, NameValueCollection Collection)
        {
            base.Initialize(ApplicationName, Collection);
        }

        public override void SetPropertyValues(SettingsContext Context, SettingsPropertyValueCollection PropertyValues)
        {
            foreach (SettingsPropertyValue PropertyValue in PropertyValues)
            {
                if (PropertyValue.IsDirty) GetRegKey(PropertyValue.Property).SetValue(PropertyValue.Name, PropertyValue.SerializedValue);
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext Context, SettingsPropertyCollection Properties)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            foreach (SettingsProperty Setting in Properties)
            {
                SettingsPropertyValue Value = new SettingsPropertyValue(Setting);

                Value.IsDirty = false;
                Value.SerializedValue = GetRegKey(Setting).GetValue(Setting.Name);
                values.Add(Value);
            }

            return values;
        }

        private RegistryKey GetRegKey(SettingsProperty Property)
        {
            RegistryKey RegistryKey;

            if (IsUserScoped(Property))
            {
                RegistryKey = Registry.CurrentUser;
            }
            else
            {
                RegistryKey = Registry.LocalMachine;
            }

            RegistryKey = RegistryKey.CreateSubKey(GetSubKeyPath());

            return RegistryKey;
        }

        private bool IsUserScoped(SettingsProperty Property)
        {
            foreach (DictionaryEntry Entry in Property.Attributes)
            {
                Attribute Attribute = (Attribute)Entry.Value;

                if (Attribute.GetType() == typeof(UserScopedSettingAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetSubKeyPath()
        {
            return "Software\\" + Application.CompanyName + "\\" + Application.ProductName;
        }
    }
}
