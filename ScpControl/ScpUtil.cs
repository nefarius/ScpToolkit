using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
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

    public class Ds3ButtonAnalogDirectionMap : SortedDictionary<Ds3Button, Ds3AnalogDirection>
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
        int HCI_Disconnect(BthHandle handle);
        int HID_Command(byte[] handle, byte[] channel, byte[] data);
    }

    public class DsNull : IDsDevice
    {
        public DsNull(DsPadId padId)
        {
            PadId = padId;
        }

        public DsPadId PadId { get; set; }

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
        public ArrivalEventArgs(IDsDevice device)
        {
            this.Device = device;
        }

        public IDsDevice Device { get; private set; }

        public bool Handled { get; set; }
    }

    public class ReportEventArgs : EventArgs
    {
        public const int Length = 96;
        private volatile byte[] _report = new byte[Length];

        public ReportEventArgs()
        {
            Pad = DsPadId.None;
        }

        public ReportEventArgs(DsPadId pad)
        {
            this.Pad = pad;
        }

        public DsPadId Pad { get; set; }

        public byte[] Report
        {
            get { return _report; }
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