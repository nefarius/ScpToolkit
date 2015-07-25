using System;
using System.ComponentModel;

namespace ScpControl 
{
    public partial class ScpHub : Component 
    {
        protected IntPtr m_Reference = IntPtr.Zero;
        protected volatile Boolean m_Started = false;

        public event EventHandler<DebugEventArgs>   Debug   = null;
        public event EventHandler<ArrivalEventArgs> Arrival = null;
        public event EventHandler<ReportEventArgs>  Report  = null;

        protected virtual Boolean LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            On_Debug(this, args);

            return true;
        }
        protected virtual Boolean LogArrival(IDsDevice Arrived) 
        {
            ArrivalEventArgs args = new ArrivalEventArgs(Arrived);

            On_Arrival(this, args);

            return args.Handled;
        }

        public Boolean Active 
        {
            get { return m_Started; }
        }


        public ScpHub() 
        {
            InitializeComponent();
        }

        public ScpHub(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public virtual Boolean Open()  
        {
            return true;
        }

        public virtual Boolean Start() 
        {
            return m_Started;
        }

        public virtual Boolean Stop()  
        {
            return !m_Started;
        }

        public virtual Boolean Close() 
        {
            if (m_Reference != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Reference);

            return !m_Started;
        }


        public virtual Boolean Suspend() 
        {
            return true;
        }

        public virtual Boolean Resume()  
        {
            return true;
        }


        public virtual DsPadId Notify(ScpDevice.Notified Notification, String Class, String Path) 
        {
            switch (Notification)
            {
                case ScpDevice.Notified.Arrival:
                    break;

                case ScpDevice.Notified.Removal:
                    break;
            }

            return DsPadId.None;
        }

        protected virtual void On_Debug(object sender, DebugEventArgs e)     
        {
            if (Debug != null) Debug(sender, e);
        }

        protected virtual void On_Arrival(object sender, ArrivalEventArgs e) 
        {
            if (Arrival != null) Arrival(this, e);
        }

        protected virtual void On_Report(object sender, ReportEventArgs e)   
        {
            if (Report != null) Report(sender, e);
        }
    }
}
