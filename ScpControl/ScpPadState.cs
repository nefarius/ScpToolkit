using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using ScpControl.Utilities;

namespace ScpControl
{
    public partial class ScpPadState : Component
    {
        protected const int Centre = 127;
        private const int Accelerate = 75;
        private const int Repeat_Delay = 40;
        private const int Repeat_Rate = 5;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected int m_dx, m_dy;
        private DsPadId m_Pad = DsPadId.One;
        private ScpProxy m_Proxy;
        // Mouse
        protected int m_Threshold;
        protected int m_vx, m_vy;

        public ScpPadState()
        {
            InitializeComponent();
        }

        public ScpPadState(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public ScpProxy Proxy
        {
            get { return m_Proxy; }
            set
            {
                m_Proxy = value;
                Proxy.Packet += Sample;
            }
        }

        public DsPadId Pad
        {
            get { return m_Pad; }
            set
            {
                lock (this)
                {
                    m_Pad = value;
                }
            }
        }

        public bool Enabled
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
                            try
                            {
                                Reset();
                            }
                            catch (Exception ex)
                            {
                                Log.ErrorFormat("Unexpected error: {0}", ex);
                            }
                        }
                    }
                }
            }
        }

        public int Threshold
        {
            get { return m_Threshold; }
            set
            {
                lock (this)
                {
                    m_Threshold = value;
                }
            }
        }

        public int ScaleX
        {
            get { return m_vx; }
            set
            {
                lock (this)
                {
                    m_vx = value;
                }
            }
        }

        public int ScaleY
        {
            get { return m_vy; }
            set
            {
                lock (this)
                {
                    m_vy = value;
                }
            }
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

                                try
                                {
                                    SampleDs3(Packet);
                                }
                                catch (Exception ex)
                                {
                                    Log.ErrorFormat("Unexpected error: {0}", ex);
                                }
                                break;

                            case DsModel.DS4:

                                try
                                {
                                    SampleDs4(Packet);
                                }
                                catch (Exception ex)
                                {
                                    Log.ErrorFormat("Unexpected error: {0}", ex);
                                }
                                break;
                        }
                    }
                    else
                    {
                        try
                        {
                            Reset();
                        }
                        catch (Exception ex)
                        {
                            Log.ErrorFormat("Unexpected error: {0}", ex);
                        }
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

        protected virtual void Rumble(byte Large, byte Small)
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

        protected virtual int Mouse(int Old, int Scale)
        {
            var New = 0;

            if (Old < (Centre - m_Threshold))
            {
                New = -Scale;
                if (Old < (Centre - Accelerate)) New <<= 1;
            }
            if (Old > (Centre + m_Threshold))
            {
                New = +Scale;
                if (Old > (Centre + Accelerate)) New <<= 1;
            }

            return New;
        }

        protected virtual bool Mouse(bool Old, bool New, KbmPost.MouseButtons Button)
        {
            if (Old != New) KbmPost.Button(Button, New);

            return New;
        }

        protected virtual bool Button(bool Old, bool New, Keys Key, bool Extended)
        {
            if (Old != New) KbmPost.Key(Key, Extended, New);

            return New;
        }

        protected virtual int Repeat(bool Old, int Count, Keys Key, bool Extended)
        {
            if (Old)
            {
                if ((++Count >= Repeat_Delay) && ((Count%Repeat_Rate) == 0))
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

        protected virtual bool Macro(bool Old, bool New, Keys[] Keys)
        {
            if (!Old && New)
            {
                foreach (var Key in Keys)
                {
                    KbmPost.Key(Key, false, true);
                    KbmPost.Key(Key, false, false);
                }
            }

            return New;
        }

        protected virtual bool Wheel(bool Old, bool New, bool Vertical, bool Direction)
        {
            if (!Old && New) KbmPost.Wheel(Vertical, Direction ? 1 : -1);

            return New;
        }

        protected virtual bool Toggle(bool Old, bool New, ref bool Target)
        {
            if (!Old && New) Target = !Target;

            return New;
        }

        internal virtual void tmUpdate_Tick(object sender, EventArgs e)
        {
            lock (this)
            {
                try
                {
                    Timer();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }
            }
        }
    }
}