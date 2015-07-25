using System;
using System.ComponentModel;

using System.Runtime.InteropServices;

namespace ScpControl
{
    public partial class ScpTimer : Component 
    {
        [Flags]
        protected enum EventFlags : uint 
        {
            TIME_ONESHOT  = 0,
            TIME_PERIODIC = 1,
        }

        protected delegate void TimerCallback(UInt32 uTimerId, UInt32 uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        [DllImport("Winmm", CharSet = CharSet.Auto)]
        private static extern UInt32 timeSetEvent(UInt32 uDelay, UInt32 uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, EventFlags Flags);

        [DllImport("Winmm", CharSet = CharSet.Auto)]
        private static extern UInt32 timeKillEvent(UInt32 uTimerId);

        protected UInt32        m_Id = 0, m_Interval = 100;
        protected EventArgs     m_Args = new EventArgs();
        protected TimerCallback m_Callback;

        public event EventHandler Tick;

        public object Tag 
        {
            get;
            set;
        }

        public Boolean Enabled   
        {
            get { return m_Id != 0; }
            set 
            {
                lock (this)
                {
                    if (Enabled != value)
                    {
                        if (value)
                        {
                            Start();
                        }
                        else
                        {
                            Stop();
                        }
                    }
                }
            }
        }

        public UInt32  Interval  
        {
            get { return m_Interval; }
            set { m_Interval = value; }
        }


        public ScpTimer() 
        {
            InitializeComponent();

            m_Callback = new TimerCallback(OnTick);
        }

        public ScpTimer(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();

            m_Callback = new TimerCallback(OnTick);
        }


        public void Start() 
        {
            lock (this)
            {
                if (!Enabled)
                {
                    m_Id = timeSetEvent(Interval, 0, m_Callback, UIntPtr.Zero, EventFlags.TIME_PERIODIC);
                }
            }
        }

        public void Stop()  
        {
            lock (this)
            {
                if (Enabled)
                {
                    timeKillEvent(m_Id);
                    m_Id = 0;
                }
            }
        }


        protected void OnTick(UInt32 uTimerID, UInt32 uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2) 
        {
            if (Tick != null)
            {
                Tick(this, m_Args);
            }
        }
    }
}
