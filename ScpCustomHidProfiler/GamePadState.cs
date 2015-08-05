using System;
using System.ComponentModel;
using System.Windows.Forms;
using ScpControl;
using ScpControl.ScpCore;
using ScpControl.Utilities;

namespace ScpCustomHidProfiler 
{
    public partial class GamePadState : ScpPadState 
    {
        protected Boolean m_Enabled = true;

        // Mouse
        protected Boolean m_LMB = false;
        protected Boolean m_RMB = false;

        // Keyboard
        protected Boolean m_Ent = false;
        protected Boolean m_Esc = false;
        protected Boolean m_Tab = false;
        protected Boolean m_Spc = false;

        protected Boolean m_CuU = false;
        protected Boolean m_CuD = false;
        protected Boolean m_CuL = false;
        protected Boolean m_CuR = false;

        protected Boolean m_PgU = false;
        protected Boolean m_PgD = false;

        // Control
        protected Boolean m_Enb = false;

        // Repeat Counters
        protected Int32 m_CuU_Repeat = 0;
        protected Int32 m_CuD_Repeat = 0;
        protected Int32 m_CuL_Repeat = 0;
        protected Int32 m_CuR_Repeat = 0;
        
        public GamePadState() 
        {
            InitializeComponent();
        }

        public GamePadState(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }
        
        protected override void SampleDs3(DsPacket Packet) 
        {
            m_Enb = Toggle(m_Enb, Packet.Button(Ds3Button.PS | Ds3Button.Start), ref m_Enabled);

            if (m_Enabled)
            {
                base.SampleDs3(Packet);

                m_LMB = Mouse (m_LMB, Packet.Button(Ds3Button.R1      ), KbmPost.MouseButtons.Left );
                m_RMB = Mouse (m_RMB, Packet.Button(Ds3Button.L1      ), KbmPost.MouseButtons.Right);

                m_Ent = Button(m_Ent, Packet.Button(Ds3Button.Cross   ), Keys.Enter,    false);
                m_Esc = Button(m_Esc, Packet.Button(Ds3Button.Circle  ), Keys.Escape,   false);
                m_Tab = Button(m_Tab, Packet.Button(Ds3Button.Square  ), Keys.Tab,      false);
                m_Spc = Button(m_Spc, Packet.Button(Ds3Button.Triangle), Keys.Space,    false);

                m_PgU = Button(m_PgU, Packet.Button(Ds3Button.R2      ), Keys.PageUp,   true );
                m_PgD = Button(m_PgD, Packet.Button(Ds3Button.L2      ), Keys.PageDown, true );

                m_CuU = Button(m_CuU, Packet.Button(Ds3Button.Up      ), Keys.Up,       true );
                m_CuD = Button(m_CuD, Packet.Button(Ds3Button.Down    ), Keys.Down,     true );
                m_CuL = Button(m_CuL, Packet.Button(Ds3Button.Left    ), Keys.Left,     true );
                m_CuR = Button(m_CuR, Packet.Button(Ds3Button.Right   ), Keys.Right,    true );
            }
        }

        protected override void SampleDs4(DsPacket Packet) 
        {
            m_Enb = Toggle(m_Enb, Packet.Button(Ds4Button.TouchPad), ref m_Enabled);

            if (m_Enabled)
            {
                base.SampleDs4(Packet);

                m_LMB = Mouse (m_LMB, Packet.Button(Ds4Button.R1      ), KbmPost.MouseButtons.Left ); 
                m_RMB = Mouse (m_RMB, Packet.Button(Ds4Button.L1      ), KbmPost.MouseButtons.Right);

                m_Ent = Button(m_Ent, Packet.Button(Ds4Button.Cross   ), Keys.Enter,    false);
                m_Esc = Button(m_Esc, Packet.Button(Ds4Button.Circle  ), Keys.Escape,   false);
                m_Tab = Button(m_Tab, Packet.Button(Ds4Button.Square  ), Keys.Tab,      false);
                m_Spc = Button(m_Spc, Packet.Button(Ds4Button.Triangle), Keys.Space,    false);

                m_PgU = Button(m_PgU, Packet.Button(Ds4Button.R2      ), Keys.PageUp,   true );
                m_PgD = Button(m_PgD, Packet.Button(Ds4Button.L2      ), Keys.PageDown, true );

                m_CuU = Button(m_CuU, Packet.Button(Ds4Button.Up      ), Keys.Up,       true );
                m_CuD = Button(m_CuD, Packet.Button(Ds4Button.Down    ), Keys.Down,     true );
                m_CuL = Button(m_CuL, Packet.Button(Ds4Button.Left    ), Keys.Left,     true );
                m_CuR = Button(m_CuR, Packet.Button(Ds4Button.Right   ), Keys.Right,    true );
            }
        }
        
        protected override void Reset() 
        {
            base.Reset();

            m_Enb = false;

            m_LMB = Mouse (m_LMB, false, KbmPost.MouseButtons.Left );
            m_RMB = Mouse (m_RMB, false, KbmPost.MouseButtons.Right);

            m_Ent = Button(m_Ent, false, Keys.Enter,    false);
            m_Esc = Button(m_Esc, false, Keys.Escape,   false);
            m_Tab = Button(m_Tab, false, Keys.Tab,      false);
            m_Spc = Button(m_Tab, false, Keys.Space,    false);

            m_PgU = Button(m_PgU, false, Keys.PageUp,   true );
            m_PgD = Button(m_PgD, false, Keys.PageDown, true );

            m_CuU = Button(m_CuU, false, Keys.Up,       true );
            m_CuD = Button(m_CuD, false, Keys.Down,     true );
            m_CuL = Button(m_CuL, false, Keys.Left,     true );
            m_CuR = Button(m_CuR, false, Keys.Right,    true );

            m_CuU_Repeat = m_CuD_Repeat = m_CuL_Repeat = m_CuR_Repeat = 0;

            m_Enabled = true;
        }

        protected override void Timer() 
        {
            if (m_Enabled)
            {
                base.Timer();

                m_CuU_Repeat = Repeat(m_CuU, m_CuU_Repeat, Keys.Up,    true);
                m_CuD_Repeat = Repeat(m_CuD, m_CuD_Repeat, Keys.Down,  true);
                m_CuL_Repeat = Repeat(m_CuL, m_CuL_Repeat, Keys.Left,  true);
                m_CuR_Repeat = Repeat(m_CuR, m_CuR_Repeat, Keys.Right, true);
            }
        }
    }
}
