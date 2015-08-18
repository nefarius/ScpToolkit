using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl
{
    public partial class XmlMapper : Component
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Ds3ButtonAxisMap Ds3ButtonAxis = new Ds3ButtonAxisMap();
        private readonly Ds4ButtonAxisMap Ds4ButtonAxis = new Ds4ButtonAxisMap();

        private readonly Profile m_Empty = new Profile(true, DsMatch.None.ToString(), DsMatch.Global.ToString(),
            string.Empty);

        private readonly ProfileMap m_Mapper = new ProfileMap();
        protected volatile string m_Active = string.Empty, m_Version = string.Empty, m_Description = string.Empty;
        private volatile bool m_Remapping;

        public XmlMapper()
        {
            InitializeComponent();

            Ds3ButtonAxis[Ds3Button.L1] = Ds3Axis.L1;
            Ds3ButtonAxis[Ds3Button.L2] = Ds3Axis.L2;
            Ds3ButtonAxis[Ds3Button.R1] = Ds3Axis.R1;
            Ds3ButtonAxis[Ds3Button.R2] = Ds3Axis.R2;

            Ds3ButtonAxis[Ds3Button.Triangle] = Ds3Axis.Triangle;
            Ds3ButtonAxis[Ds3Button.Circle] = Ds3Axis.Circle;
            Ds3ButtonAxis[Ds3Button.Cross] = Ds3Axis.Cross;
            Ds3ButtonAxis[Ds3Button.Square] = Ds3Axis.Square;

            Ds3ButtonAxis[Ds3Button.Up] = Ds3Axis.Up;
            Ds3ButtonAxis[Ds3Button.Right] = Ds3Axis.Right;
            Ds3ButtonAxis[Ds3Button.Down] = Ds3Axis.Down;
            Ds3ButtonAxis[Ds3Button.Left] = Ds3Axis.Left;

            Ds4ButtonAxis[Ds4Button.L2] = Ds4Axis.L2;
            Ds4ButtonAxis[Ds4Button.R2] = Ds4Axis.R2;
        }

        public XmlMapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Ds3ButtonAxis[Ds3Button.L1] = Ds3Axis.L1;
            Ds3ButtonAxis[Ds3Button.L2] = Ds3Axis.L2;
            Ds3ButtonAxis[Ds3Button.R1] = Ds3Axis.R1;
            Ds3ButtonAxis[Ds3Button.R2] = Ds3Axis.R2;

            Ds3ButtonAxis[Ds3Button.Triangle] = Ds3Axis.Triangle;
            Ds3ButtonAxis[Ds3Button.Circle] = Ds3Axis.Circle;
            Ds3ButtonAxis[Ds3Button.Cross] = Ds3Axis.Cross;
            Ds3ButtonAxis[Ds3Button.Square] = Ds3Axis.Square;

            Ds3ButtonAxis[Ds3Button.Up] = Ds3Axis.Up;
            Ds3ButtonAxis[Ds3Button.Right] = Ds3Axis.Right;
            Ds3ButtonAxis[Ds3Button.Down] = Ds3Axis.Down;
            Ds3ButtonAxis[Ds3Button.Left] = Ds3Axis.Left;

            Ds4ButtonAxis[Ds4Button.L2] = Ds4Axis.L2;
            Ds4ButtonAxis[Ds4Button.R2] = Ds4Axis.R2;
        }

        public virtual string[] Profiles
        {
            get
            {
                var Index = 0;
                var List = new string[m_Mapper.Count];

                foreach (var Item in m_Mapper.Keys)
                {
                    List[Index++] = Item;
                }

                return List;
            }
        }

        public virtual string Active
        {
            get { return m_Active; }
        }

        public virtual ProfileMap Map
        {
            get { return m_Mapper; }
        }

        private Profile Find(string Mac, int PadId)
        {
            var Found = m_Empty;
            var Pad = ((DsPadId) PadId).ToString();

            DsMatch Current = DsMatch.None, Target = DsMatch.None;

            foreach (var Item in m_Mapper.Values)
            {
                Target = Item.Usage(Pad, Mac);

                if (Target > Current)
                {
                    Found = Item;
                    Current = Target;
                }
            }

            return Found;
        }

        private void CreateTextNode(XmlDocument Doc, XmlNode Node, string Name, string Text)
        {
            var Item = Doc.CreateNode(XmlNodeType.Element, Name, null);

            if (Text.Length > 0)
            {
                var Elem = Doc.CreateNode(XmlNodeType.Text, Name, null);

                Elem.Value = Text;
                Item.AppendChild(Elem);
            }

            Node.AppendChild(Item);
        }

        public virtual bool Initialize(XmlDocument Map)
        {
            try
            {
                m_Remapping = false;
                m_Mapper.Clear();

                var Node = Map.SelectSingleNode("/ScpMapper");

                m_Description = Node.SelectSingleNode("Description").FirstChild.Value;
                m_Version = Node.SelectSingleNode("Version").FirstChild.Value;
                m_Active = Node.SelectSingleNode("Active").FirstChild.Value;

                foreach (XmlNode ProfileNode in Node.SelectNodes("Mapping/Profile"))
                {
                    var Name = ProfileNode.SelectSingleNode("Name").FirstChild.Value;
                    var Type = ProfileNode.SelectSingleNode("Type").FirstChild.Value;

                    var Qualifier = string.Empty;

                    var QualifierNode = ProfileNode.SelectSingleNode("Value");

                    if (QualifierNode.HasChildNodes)
                    {
                        Qualifier = QualifierNode.FirstChild.Value;
                    }


                    var Profile = new Profile(Name == m_Active, Name, Type, Qualifier);

                    foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS3/Button"))
                    {
                        foreach (XmlNode Item in Mapping.ChildNodes)
                        {
                            var Target = (Ds3Button) Enum.Parse(typeof (Ds3Button), Item.ParentNode.Name);
                            var Mapped = (Ds3Button) Enum.Parse(typeof (Ds3Button), Item.Value);

                            Profile.Ds3Button[Target] = Mapped;
                        }
                    }

                    foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS3/Axis"))
                    {
                        foreach (XmlNode Item in Mapping.ChildNodes)
                        {
                            var Target = (Ds3Axis) Enum.Parse(typeof (Ds3Axis), Item.ParentNode.Name);
                            var Mapped = (Ds3Axis) Enum.Parse(typeof (Ds3Axis), Item.Value);

                            Profile.Ds3Axis[Target] = Mapped;
                        }
                    }

                    foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS4/Button"))
                    {
                        foreach (XmlNode Item in Mapping.ChildNodes)
                        {
                            var Target = (Ds4Button) Enum.Parse(typeof (Ds4Button), Item.ParentNode.Name);
                            var Mapped = (Ds4Button) Enum.Parse(typeof (Ds4Button), Item.Value);

                            Profile.Ds4Button[Target] = Mapped;
                        }
                    }

                    foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS4/Axis"))
                    {
                        foreach (XmlNode Item in Mapping.ChildNodes)
                        {
                            var Target = (Ds4Axis) Enum.Parse(typeof (Ds4Axis), Item.ParentNode.Name);
                            var Mapped = (Ds4Axis) Enum.Parse(typeof (Ds4Axis), Item.Value);

                            Profile.Ds4Axis[Target] = Mapped;
                        }
                    }

                    m_Mapper[Profile.Name] = Profile;
                }

                var Mappings = m_Mapper[m_Active].Ds3Button.Count + m_Mapper[m_Active].Ds3Axis.Count +
                               m_Mapper[m_Active].Ds4Button.Count + m_Mapper[m_Active].Ds4Axis.Count;
                Log.DebugFormat("## Mapper.Initialize() - Profiles [{0}] Active [{1}] Mappings [{2}]", m_Mapper.Count,
                    m_Active, Mappings);

                m_Remapping = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error in XML Initialize: {0}", ex);
            }

            return m_Remapping;
        }

        public virtual bool Shutdown()
        {
            m_Remapping = false;

            Log.Debug("## Mapper.Shutdown()");
            return true;
        }

        public virtual bool Construct(ref XmlDocument Map)
        {
            var Constructed = true;

            try
            {
                XmlNode Node;
                var Doc = new XmlDocument();

                Node = Doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                Doc.AppendChild(Node);

                Node = Doc.CreateComment(string.Format(" ScpMapper Configuration Data. {0} ", DateTime.Now));
                Doc.AppendChild(Node);

                Node = Doc.CreateNode(XmlNodeType.Element, "ScpMapper", null);
                {
                    CreateTextNode(Doc, Node, "Description", "SCP Mapping File");
                    CreateTextNode(Doc, Node, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                    var Mapping = Doc.CreateNode(XmlNodeType.Element, "Mapping", null);
                    {
                        foreach (var Item in m_Mapper.Values)
                        {
                            if (Item.IsDefault) CreateTextNode(Doc, Node, "Active", Item.Name);

                            var Profile = Doc.CreateNode(XmlNodeType.Element, "Profile", null);
                            {
                                CreateTextNode(Doc, Profile, "Name", Item.Name);
                                CreateTextNode(Doc, Profile, "Type", Item.Type);
                                CreateTextNode(Doc, Profile, "Value", Item.Qualifier);

                                var Ds3 = Doc.CreateNode(XmlNodeType.Element, DsModel.DS3.ToString(), null);
                                {
                                    var Button = Doc.CreateNode(XmlNodeType.Element, "Button", null);
                                    {
                                        foreach (var Ds3Button in Item.Ds3Button.Keys)
                                        {
                                            CreateTextNode(Doc, Button, Ds3Button.ToString(),
                                                Item.Ds3Button[Ds3Button].ToString());
                                        }
                                    }
                                    Ds3.AppendChild(Button);

                                    var Axis = Doc.CreateNode(XmlNodeType.Element, "Axis", null);
                                    {
                                        foreach (var Ds3Axis in Item.Ds3Axis.Keys)
                                        {
                                            CreateTextNode(Doc, Axis, Ds3Axis.ToString(),
                                                Item.Ds3Axis[Ds3Axis].ToString());
                                        }
                                    }
                                    Ds3.AppendChild(Axis);
                                }
                                Profile.AppendChild(Ds3);

                                var Ds4 = Doc.CreateNode(XmlNodeType.Element, DsModel.DS4.ToString(), null);
                                {
                                    var Button = Doc.CreateNode(XmlNodeType.Element, "Button", null);
                                    {
                                        foreach (var Ds4Button in Item.Ds4Button.Keys)
                                        {
                                            CreateTextNode(Doc, Button, Ds4Button.ToString(),
                                                Item.Ds4Button[Ds4Button].ToString());
                                        }
                                    }
                                    Ds4.AppendChild(Button);

                                    var Axis = Doc.CreateNode(XmlNodeType.Element, "Axis", null);
                                    {
                                        foreach (var Ds4Axis in Item.Ds4Axis.Keys)
                                        {
                                            CreateTextNode(Doc, Axis, Ds4Axis.ToString(),
                                                Item.Ds4Axis[Ds4Axis].ToString());
                                        }
                                    }
                                    Ds4.AppendChild(Axis);
                                }
                                Profile.AppendChild(Ds4);
                            }
                            Mapping.AppendChild(Profile);
                        }
                    }
                    Node.AppendChild(Mapping);
                }
                Doc.AppendChild(Node);

                Map = Doc;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
                Constructed = false;
            }

            return Constructed;
        }

        public virtual bool Remap(DsModel Type, int Pad, string Mac, byte[] Input, byte[] Output)
        {
            var Mapped = false;

            try
            {
                if (m_Remapping)
                {
                    switch (Type)
                    {
                        case DsModel.DS3:
                            Mapped = RemapDs3(Find(Mac, Pad), Input, Output);
                            break;
                        case DsModel.DS4:
                            Mapped = RemapDs4(Find(Mac, Pad), Input, Output);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Mapped;
        }

        public virtual bool RemapDs3(Profile Map, byte[] Input, byte[] Output)
        {
            var Mapped = false;

            try
            {
                Array.Copy(Input, Output, Input.Length);

                // Map Buttons
                var In =
                    (Ds3Button) (uint) ((Input[10] << 0) | (Input[11] << 8) | (Input[12] << 16) | (Input[13] << 24));
                var Out = In;

                foreach (var Item in Map.Ds3Button.Keys) if ((Out & Item) != Ds3Button.None) Out ^= Item;
                foreach (var Item in Map.Ds3Button.Keys) if ((In & Item) != Ds3Button.None) Out |= Map.Ds3Button[Item];

                Output[10] = (byte) ((uint) Out >> 0 & 0xFF);
                Output[11] = (byte) ((uint) Out >> 8 & 0xFF);
                Output[12] = (byte) ((uint) Out >> 16 & 0xFF);
                Output[13] = (byte) ((uint) Out >> 24 & 0xFF);

                // Map Axis
                foreach (var Item in Map.Ds3Axis.Keys)
                {
                    switch (Item)
                    {
                        case Ds3Axis.LX:
                        case Ds3Axis.LY:
                        case Ds3Axis.RX:
                        case Ds3Axis.RY:
                            Output[(uint) Item] = 127; // Centred
                            break;

                        default:
                            Output[(uint) Item] = 0;
                            break;
                    }
                }

                foreach (var Item in Map.Ds3Axis.Keys)
                {
                    if (Map.Ds3Axis[Item] != Ds3Axis.None)
                    {
                        Output[(uint) Map.Ds3Axis[Item]] = Input[(uint) Item];
                    }
                }

                // Fix up Button-Axis Relations
                foreach (var Key in Ds3ButtonAxis.Keys)
                {
                    if ((Out & Key) != Ds3Button.None && Output[(uint) Ds3ButtonAxis[Key]] == 0)
                    {
                        Output[(uint) Ds3ButtonAxis[Key]] = 0xFF;
                    }
                }

                Mapped = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Mapped;
        }

        public virtual bool RemapDs4(Profile Map, byte[] Input, byte[] Output)
        {
            var Mapped = false;

            try
            {
                Array.Copy(Input, Output, Input.Length);

                // Map Buttons
                var In = (Ds4Button) (uint) ((Input[13] << 0) | (Input[14] << 8) | (Input[15] << 16));
                var Out = In;

                foreach (var Item in Map.Ds4Button.Keys) if ((Out & Item) != Ds4Button.None) Out ^= Item;
                foreach (var Item in Map.Ds4Button.Keys) if ((In & Item) != Ds4Button.None) Out |= Map.Ds4Button[Item];

                Output[13] = (byte) ((uint) Out >> 0 & 0xFF);
                Output[14] = (byte) ((uint) Out >> 8 & 0xFF);
                Output[15] = (byte) ((uint) Out >> 16 & 0xFF);

                // Map Axis
                foreach (var Item in Map.Ds4Axis.Keys)
                {
                    switch (Item)
                    {
                        case Ds4Axis.LX:
                        case Ds4Axis.LY:
                        case Ds4Axis.RX:
                        case Ds4Axis.RY:
                            Output[(uint) Item] = 127; // Centred
                            break;
                        default:
                            Output[(uint) Item] = 0;
                            break;
                    }
                }

                foreach (var Item in Map.Ds4Axis.Keys)
                {
                    if (Map.Ds4Axis[Item] != Ds4Axis.None)
                    {
                        Output[(uint) Map.Ds4Axis[Item]] = Input[(uint) Item];
                    }
                }

                // Fix up Button-Axis Relations
                foreach (var Key in Ds4ButtonAxis.Keys)
                {
                    if ((Out & Key) != Ds4Button.None && Output[(uint) Ds4ButtonAxis[Key]] == 0)
                    {
                        Output[(uint) Ds4ButtonAxis[Key]] = 0xFF;
                    }
                }


                Mapped = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Mapped;
        }
    }
}