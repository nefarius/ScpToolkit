using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Microsoft.Win32;
using ScpControl.Bluetooth;
using ScpControl.Shared.Core;

namespace ScpControl
{
    public interface IDsDevice
    {
        DsPadId PadId { get; set; }

        uint? XInputSlot { get; set; }

        DsConnection Connection { get; }

        DsState State { get; }

        DsBattery Battery { get; }

        DsModel Model { get; }

        PhysicalAddress DeviceAddress { get; }

        PhysicalAddress HostAddress { get; }

        bool Start();
        bool Rumble(byte large, byte small);
        bool Pair(PhysicalAddress master);
        bool Disconnect();
        ScpHidReport NewHidReport();
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

        public PhysicalAddress DeviceAddress
        {
            get { return PhysicalAddress.None; }
        }

        public bool Start()
        {
            return true;
        }

        public bool Rumble(byte large, byte small)
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
        
        public ScpHidReport NewHidReport()
        {
            return new ScpHidReport();
        }

        public PhysicalAddress HostAddress
        {
            get { return PhysicalAddress.None; }
        }
        
        public bool Pair(PhysicalAddress master)
        {
            return true;
        }


        public uint? XInputSlot { get; set; }
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

    [Obsolete]
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