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
        private string _bluetoothField;
        private string _dualShock3Field;
        private string _serviceField;
        private string _virtualBusField;

        /// <remarks />
        public string Service
        {
            get { return _serviceField; }
            set { _serviceField = value; }
        }

        /// <remarks />
        public string Bluetooth
        {
            get { return _bluetoothField; }
            set { _bluetoothField = value; }
        }

        /// <remarks />
        public string DualShock3
        {
            get { return _dualShock3Field; }
            set { _dualShock3Field = value; }
        }

        /// <remarks />
        public string VirtualBus
        {
            get { return _virtualBusField; }
            set { _virtualBusField = value; }
        }

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