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

        /// <remarks />
        public string Bluetooth { get; set; }

        /// <remarks />
        public string DualShock3 { get; set; }

        /// <remarks />
        public string VirtualBus { get; set; }

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