using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ScpControl.ScpCore
{
    public sealed class BackingStore
    {
        private byte m_Brightness = 0x80;
        private int m_Bus;
        private byte m_DeadL;
        private byte m_DeadR;
        private XmlDocument m_Doc = new XmlDocument();

        private readonly string _mFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                                  Assembly.GetExecutingAssembly().GetName().Name + ".xml");

        private int m_Idle = 600000;
        private int m_Latency = 128;
        private bool m_LED;
        private bool m_LX;
        private bool m_LY;
        private bool m_Native;
        private bool m_Repair;
        private bool m_Rumble;
        private bool m_RX;
        private bool m_RY;
        private bool m_SSP = true;
        private bool m_Triggers;

        public bool LX
        {
            get { return m_LX; }
            set { m_LX = value; }
        }

        public bool LY
        {
            get { return m_LY; }
            set { m_LY = value; }
        }

        public bool RX
        {
            get { return m_RX; }
            set { m_RX = value; }
        }

        public bool RY
        {
            get { return m_RY; }
            set { m_RY = value; }
        }

        public int Idle
        {
            get { return m_Idle; }
            set { m_Idle = value; }
        }

        public bool LED
        {
            get { return m_LED; }
            set { m_LED = value; }
        }

        public bool Rumble
        {
            get { return m_Rumble; }
            set { m_Rumble = value; }
        }

        public bool Triggers
        {
            get { return m_Triggers; }
            set { m_Triggers = value; }
        }

        public int Latency
        {
            get { return m_Latency; }
            set { m_Latency = value; }
        }

        public byte DeadL
        {
            get { return m_DeadL; }
            set { m_DeadL = value; }
        }

        public byte DeadR
        {
            get { return m_DeadR; }
            set { m_DeadR = value; }
        }

        public bool Native
        {
            get { return m_Native; }
            set { m_Native = value; }
        }

        public bool SSP
        {
            get { return m_SSP; }
            set { m_SSP = value; }
        }

        public byte Brightness
        {
            get { return m_Brightness; }
            set { m_Brightness = value; }
        }

        public int Bus
        {
            get { return m_Bus; }
            set { m_Bus = value; }
        }

        public bool Repair
        {
            get { return m_Repair; }
            set { m_Repair = value; }
        }

        private void CreateTextNode(XmlNode Node, string Name, string Text)
        {
            var Item = m_Doc.CreateNode(XmlNodeType.Element, Name, null);

            if (Text.Length > 0)
            {
                var Elem = m_Doc.CreateNode(XmlNodeType.Text, Name, null);

                Elem.Value = Text;
                Item.AppendChild(Elem);
            }
            Node.AppendChild(Item);
        }

        public bool Load()
        {
            var Loaded = true;

            try
            {
                m_Doc.Load(_mFile);

                try
                {
                    var Node = m_Doc.SelectSingleNode("/ScpControl");

                    try
                    {
                        var Item = Node.SelectSingleNode("Idle");
                        int.TryParse(Item.FirstChild.Value, out m_Idle);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("LX");
                        bool.TryParse(Item.FirstChild.Value, out m_LX);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("LY");
                        bool.TryParse(Item.FirstChild.Value, out m_LY);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("RX");
                        bool.TryParse(Item.FirstChild.Value, out m_RX);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("RY");
                        bool.TryParse(Item.FirstChild.Value, out m_RY);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("LED");
                        bool.TryParse(Item.FirstChild.Value, out m_LED);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Rumble");
                        bool.TryParse(Item.FirstChild.Value, out m_Rumble);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Triggers");
                        bool.TryParse(Item.FirstChild.Value, out m_Triggers);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Latency");
                        int.TryParse(Item.FirstChild.Value, out m_Latency);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("DeadL");
                        byte.TryParse(Item.FirstChild.Value, out m_DeadL);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("DeadR");
                        byte.TryParse(Item.FirstChild.Value, out m_DeadR);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Native");
                        bool.TryParse(Item.FirstChild.Value, out m_Native);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("SSP");
                        bool.TryParse(Item.FirstChild.Value, out m_SSP);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Brightness");
                        byte.TryParse(Item.FirstChild.Value, out m_Brightness);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Bus");
                        int.TryParse(Item.FirstChild.Value, out m_Bus);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var Item = Node.SelectSingleNode("Force");
                        bool.TryParse(Item.FirstChild.Value, out m_Repair);
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
            }
            catch
            {
                Loaded = false;
            }

            return Loaded;
        }

        public bool Save()
        {
            var Saved = true;

            try
            {
                XmlNode Node;

                m_Doc.RemoveAll();

                Node = m_Doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                m_Doc.AppendChild(Node);

                Node = m_Doc.CreateComment(string.Format(" ScpControl Configuration Data. {0} ", DateTime.Now));
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

                    CreateTextNode(Node, "LED", LED.ToString());
                    CreateTextNode(Node, "Rumble", Rumble.ToString());
                    CreateTextNode(Node, "Triggers", Triggers.ToString());

                    CreateTextNode(Node, "Latency", Latency.ToString());
                    CreateTextNode(Node, "DeadL", DeadL.ToString());
                    CreateTextNode(Node, "DeadR", DeadR.ToString());

                    CreateTextNode(Node, "Native", Native.ToString());
                    CreateTextNode(Node, "SSP", SSP.ToString());

                    CreateTextNode(Node, "Brightness", Brightness.ToString());
                    CreateTextNode(Node, "Bus", Bus.ToString());
                    CreateTextNode(Node, "Force", Repair.ToString());
                }
                m_Doc.AppendChild(Node);

                m_Doc.Save(_mFile);
            }
            catch
            {
                Saved = false;
            }

            return Saved;
        }
    }

}
