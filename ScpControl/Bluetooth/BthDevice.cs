using System;
using System.ComponentModel;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Sound;

namespace ScpControl.Bluetooth
{
    /// <summary>
    ///     Represents a generic Bluetooth client device.
    /// </summary>
    public partial class BthDevice : BthConnection, IDsDevice
    {
        protected byte m_BatteryStatus = 0;
        protected bool m_Blocked, m_IsIdle = true, m_IsDisconnect;
        protected byte m_CableStatus = 0;
        protected byte m_ControllerId;
        protected IBthDevice m_Device;
        protected byte m_Init = 0;

        protected DateTime m_Last = DateTime.Now,
            m_Idle = DateTime.Now,
            m_Tick = DateTime.Now,
            m_Disconnect = DateTime.Now;

        protected uint m_Packet;
        protected byte m_PlugStatus = 0;
        private bool m_Publish;
        protected uint m_Queued = 0;
        protected readonly ScpHidReport InputReport = new ScpHidReport();
        protected DsState m_State = DsState.Disconnected;
        private readonly byte[] m_Master = new byte[6];

        public DsState State
        {
            get { return m_State; }
        }

        public DsConnection Connection
        {
            get { return DsConnection.BTH; }
        }

        public DsBattery Battery
        {
            get { return (DsBattery) m_BatteryStatus; }
        }

        public string Local
        {
            get { return MacDisplayName; }
        }

        public string Remote
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Master[0], m_Master[1], m_Master[2],
                    m_Master[3], m_Master[4], m_Master[5]);
            }
        }

        public virtual DsPadId PadId
        {
            get { return (DsPadId) m_ControllerId; }
            set { m_ControllerId = (byte) value; }
        }

        public virtual bool Start()
        {
            Buffer.BlockCopy(LocalMac, 0, InputReport.RawBytes, (int) DsOffset.Address, LocalMac.Length);

            InputReport.ConnectionType = Connection;
            InputReport.Model = Model;

            tmUpdate.Enabled = true;

            // play connection sound
            if(GlobalConfiguration.Instance.IsBluetoothConnectSoundEnabled)
                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.BluetoothConnectSoundFile);

            return m_State == DsState.Connected;
        }

        public virtual bool Rumble(byte large, byte small)
        {
            return false;
        }

        public virtual bool Pair(byte[] master)
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            m_Publish = false;
            return m_Device.HCI_Disconnect(HciHandle) > 0;
        }

        public event EventHandler<ScpHidReport> HidReportReceived;

        protected virtual void OnHidReportReceived()
        {
            InputReport.PadId = (DsPadId)m_ControllerId;
            InputReport.PadState = m_State;

            if (HidReportReceived != null) HidReportReceived(this, InputReport);
        }

        public virtual bool Stop()
        {
            if (m_State == DsState.Connected)
            {
                tmUpdate.Enabled = false;

                m_State = DsState.Reserved;
                m_Packet = 0;

                m_Publish = false;
                OnHidReportReceived();

                // play disconnect sound
                if(GlobalConfiguration.Instance.IsBluetoothDisconnectSoundEnabled)
                    AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.BluetoothDisconnectSoundFile);
            }

            return m_State == DsState.Reserved;
        }

        public virtual bool Close()
        {
            Stop();

            if (m_State == DsState.Reserved)
            {
                m_State = DsState.Disconnected;
                m_Packet = 0;

                m_Publish = false;
                OnHidReportReceived();
            }

            return m_State == DsState.Disconnected;
        }

        public virtual void ParseHidReport(byte[] report)
        {
        }

        public virtual bool InitHidReport(byte[] report)
        {
            return true;
        }

        public override string ToString()
        {
            switch (m_State)
            {
                case DsState.Disconnected:

                    return string.Format("Pad {0} : Disconnected", m_ControllerId + 1);

                case DsState.Reserved:

                    return string.Format("Pad {0} : {1} {2} - Reserved", m_ControllerId + 1, Model, Local);

                case DsState.Connected:

                    return string.Format("Pad {0} : {1} {2} - {3} {4:X8} {5}", m_ControllerId + 1, Model,
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

        protected virtual void Process(DateTime now)
        {
        }

        protected virtual void On_Timer(object sender, EventArgs e)
        {
            if (m_State == DsState.Connected)
            {
                var now = DateTime.Now;

                if (m_IsIdle && GlobalConfiguration.Instance.IdleDisconnect)
                {
                    if ((now - m_Idle).TotalMilliseconds >= GlobalConfiguration.Instance.IdleTimeout)
                    {
                        Log.Debug("++ Idle Disconnect Triggered");

                        m_IsDisconnect = false;
                        m_IsIdle = false;

                        Disconnect();
                        return;
                    }
                }
                else if (m_IsDisconnect)
                {
                    if ((now - m_Disconnect).TotalMilliseconds >= 2000)
                    {
                        Log.Debug("++ Quick Disconnect Triggered");

                        m_IsDisconnect = false;
                        m_IsIdle = false;

                        Disconnect();
                        return;
                    }
                }

                Process(now);
            }
        }

        #region Ctors

        public BthDevice()
        {
            InitializeComponent();
        }

        public BthDevice(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthDevice(IBthDevice device, byte[] master, byte lsb, byte msb)
            : base(new BthHandle(lsb, msb))
        {
            InitializeComponent();

            m_Device = device;
            m_Master = master;
        }

        #endregion
    }
}