using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ScpControl;

namespace ScpMonitor 
{
    public partial class ProfileProperties : Form 
    {
        protected Profile         m_Profile;
        protected Boolean         m_Saved = false;
        protected PropEditProfile m_Editor;

        public Profile Profile 
        {
            get { return m_Profile; }
        }

        public Boolean Saved   
        {
            get { return m_Saved; }
        }

        public ProfileProperties(Profile Profile, DsPadId Pad, String Mac, Boolean Edit) 
        {
            InitializeComponent();

            m_Profile = Profile;
            btnSave.Enabled = Edit;

            propGrid.SelectedObject = m_Editor = new PropEditProfile(Pad, Mac);
        }

        protected void Form_Load(object sender, EventArgs e) 
        {
            Location = new Point(Owner.Location.X + Owner.Width, Owner.Location.Y);
            Height   = Owner.Height;

            m_Editor.Name   = m_Profile.Name;
            m_Editor.Type   = m_Profile.Match;
            m_Editor.Target = m_Profile.Qualifier;

            foreach (Ds3Axis Key in m_Profile.Ds3Axis.Keys)
            {
                m_Editor.Ds3Axis.Add(new Ds3AxisMap(Key, m_Profile.Ds3Axis[Key]));
            }

            foreach (Ds4Axis Key in m_Profile.Ds4Axis.Keys)
            {
                m_Editor.Ds4Axis.Add(new Ds4AxisMap(Key, m_Profile.Ds4Axis[Key]));
            }

            foreach (Ds3Button Key in m_Profile.Ds3Button.Keys)
            {
                m_Editor.Ds3Button.Add(new Ds3ButtonMap(Key, m_Profile.Ds3Button[Key]));
            }

            foreach (Ds4Button Key in m_Profile.Ds4Button.Keys)
            {
                m_Editor.Ds4Button.Add(new Ds4ButtonMap(Key, m_Profile.Ds4Button[Key]));
            }
        }

        protected void btnSave_Click(object sender, EventArgs e) 
        {
            m_Profile = new Profile(m_Profile.Default, m_Editor.Name, m_Editor.Type.ToString(), m_Editor.Target);

            foreach (Ds3AxisMap Map in m_Editor.Ds3Axis)
            {
                m_Profile.Ds3Axis[Map.Name] = Map.Value;
            }

            foreach (Ds4AxisMap Map in m_Editor.Ds4Axis)
            {
                m_Profile.Ds4Axis[Map.Name] = Map.Value;
            }

            foreach (Ds3ButtonMap Map in m_Editor.Ds3Button)
            {
                m_Profile.Ds3Button[Map.Name] = Map.Value;
            }

            foreach (Ds4ButtonMap Map in m_Editor.Ds4Button)
            {
                m_Profile.Ds4Button[Map.Name] = Map.Value;
            }

            m_Saved = true;
            Close();
        }

        protected void btnClose_Click(object sender, EventArgs e) 
        {
            Close();
        }
    }

    public class PropEditProfile 
    {
        protected String  m_Name;
        protected DsMatch m_Type;
        protected DsPadId m_Pad;
        protected String  m_Mac;
        protected String  m_Qualifier;

        protected Ds3AxisCollection m_Ds3Axis = new Ds3AxisCollection();
        protected Ds4AxisCollection m_Ds4Axis = new Ds4AxisCollection();

        protected Ds3ButtonCollection m_Ds3Button = new Ds3ButtonCollection();
        protected Ds4ButtonCollection m_Ds4Button = new Ds4ButtonCollection();

        public PropEditProfile(DsPadId Pad, String Mac) 
        {
            m_Pad = Pad;
            m_Mac = Mac;
        }

        [Category("Details")]
        public virtual String Name 
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [Category("Details")]
        public virtual DsMatch Type 
        {
            get { return m_Type; }
            set 
            {
                m_Type = value;

                switch (m_Type)
                {
                    case DsMatch.Pad: m_Qualifier = m_Pad.ToString(); break;
                    case DsMatch.Mac: m_Qualifier = m_Mac; break;
                    default:
                        m_Qualifier = String.Empty;
                        break;
                }
            }
        }

        [Category("Details")]
        public virtual String Target 
        {
            get { return m_Qualifier; }
            internal set { m_Qualifier = value; }
        }

        [Category("Mapping")]
        [DisplayName("DS3 Button")]
        [Description("DS3 Button used in the Mapping")]
        public Ds3ButtonCollection Ds3Button 
        {
            get { return m_Ds3Button; }
        }

        [Category("Mapping")]
        [DisplayName("DS3 Axis")]
        [Description("DS3 Axis used in the Mapping")]
        public Ds3AxisCollection Ds3Axis 
        {
            get { return m_Ds3Axis; }
        }

        [Category("Mapping")]
        [DisplayName("DS4 Button")]
        [Description("DS4 Button used in the Mapping")]
        public Ds4ButtonCollection Ds4Button 
        {
            get { return m_Ds4Button; }
        }

        [Category("Mapping")]
        [DisplayName("DS4 Axis")]
        [Description("DS4 Axis used in the Mapping")]
        public Ds4AxisCollection Ds4Axis 
        {
            get { return m_Ds4Axis; }
        }
    }

    public class Ds3AxisMap   
    {
        protected Ds3Axis m_Name = Ds3Axis.None, m_Value = Ds3Axis.None;

        public Ds3AxisMap() { }

        public Ds3AxisMap(Ds3Axis Name, Ds3Axis Value) 
        {
            m_Name = Name;
            m_Value = Value;
        }

        public Ds3Axis Name  
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Ds3Axis Value 
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }
    public class Ds4AxisMap   
    {
        protected Ds4Axis m_Name = Ds4Axis.None, m_Value = Ds4Axis.None;

        public Ds4AxisMap() { }

        public Ds4AxisMap(Ds4Axis Name, Ds4Axis Value) 
        {
            m_Name  = Name;
            m_Value = Value;
        }

        public Ds4Axis Name  
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Ds4Axis Value 
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }
    public class Ds3ButtonMap 
    {
        protected Ds3Button m_Name = Ds3Button.None, m_Value = Ds3Button.None;

        public Ds3ButtonMap() { }

        public Ds3ButtonMap(Ds3Button Name, Ds3Button Value) 
        {
            m_Name  = Name;
            m_Value = Value;
        }

        public Ds3Button Name  
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Ds3Button Value 
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }
    public class Ds4ButtonMap 
    {
        protected Ds4Button m_Name = Ds4Button.None, m_Value = Ds4Button.None;

        public Ds4ButtonMap() { }

        public Ds4ButtonMap(Ds4Button Name, Ds4Button Value) 
        {
            m_Name  = Name;
            m_Value = Value;
        }

        public Ds4Button Name  
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Ds4Button Value 
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
    }

    [DisplayName("DS3 Axis Mapper")]
    [Editor(typeof(Ds3AxisCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class Ds3AxisCollection : CollectionBase 
    {
        public Ds3AxisMap this[Int32 Index] 
        {
            get { return (Ds3AxisMap)List[Index]; }
        }

        public void Add(Ds3AxisMap Prop)    
        {
            List.Add(Prop);
        }

        public void Remove(Ds3AxisMap Prop) 
        {
            List.Remove(Prop);
        }
    }

    [DisplayName("DS4 Axis Mapper")]
    [Editor(typeof(Ds4AxisCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class Ds4AxisCollection : CollectionBase 
    {
        public Ds4AxisMap this[Int32 Index] 
        {
            get { return (Ds4AxisMap)List[Index]; }
        }

        public void Add(Ds4AxisMap Prop)    
        {
            List.Add(Prop);
        }

        public void Remove(Ds4AxisMap Prop) 
        {
            List.Remove(Prop);
        }
    }

    [DisplayName("DS3 Button Mapper")]
    [Editor(typeof(Ds3ButtonCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class Ds3ButtonCollection : CollectionBase 
    {
        public Ds3ButtonMap this[Int32 Index] 
        {
            get { return (Ds3ButtonMap) List[Index]; }
        }

        public void Add(Ds3ButtonMap Prop)    
        {
            List.Add(Prop);
        }

        public void Remove(Ds3ButtonMap Prop) 
        {
            List.Remove(Prop);
        }
    }

    [DisplayName("DS4 Button Mapper")]
    [Editor(typeof(Ds4ButtonCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class Ds4ButtonCollection : CollectionBase 
    {
        public Ds4ButtonMap this[Int32 Index] 
        {
            get { return (Ds4ButtonMap)List[Index]; }
        }

        public void Add(Ds4ButtonMap Prop)    
        {
            List.Add(Prop);
        }

        public void Remove(Ds4ButtonMap Prop) 
        {
            List.Remove(Prop);
        }
    }

    public class Ds3AxisCollectionEditor   : CollectionEditor 
    {
        public Ds3AxisCollectionEditor(Type Type) : base(Type) 
        {
        }

        protected override CollectionForm CreateCollectionForm() 
        {
            CollectionForm Form = base.CreateCollectionForm();

            Form.Text = "DS3 Axis Map Editor";
            Form.HelpButton = false;
            Form.StartPosition = FormStartPosition.CenterParent;

            return Form;
        }

        protected override string GetDisplayText(object Value) 
        {
            Ds3AxisMap Item = (Ds3AxisMap) Value;

            return base.GetDisplayText(string.Format("{0} -> {1}", Item.Name, Item.Value));
        }
    }
    public class Ds4AxisCollectionEditor   : CollectionEditor 
    {
        public Ds4AxisCollectionEditor(Type Type) : base(Type) 
        {
        }

        protected override CollectionForm CreateCollectionForm() 
        {
            CollectionForm Form = base.CreateCollectionForm();

            Form.Text = "DS4 Axis Map Editor";
            Form.HelpButton = false;
            Form.StartPosition = FormStartPosition.CenterParent;

            return Form;
        }

        protected override string GetDisplayText(object Value) 
        {
            Ds4AxisMap Item = (Ds4AxisMap) Value;

            return base.GetDisplayText(string.Format("{0} -> {1}", Item.Name, Item.Value));
        }
    }
    public class Ds3ButtonCollectionEditor : CollectionEditor 
    {
        public Ds3ButtonCollectionEditor(Type Type) : base(Type) 
        {
        }

        protected override CollectionForm CreateCollectionForm() 
        {
            CollectionForm Form = base.CreateCollectionForm();

            Form.Text = "DS3 Button Map Editor";
            Form.HelpButton = false;
            Form.StartPosition = FormStartPosition.CenterParent;

            return Form;
        }

        protected override string GetDisplayText(object Value) 
        {
            Ds3ButtonMap Item = (Ds3ButtonMap) Value;

            return base.GetDisplayText(string.Format("{0} -> {1}", Item.Name, Item.Value));
        }
    }
    public class Ds4ButtonCollectionEditor : CollectionEditor 
    {
        public Ds4ButtonCollectionEditor(Type Type) : base(Type) 
        {
        }

        protected override CollectionForm CreateCollectionForm() 
        {
            CollectionForm Form = base.CreateCollectionForm();

            Form.Text = "DS4 Button Map Editor";
            Form.HelpButton = false;
            Form.StartPosition = FormStartPosition.CenterParent;

            return Form;
        }

        protected override string GetDisplayText(object Value) 
        {
            Ds4ButtonMap Item = (Ds4ButtonMap) Value;

            return base.GetDisplayText(string.Format("{0} -> {1}", Item.Name, Item.Value));
        }
    }
}
