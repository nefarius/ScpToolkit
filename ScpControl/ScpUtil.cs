using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ScpControl
{
    public enum DsOffset
    {
        Pad = 0,
        State = 1,
        Battery = 2,
        Connection = 3,
        Model = 89,
        Address = 90
    };

    public enum DsState
    {
        Disconnected = 0x00,
        Reserved = 0x01,
        Connected = 0x02
    };

    public enum DsConnection
    {
        None = 0x00,
        USB = 0x01,
        BTH = 0x02
    };

    public enum DsBattery : byte
    {
        None = 0x00,
        Dieing = 0x01,
        Low = 0x02,
        Medium = 0x03,
        High = 0x04,
        Full = 0x05,
        Charging = 0xEE,
        Charged = 0xEF
    };

    public enum DsPadId : byte
    {
        None = 0xFF,
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        All = 0x04
    };

    public enum DsModel : byte
    {
        None = 0,
        DS3 = 1,
        DS4 = 2
    }

    public enum DsMatch
    {
        None = 0,
        Global = 1,
        Pad = 2,
        Mac = 3
    }

    [Flags]
    public enum X360Button : uint
    {
        None = 0,

        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,

        Start = 1 << 4,
        Back = 1 << 5,
        LS = 1 << 6,
        RS = 1 << 7,

        LB = 1 << 8,
        RB = 1 << 9,

        Guide = 1 << 10,

        A = 1 << 12,
        B = 1 << 13,
        X = 1 << 14,
        Y = 1 << 15
    }

    public enum X360Axis
    {
        BT_Lo = 10,
        BT_Hi = 11,

        LT = 12,
        RT = 13,

        LX_Lo = 14,
        LX_Hi = 15,
        LY_Lo = 16,
        LY_Hi = 17,

        RX_Lo = 18,
        RX_Hi = 19,
        RY_Lo = 20,
        RY_Hi = 21
    }

    [Flags]
    public enum Ds3Button : uint
    {
        None = 0,

        Select = 1 << 0,
        L3 = 1 << 1,
        R3 = 1 << 2,
        Start = 1 << 3,

        Up = 1 << 4,
        Right = 1 << 5,
        Down = 1 << 6,
        Left = 1 << 7,

        L2 = 1 << 8,
        R2 = 1 << 9,
        L1 = 1 << 10,
        R1 = 1 << 11,

        Triangle = 1 << 12,
        Circle = 1 << 13,
        Cross = 1 << 14,
        Square = 1 << 15,

        PS = 1 << 16
    }

    public enum Ds3Axis
    {
        None = 0,

        LX = 14,
        LY = 15,
        RX = 16,
        RY = 17,

        Up = 22,
        Right = 23,
        Down = 24,
        Left = 25,

        L2 = 26,
        R2 = 27,
        L1 = 28,
        R1 = 29,

        Triangle = 30,
        Circle = 31,
        Cross = 32,
        Square = 33
    }

    [Flags]
    public enum Ds4Button : uint
    {
        None = 0,

        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,

        Square = 1 << 4,
        Cross = 1 << 5,
        Circle = 1 << 6,
        Triangle = 1 << 7,

        L1 = 1 << 8,
        R1 = 1 << 9,
        L2 = 1 << 10,
        R2 = 1 << 11,

        Share = 1 << 12,
        Options = 1 << 13,
        L3 = 1 << 14,
        R3 = 1 << 15,

        PS = 1 << 16,
        TouchPad = 1 << 17
    }

    public enum Ds4Axis
    {
        None = 0,

        LX = 9,
        LY = 10,
        RX = 11,
        RY = 12,

        L2 = 16,
        R2 = 17
    }

    public class Ds3ButtonMap : SortedDictionary<Ds3Button, Ds3Button>
    {
    }

    public class Ds3AxisMap : SortedDictionary<Ds3Axis, Ds3Axis>
    {
    }

    public class Ds3ButtonAxisMap : SortedDictionary<Ds3Button, Ds3Axis>
    {
    }

    public class Ds4ButtonMap : SortedDictionary<Ds4Button, Ds4Button>
    {
    }

    public class Ds4AxisMap : SortedDictionary<Ds4Axis, Ds4Axis>
    {
    }

    public class Ds4ButtonAxisMap : SortedDictionary<Ds4Button, Ds4Axis>
    {
    }

    public class ProfileMap : SortedDictionary<string, Profile>
    {
    }

    [DataContract]
    public class Profile
    {
        private bool m_Default;
        private Ds3AxisMap m_Ds3AxisMap = new Ds3AxisMap();
        private Ds3ButtonMap m_Ds3ButtonMap = new Ds3ButtonMap();
        private Ds4AxisMap m_Ds4AxisMap = new Ds4AxisMap();
        private Ds4ButtonMap m_Ds4ButtonMap = new Ds4ButtonMap();
        private DsMatch m_Match = DsMatch.Global;
        protected string m_Name, m_Type, m_Pad = string.Empty, m_Mac = string.Empty;

        public Profile(string Name)
        {
            m_Name = Name;
        }

        public Profile(bool Default, string Name, string Type, string Qualifier)
        {
            m_Name = Name;
            m_Type = Type;

            m_Default = Default;
            m_Match = (DsMatch) Enum.Parse(typeof (DsMatch), Type, true);

            switch (m_Match)
            {
                case DsMatch.Pad:
                    m_Pad = Qualifier;
                    break;
                case DsMatch.Mac:
                    m_Mac = Qualifier;
                    break;
            }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public string Type
        {
            get { return m_Type; }
        }

        public DsMatch Match
        {
            get { return m_Match; }
        }

        public string Qualifier
        {
            get
            {
                var Qualifier = string.Empty;

                switch (m_Match)
                {
                    case DsMatch.Pad:
                        Qualifier = m_Pad;
                        break;
                    case DsMatch.Mac:
                        Qualifier = m_Mac;
                        break;
                }

                return Qualifier;
            }
        }

        public bool Default
        {
            get { return m_Default; }
            set { m_Default = value; }
        }

        public Ds3ButtonMap Ds3Button
        {
            get { return m_Ds3ButtonMap; }
        }

        public Ds3AxisMap Ds3Axis
        {
            get { return m_Ds3AxisMap; }
        }

        public Ds4ButtonMap Ds4Button
        {
            get { return m_Ds4ButtonMap; }
        }

        public Ds4AxisMap Ds4Axis
        {
            get { return m_Ds4AxisMap; }
        }

        public DsMatch Usage(string Pad, string Mac)
        {
            var Matched = DsMatch.None;

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
        DsPadId PadId { get; set; }

        DsConnection Connection { get; }

        DsState State { get; }

        DsBattery Battery { get; }

        DsModel Model { get; }

        byte[] BD_Address { get; }

        string Local { get; }

        string Remote { get; }

        bool Start();
        bool Rumble(byte large, byte small);
        bool Pair(byte[] master);
        bool Disconnect();
    }

    public interface IBthDevice
    {
        int HCI_Disconnect(BthHandle Handle);
        int HID_Command(byte[] Handle, byte[] Channel, byte[] Data);
    }

    public sealed class BthHandle : IEquatable<BthHandle>, IComparable<BthHandle>
    {
        private readonly byte[] _mHandle = new byte[2] {0x00, 0x00};
        private readonly ushort _mValue;

        public BthHandle(byte Lsb, byte Msb)
        {
            _mHandle[0] = Lsb;
            _mHandle[1] = Msb;

            _mValue = (ushort) (_mHandle[0] | (ushort) (_mHandle[1] << 8));
        }

        public BthHandle(byte[] Handle) : this(Handle[0], Handle[1])
        {
        }

        public BthHandle(ushort Short) : this((byte) ((Short >> 0) & 0xFF), (byte) ((Short >> 8) & 0xFF))
        {
        }

        public byte[] Bytes
        {
            get { return _mHandle; }
        }

        public ushort Short
        {
            get { return _mValue; }
        }

        #region IComparable<BthHandle> Members

        public int CompareTo(BthHandle other)
        {
            return _mValue.CompareTo(other._mValue);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0:X4}", _mValue);
        }

        #region IEquatable<BthHandle> Members

        public bool Equals(BthHandle other)
        {
            return _mValue == other._mValue;
        }

        public bool Equals(byte Lsb, byte Msb)
        {
            return _mHandle[0] == Lsb && _mHandle[1] == Msb;
        }

        public bool Equals(byte[] other)
        {
            return Equals(other[0], other[1]);
        }

        #endregion
    }

    public class DsNull : IDsDevice
    {
        private DsPadId m_PadId = DsPadId.None;

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

        public byte[] BD_Address
        {
            get { return new byte[6]; }
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

        public bool Rumble(byte large, byte small)
        {
            return true;
        }

        public bool Pair(byte[] master)
        {
            return true;
        }

        public bool Disconnect()
        {
            return true;
        }

        public override string ToString()
        {
            return string.Format("Pad {0} : {1}", 1 + (int) PadId, DsState.Disconnected);
        }
    }

    public class ArrivalEventArgs : EventArgs
    {
        public ArrivalEventArgs(IDsDevice Device)
        {
            this.Device = Device;
        }

        public IDsDevice Device { get; set; }

        public bool Handled { get; set; }
    }

    public class ReportEventArgs : EventArgs
    {
        public const int Length = 96;
        private DsPadId m_Pad = DsPadId.None;
        private volatile byte[] m_Report = new byte[Length];

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

        public byte[] Report
        {
            get { return m_Report; }
        }
    }
    
    public class RegistryProvider : SettingsProvider
    {
        public override string ApplicationName
        {
            get { return Application.ProductName; }
            set { }
        }

        public override void Initialize(string name, NameValueCollection Collection)
        {
            base.Initialize(ApplicationName, Collection);
        }

        public override void SetPropertyValues(SettingsContext Context, SettingsPropertyValueCollection PropertyValues)
        {
            foreach (SettingsPropertyValue PropertyValue in PropertyValues)
            {
                if (PropertyValue.IsDirty)
                    GetRegKey(PropertyValue.Property).SetValue(PropertyValue.Name, PropertyValue.SerializedValue);
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext Context,
            SettingsPropertyCollection Properties)
        {
            var values = new SettingsPropertyValueCollection();

            foreach (SettingsProperty Setting in Properties)
            {
                var Value = new SettingsPropertyValue(Setting);

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

        private static bool IsUserScoped(SettingsProperty property)
        {
            return (from DictionaryEntry entry in property.Attributes select (Attribute) entry.Value).OfType<UserScopedSettingAttribute>().Any();
        }

        private string GetSubKeyPath()
        {
            return "Software\\" + Application.CompanyName + "\\" + Application.ProductName;
        }
    }
}