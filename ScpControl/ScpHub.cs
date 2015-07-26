using System;
using System.ComponentModel;
using System.Reflection;
using log4net;

namespace ScpControl
{
    public partial class ScpHub : Component
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IntPtr m_Reference = IntPtr.Zero;
        protected volatile bool m_Started = false;

        public ScpHub()
        {
            InitializeComponent();
        }

        public ScpHub(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public bool Active
        {
            get { return m_Started; }
        }

        public event EventHandler<ArrivalEventArgs> Arrival;
        public event EventHandler<ReportEventArgs> Report;

        protected virtual bool LogArrival(IDsDevice Arrived)
        {
            var args = new ArrivalEventArgs(Arrived);

            On_Arrival(this, args);

            return args.Handled;
        }

        public virtual bool Open()
        {
            return true;
        }

        public virtual bool Start()
        {
            return m_Started;
        }

        public virtual bool Stop()
        {
            return !m_Started;
        }

        public virtual bool Close()
        {
            if (m_Reference != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Reference);

            return !m_Started;
        }

        public virtual bool Suspend()
        {
            return true;
        }

        public virtual bool Resume()
        {
            return true;
        }

        public virtual DsPadId Notify(ScpDevice.Notified Notification, string Class, string Path)
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