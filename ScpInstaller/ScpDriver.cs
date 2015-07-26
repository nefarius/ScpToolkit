using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ScpDriver
{
    /// <remarks />
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ScpDriver
    {
        /// <remarks />
        public string Service { get; set; }
        public bool UseService { get { return !string.IsNullOrEmpty(Service) && bool.Parse(Service); } }

        /// <remarks />
        public string Bluetooth { get; set; }
        public bool UseBluetooth { get { return !string.IsNullOrEmpty(Bluetooth) && bool.Parse(Bluetooth); } }

        /// <remarks />
        public string DualShock3 { get; set; }
        public bool UseDualShock3 { get { return !string.IsNullOrEmpty(DualShock3) && bool.Parse(DualShock3); } }

        /// <remarks />
        public string VirtualBus { get; set; }
        public bool UseVirtualBus { get { return !string.IsNullOrEmpty(VirtualBus) && bool.Parse(VirtualBus); } }

        public static ScpDriver Deserialize(string file)
        {
            var serializer = new XmlSerializer(typeof (ScpDriver));

            using (var reader = new StreamReader(file))
            {
                return (ScpDriver) serializer.Deserialize(reader);
            }
        }
    }
}