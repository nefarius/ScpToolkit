using System;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace ScpControl 
{
    public partial class UsbDevice : ScpDevice, IDsDevice 
    {
        protected ReportEventArgs m_ReportArgs = new ReportEventArgs();

        protected String   m_Instance = String.Empty, m_Mac = String.Empty;
        protected Boolean  m_Publish = false;
        protected Boolean  m_IsDisconnect = false;
        protected DateTime m_Last = DateTime.Now, m_Tick = DateTime.Now, m_Disconnect = DateTime.Now;

        public event EventHandler<DebugEventArgs>  Debug  = null;
        public event EventHandler<ReportEventArgs> Report = null;

        protected Byte[] m_Buffer = new Byte[64];
        protected Byte[] m_Master = new Byte[6];
        protected Byte[] m_Local  = new Byte[6];

        protected Byte m_ControllerId  = 0;
        protected Byte m_BatteryStatus = 0;
        protected Byte m_CableStatus   = 0;
        protected Byte m_PlugStatus    = 0;
        protected Byte m_Model         = 0;

        protected DsState m_State = DsState.Disconnected;

        protected UInt32 m_Packet = 0;

        public virtual DsModel Model 
        {
            get { return (DsModel) m_Model; }
        }
        
        public virtual DsPadId PadId 
        {
            get { return (DsPadId) m_ControllerId; }
            set
            {
                m_ControllerId = (Byte) value;

                m_ReportArgs.Pad = PadId;
            }
        }

        public virtual DsConnection Connection 
        {
            get { return DsConnection.USB; }
        }

        public virtual DsState State 
        {
            get { return (DsState) m_State; }
        }

        public virtual DsBattery Battery 
        {
            get { return (DsBattery) m_BatteryStatus; }
        }

        public virtual Byte[] BD_Address 
        {
            get { return m_Local; }
        }

        public virtual String Local  
        {
            get { return m_Mac; }
        }

        public virtual String Remote 
        {
            get { return String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Master[0], m_Master[1], m_Master[2], m_Master[3], m_Master[4], m_Master[5]); }
        }

        public virtual Boolean IsShutdown 
        {
            get { return m_IsDisconnect; }
            set { m_IsDisconnect = value; }
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


        protected virtual void Process(DateTime Now) 
        {
        }

        protected virtual void Parse(Byte[] Report) 
        {
        }

        protected virtual Boolean Shutdown() 
        {
            Stop();

            return RestartDevice(m_Instance);
        }


        protected UsbDevice(String Guid) : base(Guid) 
        {
            InitializeComponent();
        }


        public UsbDevice() 
        {
            InitializeComponent();
        }

        public UsbDevice(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public override Boolean Start() 
        {
            if (IsActive)
            {
                Array.Copy(m_Local, 0, m_ReportArgs.Report, (Int32) DsOffset.Address, m_Local.Length);

                m_ReportArgs.Report[(Int32) DsOffset.Connection] = (Byte) Connection;
                m_ReportArgs.Report[(Int32) DsOffset.Model     ] = (Byte) Model;

                m_State  = DsState.Connected;
                m_Packet = 0;

                HID_Worker.RunWorkerAsync();
                tmUpdate.Enabled = true;

                Rumble(0, 0);
                LogDebug(String.Format("-- Started Device Instance [{0}] Local [{1}] Remote [{2}]", m_Instance, Local, Remote));
            }

            return State == DsState.Connected;
        }

        public override Boolean Stop() 
        {
            if (IsActive)
            {
                tmUpdate.Enabled = false;
                m_State = DsState.Reserved;

                Publish();
            }

            return base.Stop();
        }

        public override Boolean Close() 
        {
            if (IsActive)
            {
                base.Close();

                tmUpdate.Enabled = false;
                m_State = DsState.Disconnected;

                Publish();
            }

            return !IsActive;
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


        protected void HID_Worker_Thread(object sender, DoWorkEventArgs e) 
        {
            Int32  Transfered = 0;
            Byte[] Buffer = new Byte[64];

            LogDebug("-- USB Device : HID_Worker_Thread Starting");

            while (IsActive)
            {
                try
                {
                    if (ReadIntPipe(Buffer, Buffer.Length, ref Transfered) && Transfered > 0)
                    {
                        Parse(Buffer);
                    }
                }
                catch { }
            }

            LogDebug("-- USB Device : HID_Worker_Thread Exiting");
        }

        protected void On_Timer(object sender, EventArgs e) 
        {
            lock (this)
            {
                Process(DateTime.Now);
            }
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
            return true;
        }
    }
}
