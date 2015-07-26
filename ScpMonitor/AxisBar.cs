using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScpMonitor
{
    public partial class AxisBar : UserControl
    {
        private Color m_Colour = Color.Green;
        private int m_Maximum = 100;
        private int m_Minimum;
        private int m_Value;

        public AxisBar()
        {
            InitializeComponent();
        }

        public int Minimum
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

        public int Maximum
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

        public int Value
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
                    var newRect = ClientRectangle;
                    var oldRect = ClientRectangle;
                    var oldValue = m_Value;
                    float Fill;

                    m_Value = value;

                    Fill = (m_Value - m_Minimum)/(float) (m_Maximum - m_Minimum);
                    newRect.Width = (int) (newRect.Width*Fill);

                    Fill = (oldValue - m_Minimum)/(float) (m_Maximum - m_Minimum);
                    oldRect.Width = (int) (oldRect.Width*Fill);

                    var Rect = new Rectangle();
                    Rect.Height = Height;

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

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var gr = e.Graphics)
            {
                using (var br = new SolidBrush(m_Colour))
                {
                    var Fill = (m_Value - m_Minimum)/(float) (m_Maximum - m_Minimum);
                    var Rect = ClientRectangle;

                    Rect.Width = (int) (Rect.Width*Fill);
                    gr.FillRectangle(br, Rect);
                }
            }
        }
    }
}