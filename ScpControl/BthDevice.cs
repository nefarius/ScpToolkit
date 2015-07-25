using System;
using System.ComponentModel;
using System.Threading;
using System.Text;

namespace ScpControl 
{
    public partial class BthDevice : BthConnection, IDsDevice 
    {
        public event EventHandler<DebugEventArgs>  Debug  = null;
        public event EventHandler<ReportEventArgs> Report = null;

        protected ReportEventArgs m_ReportArgs = new ReportEventArgs();

        protected Byte       m_Init = 0;
        protected Boolean    m_Publish = false;
        protected Boolean    m_Blocked = false, m_IsIdle = true, m_IsDisconnect = false;
        protected UInt32     m_Queued = 0;
        protected DateTime   m_Last = DateTime.Now, m_Idle = DateTime.Now, m_Tick = DateTime.Now, m_Disconnect = DateTime.Now;
        protected IBthDevice m_Device;

        protected Byte[] m_Master = new Byte[6];

        protected Byte m_ControllerId  = 0;
        protected Byte m_BatteryStatus = 0;
        protected Byte m_CableStatus   = 0;
        protected Byte m_PlugStatus    = 0;

        protected DsState m_State = DsState.Disconnected;

        protected UInt32 m_Packet = 0;

        public DsState      State      
        {
            get { return m_State; }
        }
        public DsConnection Connection 
        {
            get { return DsConnection.BTH; }
        }
        public DsBattery    Battery    
        {
            get { return (DsBattery)m_BatteryStatus; }
        }

        public String Local  
        {
            get { return m_Mac; }
        }
        public String Remote 
        {
            get { return String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Master[0], m_Master[1], m_Master[2], m_Master[3], m_Master[4], m_Master[5]); }
        }

        public virtual DsPadId PadId 
        {
            get { return (DsPadId) m_ControllerId; }
            set { m_ControllerId = (Byte) value; }
        }


        protected virtual void Publish() 
        {
            m_ReportArgs.Report[0] = m_ControllerId;
            m_ReportArgs.Report[1] = (Byte) m_State;

            if (Report != null) Report(this, m_ReportArgs);
        }

        protected virtual void LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            if (Debug != null)
            {
                Debug(this, args);
            }
        }


        public BthDevice() 
        {
            InitializeComponent();
        }

        public BthDevice(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthDevice(IBthDevice Device, Byte[] Master, Byte Lsb, Byte Msb) : base(new BthHandle(Lsb, Msb)) 
        {
            InitializeComponent();

            m_Device = Device;
            m_Master = Master;
        }


        public virtual Boolean Start() 
        {
            Array.Copy(m_Local, 0, m_ReportArgs.Report, (Int32) DsOffset.Address, m_Local.Length);

            m_ReportArgs.Report[(Int32) DsOffset.Connection] = (Byte) Connection;
            m_ReportArgs.Report[(Int32) DsOffset.Model     ] = (Byte) Model;

            tmUpdate.Enabled = true;

            return m_State == DsState.Connected;
        }

        public virtual Boolean Stop()  
        {
            if (m_State == DsState.Connected)
            {
                tmUpdate.Enabled = false;

                m_State = DsState.Reserved;
                m_Packet = 0;

                m_Publish = false;
                Publish();
            }

            return m_State == DsState.Reserved;
        }

        public virtual Boolean Close() 
        {
            Stop();

            if (m_State == DsState.Reserved)
            {
                m_State = DsState.Disconnected;
                m_Packet = 0;

                m_Publish = false;
                Publish();
            }

            return m_State == DsState.Disconnected;
        }


        public virtual void Parse(Byte[] Report) 
        {
        }


        public virtual Boolean Rumble(Byte Large, Byte Small) 
        {
            return false;
        }

        public virtual Boolean Pair(Byte[] Master) 
        {
            return false;
        }

        public virtual Boolean Disconnect() 
        {
            m_Publish = false;
            return m_Device.HCI_Disconnect(m_HCI_Handle) > 0;
        }


        public virtual Boolean InitReport(Byte[] Report) 
        {
            return true;
        }

        public override String ToString() 
        {
            switch ((DsState) m_State)
            {
                case DsState.Disconnected:

                    return String.Format("Pad {0} : Disconnected", m_ControllerId + 1);

                case DsState.Reserved:

                    return String.Format("Pad {0} : {1} {2} - Reserved", m_ControllerId + 1, Model, Local);

                case DsState.Connected:

                    return String.Format("Pad {0} : {1} {2} - {3} {4:X8} {5}", m_ControllerId + 1, Model,
                        Local,
                        Connection,
                        m_Packet,
                        Battery
                        );
            }

            throw new Exception();
        }


        public virtual void Completed() 
        {
            lock (this)
            {
                m_Blocked = false;
            }
        }

        protected virtual void Process(DateTime Now) 
        {
        }

        protected virtual void On_Timer(object sender, EventArgs e) 
        {
            if (m_State == DsState.Connected)
            {
                DateTime Now = DateTime.Now;

                if (m_IsIdle && Global.IdleDisconnect)
                {
                    if ((Now - m_Idle).TotalMilliseconds >= Global.IdleTimeout)
                    {
                        LogDebug("++ Idle Disconnect Triggered");

                        m_IsDisconnect = false;
                        m_IsIdle = false;

                        Disconnect();
                        return;
                    }
                }
                else if (m_IsDisconnect)
                {
                    if ((Now - m_Disconnect).TotalMilliseconds >= 2000)
                    {
                        LogDebug("++ Quick Disconnect Triggered");

                        m_IsDisconnect = false;
                        m_IsIdle = false;

                        Disconnect();
                        return;
                    }
                }

                Process(Now);
            }
        }
    }
}
