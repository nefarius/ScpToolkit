using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using HidSharp;

namespace ScpControl.Utilities
{
    [Serializable]
    public class GenericGamepadConfig
    {
        public string DevicePath { get; set; }
    }

    [Serializable]
    public class GenericGamepadConfigCollection : List<GenericGamepadConfig>
    {
        public static IList<HidDevice> LocalDevices
        {
            get
            {
                var loader = new HidDeviceLoader();
                return loader.GetDevices().ToList();
            }
        }

        #region Serialization

        public static GenericGamepadConfigCollection LoadFromFile(string file)
        {
            var serializer = new XmlSerializer(typeof(GenericGamepadConfigCollection));
            using (var reader = XmlReader.Create(file))
            {
                return (GenericGamepadConfigCollection)serializer.Deserialize(reader);
            }
        }

        public void SaveToFile(string file)
        {
            var serializer = new XmlSerializer(this.GetType());
            using (var writer = XmlWriter.Create(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        #endregion
    }
}
