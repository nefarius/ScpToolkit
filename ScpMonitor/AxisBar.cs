using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ScpMonitor
{
    public partial class AxisBar : UserControl
    {
        public AxisBar() 
        {
            InitializeComponent();
        }

        protected Int32 m_Minimum =   0;
        protected Int32 m_Maximum = 100;
        protected Int32 m_Value   =   0;
        protected Color m_Colour  = Color.Green;

        protected override void OnResize(EventArgs e) 
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) 
        {
            using (Graphics gr = e.Graphics)
            {
                using (SolidBrush br = new SolidBrush(m_Colour))
                {
                    float     Fill = (float)(m_Value - m_Minimum) / (float)(m_Maximum - m_Minimum);
                    Rectangle Rect = ClientRectangle;

                    Rect.Width = (Int32)((float) Rect.Width * Fill);
                    gr.FillRectangle(br, Rect);
                }
            }
        }

        public Int32 Minimum 
        {
            get { return m_Minimum; }
            set 
            {
                if (value < 0 || value > m_Maximum)
                {
                    throw new ArgumentOutOfRangeException("Minimum");
                }

                if (m_Value < value)
                {
                    m_Value = value;
                }

                if (m_Minimum != value)
                {
                    m_Minimum = value;
                    Invalidate();
                }
            }
        }

        public Int32 Maximum 
        {
            get { return m_Maximum; }
            set 
            {
                if (value < 0 || value < m_Minimum)
                {
                    throw new ArgumentOutOfRangeException("Maximum");
                }

                if (m_Value > value)
                {
                    m_Value = value;
                }

                if (m_Maximum != value)
                {
                    m_Maximum = value;
                    Invalidate();
                }
            }
        }

        public Int32 Value   
        {
            get { return m_Value; }
            set 
            {
                if (value < m_Minimum || value > m_Maximum)
                {
                    throw new ArgumentOutOfRangeException("Maximum");
                }

                if (m_Value != value)
                {
                    Rectangle newRect = ClientRectangle;
                    Rectangle oldRect = ClientRectangle;
                    Int32 oldValue    = m_Value;
                    float Fill;

                    m_Value = value;

                    Fill = (float)(m_Value - m_Minimum) / (float)(m_Maximum - m_Minimum);
                    newRect.Width = (int)((float)newRect.Width * Fill);

                    Fill = (float)(oldValue - m_Minimum) / (float)(m_Maximum - m_Minimum);
                    oldRect.Width = (int)((float)oldRect.Width * Fill);

                    Rectangle Rect = new Rectangle(); Rect.Height = Height;

                    if (newRect.Width > oldRect.Width)
                    {
                        Rect.X = oldRect.Size.Width;
                        Rect.Width = newRect.Width - oldRect.Width;
                    }
                    else
                    {
                        Rect.X = newRect.Size.Width;
                        Rect.Width = oldRect.Width - newRect.Width;
                    }

                    Invalidate(Rect);
                }
            }
        }

        public Color Color   
        {
            get { return m_Colour; }
            set 
            {
                if (m_Colour != value)
                {
                    m_Colour = value;
                    Invalidate();
                }
            }
        }
    }
}
