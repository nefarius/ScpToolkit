using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;

namespace ScpControl 
{
    public partial class XmlMapper : Component 
    {        
        public event EventHandler<DebugEventArgs> Debug = null;

        protected virtual void LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            if (Debug != null)
            {
                Debug(this, args);
            }
        }

        protected Profile    m_Empty  = new Profile(true, DsMatch.None.ToString(), DsMatch.Global.ToString(), String.Empty);
        protected ProfileMap m_Mapper = new ProfileMap();

        protected Ds3ButtonAxisMap Ds3ButtonAxis = new Ds3ButtonAxisMap();
        protected Ds4ButtonAxisMap Ds4ButtonAxis = new Ds4ButtonAxisMap();

        protected volatile Boolean m_Remapping = false;
        protected volatile String  m_Active = String.Empty, m_Version = String.Empty, m_Description = String.Empty;

        protected Profile Find(String Mac, Int32 PadId) 
        {
            Profile Found = m_Empty;
            String  Pad   = ((DsPadId) PadId).ToString();

            DsMatch Current = DsMatch.None, Target = DsMatch.None;

            foreach(Profile Item in m_Mapper.Values)
            {
                Target = Item.Usage(Pad, Mac);

                if (Target > Current)
                {
                    Found = Item; Current = Target;
                }
            }

            return Found;
        }

        protected void CreateTextNode(XmlDocument Doc, XmlNode Node, String Name, String Text) 
        {
            XmlNode Item = Doc.CreateNode(XmlNodeType.Element, Name, null);

            if (Text.Length > 0)
            {
                XmlNode Elem = Doc.CreateNode(XmlNodeType.Text, Name, null);

                Elem.Value = Text;
                Item.AppendChild(Elem);
            }

            Node.AppendChild(Item);
        }


        public XmlMapper() 
        {
            InitializeComponent();

            Ds3ButtonAxis[Ds3Button.L1      ] = Ds3Axis.L1;
            Ds3ButtonAxis[Ds3Button.L2      ] = Ds3Axis.L2;
            Ds3ButtonAxis[Ds3Button.R1      ] = Ds3Axis.R1;
            Ds3ButtonAxis[Ds3Button.R2      ] = Ds3Axis.R2;

            Ds3ButtonAxis[Ds3Button.Triangle] = Ds3Axis.Triangle;
            Ds3ButtonAxis[Ds3Button.Circle  ] = Ds3Axis.Circle;
            Ds3ButtonAxis[Ds3Button.Cross   ] = Ds3Axis.Cross;
            Ds3ButtonAxis[Ds3Button.Square  ] = Ds3Axis.Square;

            Ds3ButtonAxis[Ds3Button.Up      ] = Ds3Axis.Up;
            Ds3ButtonAxis[Ds3Button.Right   ] = Ds3Axis.Right;
            Ds3ButtonAxis[Ds3Button.Down    ] = Ds3Axis.Down;
            Ds3ButtonAxis[Ds3Button.Left    ] = Ds3Axis.Left;

            Ds4ButtonAxis[Ds4Button.L2      ] = Ds4Axis.L2;
            Ds4ButtonAxis[Ds4Button.R2      ] = Ds4Axis.R2;
        }

        public XmlMapper(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();

            Ds3ButtonAxis[Ds3Button.L1      ] = Ds3Axis.L1;
            Ds3ButtonAxis[Ds3Button.L2      ] = Ds3Axis.L2;
            Ds3ButtonAxis[Ds3Button.R1      ] = Ds3Axis.R1;
            Ds3ButtonAxis[Ds3Button.R2      ] = Ds3Axis.R2;

            Ds3ButtonAxis[Ds3Button.Triangle] = Ds3Axis.Triangle;
            Ds3ButtonAxis[Ds3Button.Circle  ] = Ds3Axis.Circle;
            Ds3ButtonAxis[Ds3Button.Cross   ] = Ds3Axis.Cross;
            Ds3ButtonAxis[Ds3Button.Square  ] = Ds3Axis.Square;

            Ds3ButtonAxis[Ds3Button.Up      ] = Ds3Axis.Up;
            Ds3ButtonAxis[Ds3Button.Right   ] = Ds3Axis.Right;
            Ds3ButtonAxis[Ds3Button.Down    ] = Ds3Axis.Down;
            Ds3ButtonAxis[Ds3Button.Left    ] = Ds3Axis.Left;

            Ds4ButtonAxis[Ds4Button.L2      ] = Ds4Axis.L2;
            Ds4ButtonAxis[Ds4Button.R2      ] = Ds4Axis.R2;
        }


        public virtual Boolean Initialize(XmlDocument Map) 
        {
            try
            {
                m_Remapping = false; m_Mapper.Clear();

                XmlNode Node = Map.SelectSingleNode("/ScpMapper");

                m_Description = Node.SelectSingleNode("Description").FirstChild.Value;
                m_Version     = Node.SelectSingleNode("Version"    ).FirstChild.Value;
                m_Active      = Node.SelectSingleNode("Active"     ).FirstChild.Value;

                foreach (XmlNode ProfileNode in Node.SelectNodes("Mapping/Profile"))
                {
                    String Name = ProfileNode.SelectSingleNode("Name").FirstChild.Value;
                    String Type = ProfileNode.SelectSingleNode("Type").FirstChild.Value;

                    String Qualifier = String.Empty;

                    try
                    {
                        XmlNode QualifierNode = ProfileNode.SelectSingleNode("Value");

                        if (QualifierNode.HasChildNodes)
                        {
                            Qualifier = QualifierNode.FirstChild.Value;
                        }
                    }
                    catch { }

                    Profile Profile = new Profile(Name == m_Active, Name, Type, Qualifier);

                    try
                    {
                        foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS3/Button"))
                        {
                            foreach (XmlNode Item in Mapping.ChildNodes)
                            {
                                Ds3Button Target = (Ds3Button) Enum.Parse(typeof(Ds3Button), Item.ParentNode.Name);
                                Ds3Button Mapped = (Ds3Button) Enum.Parse(typeof(Ds3Button), Item.Value);

                                Profile.Ds3Button[Target] = Mapped;
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS3/Axis"))
                        {
                            foreach (XmlNode Item in Mapping.ChildNodes)
                            {
                                Ds3Axis Target = (Ds3Axis) Enum.Parse(typeof(Ds3Axis), Item.ParentNode.Name);
                                Ds3Axis Mapped = (Ds3Axis) Enum.Parse(typeof(Ds3Axis), Item.Value);

                                Profile.Ds3Axis[Target] = Mapped;
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS4/Button"))
                        {
                            foreach (XmlNode Item in Mapping.ChildNodes)
                            {
                                Ds4Button Target = (Ds4Button) Enum.Parse(typeof(Ds4Button), Item.ParentNode.Name);
                                Ds4Button Mapped = (Ds4Button) Enum.Parse(typeof(Ds4Button), Item.Value);

                                Profile.Ds4Button[Target] = Mapped;
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        foreach (XmlNode Mapping in ProfileNode.SelectSingleNode("DS4/Axis"))
                        {
                            foreach (XmlNode Item in Mapping.ChildNodes)
                            {
                                Ds4Axis Target = (Ds4Axis) Enum.Parse(typeof(Ds4Axis), Item.ParentNode.Name);
                                Ds4Axis Mapped = (Ds4Axis) Enum.Parse(typeof(Ds4Axis), Item.Value);

                                Profile.Ds4Axis[Target] = Mapped;
                            }
                        }
                    }
                    catch { }

                    m_Mapper[Profile.Name] = Profile;
                }

                Int32 Mappings = m_Mapper[m_Active].Ds3Button.Count + m_Mapper[m_Active].Ds3Axis.Count + m_Mapper[m_Active].Ds4Button.Count + m_Mapper[m_Active].Ds4Axis.Count;
                LogDebug(String.Format("## Mapper.Initialize() - Profiles [{0}] Active [{1}] Mappings [{2}]", m_Mapper.Count, m_Active, Mappings));

                m_Remapping = true;
            }
            catch { }

            return m_Remapping;
        }

        public virtual Boolean Shutdown() 
        {
            m_Remapping = false;

            LogDebug("## Mapper.Shutdown()");
            return true;
        }

        public virtual Boolean Construct(ref XmlDocument Map) 
        {
            Boolean Constructed = true;

            try
            {
                XmlNode     Node;
                XmlDocument Doc = new XmlDocument();

                Node = Doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
                Doc.AppendChild(Node);

                Node = Doc.CreateComment(String.Format(" ScpMapper Configuration Data. {0} ", DateTime.Now));
                Doc.AppendChild(Node);

                Node = Doc.CreateNode(XmlNodeType.Element, "ScpMapper", null);
                {
                    CreateTextNode(Doc, Node, "Description", "SCP Mapping File");
                    CreateTextNode(Doc, Node, "Version",     Assembly.GetExecutingAssembly().GetName().Version.ToString());

                    XmlNode Mapping = Doc.CreateNode(XmlNodeType.Element, "Mapping", null);
                    {
                        foreach (Profile Item in m_Mapper.Values)
                        {
                            if (Item.Default) CreateTextNode(Doc, Node, "Active", Item.Name);

                            XmlNode Profile = Doc.CreateNode(XmlNodeType.Element, "Profile", null);
                            {
                                CreateTextNode(Doc, Profile, "Name",  Item.Name);
                                CreateTextNode(Doc, Profile, "Type",  Item.Type);
                                CreateTextNode(Doc, Profile, "Value", Item.Qualifier);

                                XmlNode Ds3 = Doc.CreateNode(XmlNodeType.Element, DsModel.DS3.ToString(), null);
                                {
                                    XmlNode Button = Doc.CreateNode(XmlNodeType.Element, "Button", null);
                                    {
                                        foreach (Ds3Button Ds3Button in Item.Ds3Button.Keys)
                                        {
                                            CreateTextNode(Doc, Button, Ds3Button.ToString(), Item.Ds3Button[Ds3Button].ToString());
                                        }
                                    }
                                    Ds3.AppendChild(Button);

                                    XmlNode Axis = Doc.CreateNode(XmlNodeType.Element, "Axis", null);
                                    {
                                        foreach (Ds3Axis Ds3Axis in Item.Ds3Axis.Keys)
                                        {
                                            CreateTextNode(Doc, Axis, Ds3Axis.ToString(), Item.Ds3Axis[Ds3Axis].ToString());
                                        }
                                    }
                                    Ds3.AppendChild(Axis);
                                }
                                Profile.AppendChild(Ds3);

                                XmlNode Ds4 = Doc.CreateNode(XmlNodeType.Element, DsModel.DS4.ToString(), null);
                                {
                                    XmlNode Button = Doc.CreateNode(XmlNodeType.Element, "Button", null);
                                    {
                                        foreach (Ds4Button Ds4Button in Item.Ds4Button.Keys)
                                        {
                                            CreateTextNode(Doc, Button, Ds4Button.ToString(), Item.Ds4Button[Ds4Button].ToString());
                                        }
                                    }
                                    Ds4.AppendChild(Button);

                                    XmlNode Axis = Doc.CreateNode(XmlNodeType.Element, "Axis", null);
                                    {
                                        foreach (Ds4Axis Ds4Axis in Item.Ds4Axis.Keys)
                                        {
                                            CreateTextNode(Doc, Axis, Ds4Axis.ToString(), Item.Ds4Axis[Ds4Axis].ToString());
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
            catch { Constructed = false; }

            return Constructed;
        }


        public virtual Boolean Remap(DsModel Type, Int32 Pad, String Mac, Byte[] Input, Byte[] Output) 
        {
            Boolean Mapped = false;

            try
            {
                if (m_Remapping)
                {
                    switch (Type)
                    {
                        case DsModel.DS3: Mapped = RemapDs3(Find(Mac, Pad), Input, Output); break;
                        case DsModel.DS4: Mapped = RemapDs4(Find(Mac, Pad), Input, Output); break;
                    }
                }
            }
            catch { }

            return Mapped;
        }


        public virtual Boolean RemapDs3(Profile Map, Byte[] Input, Byte[] Output) 
        {
            Boolean Mapped = false;

            try
            {
                Array.Copy(Input, Output, Input.Length);

                // Map Buttons
                Ds3Button In = (Ds3Button)(UInt32)((Input[10] << 0) | (Input[11] << 8) | (Input[12] << 16) | (Input[13] << 24));
                Ds3Button Out = In;

                foreach (Ds3Button Item in Map.Ds3Button.Keys) if ((Out & Item) != Ds3Button.None) Out ^= Item;
                foreach (Ds3Button Item in Map.Ds3Button.Keys) if ((In  & Item) != Ds3Button.None) Out |= Map.Ds3Button[Item];

                Output[10] = (Byte)((UInt32) Out >>  0 & 0xFF);
                Output[11] = (Byte)((UInt32) Out >>  8 & 0xFF);
                Output[12] = (Byte)((UInt32) Out >> 16 & 0xFF);
                Output[13] = (Byte)((UInt32) Out >> 24 & 0xFF);

                // Map Axis
                foreach (Ds3Axis Item in Map.Ds3Axis.Keys)
                {
                    switch (Item)
                    {
                        case Ds3Axis.LX:
                        case Ds3Axis.LY:
                        case Ds3Axis.RX:
                        case Ds3Axis.RY: 
                            Output[(UInt32) Item] = 127; // Centred
                            break;

                        default:
                            Output[(UInt32) Item] =   0;
                            break;
                    }
                }

                foreach (Ds3Axis Item in Map.Ds3Axis.Keys)
                {
                    if (Map.Ds3Axis[Item] != Ds3Axis.None)
                    {
                        Output[(UInt32) Map.Ds3Axis[Item]] = Input[(UInt32) Item];
                    }
                }

                // Fix up Button-Axis Relations
                foreach (Ds3Button Key in Ds3ButtonAxis.Keys)
                {
                    if ((Out & Key) != Ds3Button.None && Output[(UInt32) Ds3ButtonAxis[Key]] == 0)
                    {
                        Output[(UInt32) Ds3ButtonAxis[Key]] = 0xFF;
                    }
                }

                Mapped = true;
            }
            catch { }

            return Mapped;
        }

        public virtual Boolean RemapDs4(Profile Map, Byte[] Input, Byte[] Output) 
        {
            Boolean Mapped = false;

            try
            {
                Array.Copy(Input, Output, Input.Length);

                // Map Buttons
                Ds4Button In = (Ds4Button)(UInt32)((Input[13] << 0) | (Input[14] << 8) | (Input[15] << 16));
                Ds4Button Out = In;

                foreach (Ds4Button Item in Map.Ds4Button.Keys) if ((Out & Item) != Ds4Button.None) Out ^= Item;
                foreach (Ds4Button Item in Map.Ds4Button.Keys) if ((In  & Item) != Ds4Button.None) Out |= Map.Ds4Button[Item];

                Output[13] = (Byte)((UInt32) Out >>  0 & 0xFF);
                Output[14] = (Byte)((UInt32) Out >>  8 & 0xFF);
                Output[15] = (Byte)((UInt32) Out >> 16 & 0xFF);

                // Map Axis
                foreach (Ds4Axis Item in Map.Ds4Axis.Keys)
                {
                    switch (Item)
                    {
                        case Ds4Axis.LX:
                        case Ds4Axis.LY:
                        case Ds4Axis.RX:
                        case Ds4Axis.RY:
                            Output[(UInt32) Item] = 127; // Centred
                            break;
                        default:
                            Output[(UInt32) Item] = 0;
                            break;
                    }
                }

                foreach (Ds4Axis Item in Map.Ds4Axis.Keys)
                {
                    if (Map.Ds4Axis[Item] != Ds4Axis.None)
                    {
                        Output[(UInt32) Map.Ds4Axis[Item]] = Input[(UInt32) Item];
                    }
                }

                // Fix up Button-Axis Relations
                foreach (Ds4Button Key in Ds4ButtonAxis.Keys)
                {
                    if ((Out & Key) != Ds4Button.None && Output[(UInt32) Ds4ButtonAxis[Key]] == 0)
                    {
                        Output[(UInt32) Ds4ButtonAxis[Key]] = 0xFF;
                    }
                }


                Mapped = true;
            }
            catch { }

            return Mapped;
        }


        public virtual String[] Profiles 
        {
            get 
            {
                Int32 Index = 0;
                String[] List = new String[m_Mapper.Count];

                foreach (String Item in m_Mapper.Keys)
                {
                    List[Index++] = Item;
                }

                return List;
            }
        }

        public virtual String   Active   
        {
            get { return m_Active; }
        }

        public virtual ProfileMap Map    
        {
            get { return m_Mapper; }
        }
    }
}
