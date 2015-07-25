using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace ScpControl 
{
    public partial class ScpPadState : Component 
    {
        protected ScpProxy m_Proxy = null;

        protected const Int32 Centre       = 127;
        protected const Int32 Accelerate   =  75;
        protected const Int32 Repeat_Delay =  40;
        protected const Int32 Repeat_Rate  =   5;

        protected DsPadId m_Pad = DsPadId.One;

        // Mouse
        protected Int32 m_Threshold = 0;
        protected Int32 m_vx = 0, m_vy = 0;
        protected Int32 m_dx = 0, m_dy = 0;

        public ScpProxy Proxy 
        {
            get { return m_Proxy; }
            set { m_Proxy = value; Proxy.Packet += Sample; }
        }

        public DsPadId Pad 
        {
            get { return m_Pad; }
            set { lock (this) { m_Pad = value; } }
        }

        public Boolean Enabled 
        {
            get { return tmUpdate.Enabled; }
            set 
            {
                if (tmUpdate.Enabled != value)
                {
                    lock (this)
                    {
                        tmUpdate.Enabled = value;

                        if (!value)
                        {
                            try { Reset(); }
                            catch { }
                        }
                    }
                }
            }
        }


        public Int32 Threshold 
        {
            get { return m_Threshold; }
            set { lock (this) { m_Threshold = value; } }
        }

        public Int32 ScaleX 
        {
            get { return m_vx; }
            set { lock (this) { m_vx = value; } }
        }

        public Int32 ScaleY 
        {
            get { return m_vy; }
            set { lock (this) { m_vy = value; } }
        }


        public ScpPadState() 
        {
            InitializeComponent();
        }

        public ScpPadState(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public virtual void Sample(object sender, DsPacket Packet) 
        {
            lock (this)
            {
                if (Packet.Detail.Pad == Pad)
                {
                    if (Packet.Detail.State == DsState.Connected)
                    {
                        switch (Packet.Detail.Model)
                        {
                            case DsModel.DS3:

                                try { SampleDs3(Packet); }
                                catch { };
                                break;

                            case DsModel.DS4:

                                try { SampleDs4(Packet); }
                                catch { }
                                break;
                        }
                    }
                    else
                    {
                        try { Reset(); }
                        catch { }
                    }
                }
            }
        }


        protected virtual void SampleDs3(DsPacket Packet) 
        {
            m_dx = Mouse(Packet.Axis(Ds3Axis.RX), m_vx);
            m_dy = Mouse(Packet.Axis(Ds3Axis.RY), m_vy);
        }

        protected virtual void SampleDs4(DsPacket Packet) 
        {
            m_dx = Mouse(Packet.Axis(Ds4Axis.RX), m_vx);
            m_dy = Mouse(Packet.Axis(Ds4Axis.RY), m_vy);
        }


        protected virtual void Rumble(Byte Large, Byte Small) 
        {
            if (Proxy != null)
            {
                Proxy.Rumble(Pad, Large, Small);
            }
        }

        protected virtual void Reset() 
        {
            m_dx = m_dy = 0;
        }

        protected virtual void Timer() 
        {
            if (m_dx != 0 || m_dy != 0) KbmPost.Move(m_dx, m_dy);
        }


        protected virtual Int32   Mouse (Int32   Old, Int32   Scale) 
        {
            Int32 New = 0;

            if (Old < (Centre - m_Threshold)) { New = -Scale; if (Old < (Centre - Accelerate)) New <<= 1; }
            if (Old > (Centre + m_Threshold)) { New = +Scale; if (Old > (Centre + Accelerate)) New <<= 1; }

            return New;
        }

        protected virtual Boolean Mouse (Boolean Old, Boolean New,   KbmPost.MouseButtons Button) 
        {
            if (Old != New) KbmPost.Button(Button, New);

            return New;
        }

        protected virtual Boolean Button(Boolean Old, Boolean New,   Keys Key, Boolean Extended) 
        {
            if (Old != New) KbmPost.Key(Key, Extended, New);

            return New;
        }

        protected virtual Int32   Repeat(Boolean Old, Int32   Count, Keys Key, Boolean Extended) 
        {
            if (Old)
            {
                if ((++Count >= Repeat_Delay) && ((Count % Repeat_Rate) == 0))
                {
                    KbmPost.Key(Key, Extended, false);
                    KbmPost.Key(Key, Extended, true);
                }
            }
            else
            {
                Count = 0;
            }

            return Count;
        }

        protected virtual Boolean Macro (Boolean Old, Boolean New,   Keys[] Keys) 
        {
            if (!Old && New)
            {
                foreach(Keys Key in Keys)
                {
                    KbmPost.Key(Key, false, true );
                    KbmPost.Key(Key, false, false);
                }
            }

            return New;
        }

        protected virtual Boolean Wheel (Boolean Old, Boolean New,   Boolean Vertical, Boolean Direction) 
        {
            if (!Old && New) KbmPost.Wheel(Vertical, Direction ? 1 : -1);

            return New;
        }

        protected virtual Boolean Toggle(Boolean Old, Boolean New,   ref Boolean Target) 
        {
            if (!Old && New) Target = !Target;

            return New;
        }


        internal virtual void tmUpdate_Tick(object sender, EventArgs e) 
        {
            lock (this)
            {
                try { Timer(); }
                catch { }
            }
        }
    }
}
