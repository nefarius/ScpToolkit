using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ScpMonitor
{
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
            return (from DictionaryEntry entry in property.Attributes select (Attribute)entry.Value).OfType<UserScopedSettingAttribute>().Any();
        }

        private string GetSubKeyPath()
        {
            return "Software\\" + Application.CompanyName + "\\" + Application.ProductName;
        }
    }
}
