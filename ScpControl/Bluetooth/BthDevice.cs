using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
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
        protected DsState m_State = DsState.Disconnected;

        public DsState State
        {
            get { return m_State; }
        }

        public DsConnection Connection
        {
            get { return DsConnection.Bluetooth; }
        }

        public DsBattery Battery
        {
            get { return (DsBattery) m_BatteryStatus; }
        }

        public PhysicalAddress HostAddress { get; protected set; }

        public virtual DsPadId PadId { get; set; }

        public virtual bool Start()
        {
            tmUpdate.Enabled = true;

            // play connection sound
            if (GlobalConfiguration.Instance.IsBluetoothConnectSoundEnabled)
                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.BluetoothConnectSoundFile);

            return m_State == DsState.Connected;
        }

        public virtual bool Rumble(byte large, byte small)
        {
            return false;
        }

        public virtual bool Pair(PhysicalAddress master)
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            m_Publish = false;
            return m_Device.HCI_Disconnect(HciHandle) > 0;
        }

        public ScpHidReport NewHidReport()
        {
            return new ScpHidReport
            {
                PadId = PadId,
                PadState = m_State,
                ConnectionType = Connection,
                Model = Model,
                PadMacAddress = DeviceAddress
            };
        }

        public event EventHandler<ScpHidReport> HidReportReceived;

        protected virtual void OnHidReportReceived(ScpHidReport report)
        {
            if (HidReportReceived != null) HidReportReceived(this, report);
        }

        public virtual bool Stop()
        {
            if (m_State == DsState.Connected)
            {
                tmUpdate.Enabled = false;

                m_State = DsState.Reserved;
                m_Packet = 0;

                m_Publish = false;
                OnHidReportReceived(NewHidReport());

                // play disconnect sound
                if (GlobalConfiguration.Instance.IsBluetoothDisconnectSoundEnabled)
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
                OnHidReportReceived(NewHidReport());
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

                    return string.Format("Pad {0} : Disconnected", PadId);

                case DsState.Reserved:

                    return string.Format("Pad {0} : {1} {2} - Reserved", PadId, Model, DeviceAddress);

                case DsState.Connected:

                    return string.Format("Pad {0} : {1} {2} - {3} {4:X8} {5}", PadId, Model,
                        DeviceAddress,
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
            if (m_State != DsState.Connected) return;

            #region Calculate and trigger idle auto-disconnect

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

            #endregion

            Process(now);
        }

        #region Ctors

        public BthDevice()
        {
            InitializeComponent();

            DeviceAddress = PhysicalAddress.None;
            HostAddress = PhysicalAddress.None;
        }

        public BthDevice(IContainer container) : this()
        {
            container.Add(this);
        }

        public BthDevice(IBthDevice device, PhysicalAddress master, byte lsb, byte msb)
            : base(new BthHandle(lsb, msb))
        {
            InitializeComponent();

            m_Device = device;
            HostAddress = master;
        }

        #endregion
    }
}