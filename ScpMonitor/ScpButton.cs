using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScpMonitor
{
    [DesignTimeVisible(true), ToolboxItem(true)]
    public partial class ScpButton : Button
    {
        private readonly Color m_HoverColor = Color.DodgerBlue;
        private Color m_NormalColor = Color.Silver;
        protected State m_State = State.Normal;

        public ScpButton()
        {
            InitializeComponent();
        }

        public bool Glassy
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

        private static GraphicsPath CreateRoundRectangle(Rectangle Rectangle, int Radius)
        {
            var gp = new GraphicsPath();

            var l = Rectangle.Left;
            var t = Rectangle.Top;
            var w = Rectangle.Width;
            var h = Rectangle.Height;
            var d = Radius << 1;

            gp.AddArc(l, t, d, d, 180, 90); // top left
            gp.AddLine(l + Radius, t, l + w - Radius, t); // top
            gp.AddArc(l + w - d, t, d, d, 270, 90); // top right
            gp.AddLine(l + w, t + Radius, l + w, t + h - Radius); // right
            gp.AddArc(l + w - d, t + h - d, d, d, 0, 90); // bottom right
            gp.AddLine(l + w - Radius, t + h, l + Radius, t + h); // bottom
            gp.AddArc(l, t + h - d, d, d, 90, 90); // bottom left
            gp.AddLine(l, t + h - Radius, l, t + Radius); // left

            gp.CloseFigure();
            return gp;
        }

        private static GraphicsPath CreateTopRoundRectangle(Rectangle Rectangle, int Radius)
        {
            var gp = new GraphicsPath();

            var l = Rectangle.Left;
            var t = Rectangle.Top;
            var w = Rectangle.Width;
            var h = Rectangle.Height;
            var d = Radius << 1;

            gp.AddArc(l, t, d, d, 180, 90); // topleft
            gp.AddLine(l + Radius, t, l + w - Radius, t); // top
            gp.AddArc(l + w - d, t, d, d, 270, 90); // topright
            gp.AddLine(l + w, t + Radius, l + w, t + h); // right
            gp.AddLine(l + w, t + h, l, t + h); // bottom
            gp.AddLine(l, t + h, l, t + Radius); // left

            gp.CloseFigure();
            return gp;
        }

        private static GraphicsPath CreateBottomRadialPath(Rectangle Rectangle)
        {
            var gp = new GraphicsPath();
            RectangleF rf = Rectangle;

            rf.X -= rf.Width*.35f;
            rf.Y -= rf.Height*.15f;

            rf.Width *= 1.7f;
            rf.Height *= 2.3f;

            gp.AddEllipse(rf);

            gp.CloseFigure();
            return gp;
        }

        private void Glassify(Rectangle Rectangle, PaintEventArgs e, Color Color, bool Pressed)
        {
            using (var gp = CreateRoundRectangle(Rectangle, 2))
            {
                var opacity = 0x7f;

                using (Brush brush = new SolidBrush(Color.FromArgb(opacity, Color)))
                {
                    e.Graphics.FillPath(brush, gp);
                }
            }

            using (var clip = CreateRoundRectangle(Rectangle, 2))
            {
                e.Graphics.SetClip(clip, CombineMode.Intersect);

                using (var gp = CreateBottomRadialPath(Rectangle))
                {
                    using (var brush = new PathGradientBrush(gp))
                    {
                        var opacity = (int) (0xB2*.99f + .5f);

                        var bounds = gp.GetBounds();

                        brush.CenterPoint = new PointF((bounds.Left + bounds.Right)/2f, (bounds.Top + bounds.Bottom)/2f);
                        brush.CenterColor = Color.FromArgb(opacity, Color.White);
                        brush.SurroundColors = new[] {Color.FromArgb(0, Color.White)};

                        e.Graphics.FillPath(brush, gp);
                    }
                }

                e.Graphics.ResetClip();
            }

            var newRect = Rectangle;
            newRect.Height >>= 1;

            if (newRect.Width > 0 && newRect.Height > 0)
            {
                newRect.Height++;

                using (var gp = CreateTopRoundRectangle(newRect, 2))
                {
                    var opacity = Pressed ? (int) (.4f*0x9 + .5f) : 0x99;

                    newRect.Height++;

                    using (
                        var brush = new LinearGradientBrush(newRect, Color.FromArgb(opacity, Color.White),
                            Color.FromArgb(opacity/3, Color.White), LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, gp);
                    }
                }
            }

            var Y = Rectangle.Y + Rectangle.Height - 1;

            // e.Graphics.DrawLine(new Pen(Color.Black), Rectangle.Left, Y, Rectangle.Right, Y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.Clear(Color.White);

            var textSize = e.Graphics.MeasureString(Text, Font);

            var textX = Size.Width/2 - (int) (textSize.Width/2);
            var textY = Size.Height/2 - (int) (textSize.Height/2);

            var newRect = new Rectangle(ClientRectangle.X - 1, ClientRectangle.Y - 1, ClientRectangle.Width + 1,
                ClientRectangle.Height + 1);

            if (Enabled)
            {
                switch (m_State)
                {
                    case State.Normal:
                    {
                        Glassify(newRect, e, m_NormalColor, false);

                        e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX, textY);
                    }
                        break;

                    case State.Hover:
                    {
                        Glassify(newRect, e, m_HoverColor, false);

                        e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX, textY);
                    }
                        break;

                    case State.Clicked:
                    {
                        Glassify(newRect, e, m_HoverColor, true);

                        e.Graphics.DrawRectangle(new Pen(m_HoverColor, 2), newRect);
                        e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX + 1, textY + 1);
                    }
                        break;
                }
            }
            else
            {
                Glassify(newRect, e, m_NormalColor, false);

                e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX, textY);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            m_State = State.Normal;

            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (Enabled) m_State = State.Hover;

            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (Enabled) m_State = State.Hover;

            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Enabled) m_State = State.Clicked;

            Invalidate();
            base.OnMouseDown(e);
        }

        protected enum State
        {
            Normal,
            Hover,
            Clicked
        }
    }
}