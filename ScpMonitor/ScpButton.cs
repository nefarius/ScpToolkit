using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ScpMonitor
{
    [DesignTimeVisible(true), ToolboxItem(true)]
    public partial class ScpButton : Button
    {
        protected enum State { Normal, Hover, Clicked }

        protected State m_State = State.Normal;

        protected Color m_NormalColor = Color.Silver;
        protected Color m_HoverColor  = Color.DodgerBlue;

        public Boolean Glassy 
        {
            get { return m_NormalColor == Color.Black; }
            set
            {
                if (value ^ Glassy)
                {
                    m_NormalColor = value ? Color.Black : Color.Silver;
                    Invalidate();
                }
            }
        }

        public ScpButton() 
        {
            InitializeComponent();
        }

        protected static GraphicsPath CreateRoundRectangle(Rectangle Rectangle, int Radius) 
        {
            GraphicsPath gp = new GraphicsPath();

            int l = Rectangle.Left;
            int t = Rectangle.Top;
            int w = Rectangle.Width;
            int h = Rectangle.Height;
            int d = Radius << 1;

            gp.AddArc(l, t, d, d, 180, 90);                       // top left
            gp.AddLine(l + Radius, t, l + w - Radius, t);         // top
            gp.AddArc(l + w - d, t, d, d, 270, 90);               // top right
            gp.AddLine(l + w, t + Radius, l + w, t + h - Radius); // right
            gp.AddArc(l + w - d, t + h - d, d, d, 0, 90);         // bottom right
            gp.AddLine(l + w - Radius, t + h, l + Radius, t + h); // bottom
            gp.AddArc(l, t + h - d, d, d, 90, 90);                // bottom left
            gp.AddLine(l, t + h - Radius, l, t + Radius);         // left

            gp.CloseFigure();
            return gp;
        }

        protected static GraphicsPath CreateTopRoundRectangle(Rectangle Rectangle, int Radius) 
        {
            GraphicsPath gp = new GraphicsPath();

            int l = Rectangle.Left;
            int t = Rectangle.Top;
            int w = Rectangle.Width;
            int h = Rectangle.Height;
            int d = Radius << 1;

            gp.AddArc(l, t, d, d, 180, 90);               // topleft
            gp.AddLine(l + Radius, t, l + w - Radius, t); // top
            gp.AddArc(l + w - d, t, d, d, 270, 90);       // topright
            gp.AddLine(l + w, t + Radius, l + w, t + h);  // right
            gp.AddLine(l + w, t + h, l, t + h);           // bottom
            gp.AddLine(l, t + h, l, t + Radius);          // left

            gp.CloseFigure();
            return gp;
        }

        protected static GraphicsPath CreateBottomRadialPath(Rectangle Rectangle) 
        {
            GraphicsPath gp = new GraphicsPath();
            RectangleF rf = Rectangle;

            rf.X -= rf.Width * .35f;
            rf.Y -= rf.Height * .15f;

            rf.Width *= 1.7f;
            rf.Height *= 2.3f;

            gp.AddEllipse(rf);

            gp.CloseFigure();
            return gp;
        }

        protected void Glassify(Rectangle Rectangle, PaintEventArgs e, Color Color, bool Pressed) 
        {
            using (GraphicsPath gp = CreateRoundRectangle(Rectangle, 2))
            {
                int opacity = 0x7f;

                using (Brush brush = new SolidBrush(Color.FromArgb(opacity, Color)))
                {
                    e.Graphics.FillPath(brush, gp);
                }
            }

            using (GraphicsPath clip = CreateRoundRectangle(Rectangle, 2))
            {
                e.Graphics.SetClip(clip, CombineMode.Intersect);

                using (GraphicsPath gp = CreateBottomRadialPath(Rectangle))
                {
                    using (PathGradientBrush brush = new PathGradientBrush(gp))
                    {
                        int opacity = (int)(0xB2 * .99f + .5f);

                        RectangleF bounds = gp.GetBounds();

                        brush.CenterPoint = new PointF((bounds.Left + bounds.Right) / 2f, (bounds.Top + bounds.Bottom) / 2f);
                        brush.CenterColor = Color.FromArgb(opacity, Color.White);
                        brush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };

                        e.Graphics.FillPath(brush, gp);
                    }
                }

                e.Graphics.ResetClip();
            }

            Rectangle newRect = Rectangle; newRect.Height >>= 1;

            if (newRect.Width > 0 && newRect.Height > 0)
            {
                newRect.Height++;

                using (GraphicsPath gp = CreateTopRoundRectangle(newRect, 2))
                {
                    int opacity = Pressed ? (int)(.4f * 0x9 + .5f) : 0x99;

                    newRect.Height++;

                    using (LinearGradientBrush brush = new LinearGradientBrush(newRect, Color.FromArgb(opacity, Color.White), Color.FromArgb(opacity / 3, Color.White), LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, gp);
                    }
                }
            }

            int Y = Rectangle.Y + Rectangle.Height - 1;

            // e.Graphics.DrawLine(new Pen(Color.Black), Rectangle.Left, Y, Rectangle.Right, Y);
        }

        protected override void OnPaint(PaintEventArgs e) 
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.Clear(Color.White);

            SizeF textSize = e.Graphics.MeasureString(this.Text, base.Font);

            int textX = (int)(base.Size.Width  / 2) - (int)(textSize.Width  / 2);
            int textY = (int)(base.Size.Height / 2) - (int)(textSize.Height / 2);

            Rectangle newRect = new Rectangle(ClientRectangle.X - 1, ClientRectangle.Y - 1, ClientRectangle.Width + 1, ClientRectangle.Height + 1);

            if (Enabled)
            {
                switch (m_State)
                {
                    case State.Normal:
                        {
                            Glassify(newRect, e, m_NormalColor, false);

                            e.Graphics.DrawString(this.Text, base.Font, new SolidBrush(base.ForeColor), textX, textY);
                        }
                        break;

                    case State.Hover:
                        {
                            Glassify(newRect, e, m_HoverColor, false);

                            e.Graphics.DrawString(this.Text, base.Font, new SolidBrush(base.ForeColor), textX, textY);
                        }
                        break;

                    case State.Clicked:
                        {
                            Glassify(newRect, e, m_HoverColor, true);

                            e.Graphics.DrawRectangle(new Pen(m_HoverColor, 2), newRect);
                            e.Graphics.DrawString(this.Text, base.Font, new SolidBrush(base.ForeColor), textX + 1, textY + 1);
                        }
                        break;
                }
            }
            else
            {
                Glassify(newRect, e, m_NormalColor, false);

                e.Graphics.DrawString(this.Text, base.Font, new SolidBrush(base.ForeColor), textX, textY);
            }
        }

        protected override void OnMouseLeave(EventArgs e) 
        {
            m_State = State.Normal;

            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e) 
        {
            if (Enabled) m_State = State.Hover;

            this.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) 
        {
            if (Enabled) m_State = State.Hover;

            this.Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) 
        {
            if (Enabled) m_State = State.Clicked;

            this.Invalidate();
            base.OnMouseDown(e);
        }
    }
}
