using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ScpMonitor 
{
    public enum Orientation { Left, Right, Top, Bottom }

    public partial class AxisControl : UserControl 
    {
        protected Orientation m_Orientation = Orientation.Left;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override String Text 
        {
            get { return axButton.Text; }
            set { axButton.Text = value; }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Byte Value 
        {
            get { return (Byte) axBar.Value; }
            set { axBar.Value = value; }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color Color 
        {
            get { return axBar.Color; }
            set { axBar.Color = value; }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Orientation Orientation 
        {
            get { return m_Orientation; }
            set 
            {
                if (m_Orientation != value)
                {
                    m_Orientation = value;

                    switch(m_Orientation)
                    {
                        case ScpMonitor.Orientation.Left:
                            {
                                Size = new Size(115, 15);

                                axButton.Location = new Point(0, 0);
                                axButton.Anchor   = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                                axButton.Size     = new Size(25, Height);

                                axBar.Location = new Point(25, 0);
                                axBar.Anchor   = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                                axBar.Size     = new Size(Width - 25, Height);
                            }
                            break;

                        case ScpMonitor.Orientation.Right:
                            {
                                Size = new Size(115, 15);

                                axButton.Location = new Point(Width - 25, 0);
                                axButton.Anchor   = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                                axButton.Size     = new Size(25, Height);

                                axBar.Location = new Point(0, 0);
                                axBar.Anchor   = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                                axBar.Size     = new Size(Width - 25, Height);
                            }
                            break;

                        case ScpMonitor.Orientation.Top:
                            {
                                Size = new Size(25, 30);

                                axButton.Location = new Point(0, Height - 15);
                                axButton.Anchor   = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                                axButton.Size     = new Size(Width, 15);

                                axBar.Location = new Point(0, 0);
                                axBar.Anchor   = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                                axBar.Size     = new Size(Width, Height - 15);
                            }
                            break;

                        case ScpMonitor.Orientation.Bottom:
                            {
                                Size = new Size(25, 30);

                                axButton.Location = new Point(0, 0);
                                axButton.Anchor   = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                                axButton.Size     = new Size(Width, 15);

                                axBar.Location = new Point(0, 15);
                                axBar.Anchor   = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                                axBar.Size     = new Size(Width, Height - 15);
                            }
                            break;
                    }
                }
            }
        }

        public AxisControl() 
        {
            InitializeComponent();
        }

        private void axButton_Click(object sender, EventArgs e) 
        {
            InvokeOnClick(this, e);
        }
    }
}
