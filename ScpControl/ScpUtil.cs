using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using ScpControl.Bluetooth;
using ScpControl.ScpCore;

namespace ScpControl
{
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
        private readonly Ds3AxisMap _ds3AxisMap = new Ds3AxisMap();
        private readonly Ds3ButtonMap _ds3ButtonMap = new Ds3ButtonMap();
        private readonly Ds4AxisMap _ds4AxisMap = new Ds4AxisMap();
        private readonly Ds4ButtonMap _ds4ButtonMap = new Ds4ButtonMap();
        private readonly DsMatch _match = DsMatch.Global;
        private readonly string _pad = string.Empty;
        private readonly string _mac = string.Empty;

        public Profile(string name)
        {
            Name = name;
        }

        public Profile(bool setDefault, string name, string type, string qualifier) : this(name)
        {
            Type = type;

            IsDefault = setDefault;
            _match = (DsMatch) Enum.Parse(typeof (DsMatch), type, true);

            switch (_match)
            {
                case DsMatch.Pad:
                    _pad = qualifier;
                    break;
                case DsMatch.Mac:
                    _mac = qualifier;
                    break;
            }
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public DsMatch Match
        {
            get { return _match; }
        }

        public string Qualifier
        {
            get
            {
                var qualifier = string.Empty;

                switch (_match)
                {
                    case DsMatch.Pad:
                        qualifier = _pad;
                        break;
                    case DsMatch.Mac:
                        qualifier = _mac;
                        break;
                }

                return qualifier;
            }
        }

        public bool IsDefault { get; set; }

        public Ds3ButtonMap Ds3Button
        {
            get { return _ds3ButtonMap; }
        }

        public Ds3AxisMap Ds3Axis
        {
            get { return _ds3AxisMap; }
        }

        public Ds4ButtonMap Ds4Button
        {
            get { return _ds4ButtonMap; }
        }

        public Ds4AxisMap Ds4Axis
        {
            get { return _ds4AxisMap; }
        }

        public DsMatch Usage(string Pad, string Mac)
        {
            var matched = DsMatch.None;

            switch (_match)
            {
                case DsMatch.Mac:
                    if (Mac == _mac) matched = DsMatch.Mac;
                    break;
                case DsMatch.Pad:
                    if (Pad == _pad) matched = DsMatch.Pad;
                    break;
                case DsMatch.Global:
                    if (IsDefault) matched = DsMatch.Global;
                    break;
            }

            return matched;
        }
    }

    public interface IDsDevice
    {
        DsPadId PadId { get; set; }

        DsConnection Connection { get; }

        DsState State { get; }

        DsBattery Battery { get; }

        DsModel Model { get; }

        byte[] BdAddress { get; }

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

        public byte[] BdAddress
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