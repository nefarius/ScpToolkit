using System.ComponentModel;

namespace ScpControl.Bluetooth
{
    public partial class BthHub : ScpHub
    {
        private BthDongle _device;

        public BthHub()
        {
            InitializeComponent();
        }

        public BthHub(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public string Dongle
        {
            get
            {
                return _device!=null ? _device.ToString() : "<UNKNOWN>";
            }
        }

        public string Master
        {
            get { return _device.Local; }
        }

        public bool Pairable
        {
            get { return m_Started && _device.State == DsState.Connected && _device.Initialised; }
        }

        public override bool Open()
        {
            _device = new BthDongle();

            _device.Arrival += On_Arrival;
            _device.Report += On_Report;

            if (!_device.Open()) _device.Close();

            return true;
        }

        public override bool Start()
        {
            m_Started = true;

            if (_device.State == DsState.Reserved)
            {
                _device.Start();
            }

            return m_Started;
        }

        public override bool Stop()
        {
            m_Started = false;

            if (_device.State == DsState.Connected)
            {
                _device.Stop();
            }

            return !m_Started;
        }

        public override bool Close()
        {
            m_Started = false;

            return _device.Close();
        }

        public override bool Suspend()
        {
            Stop();
            Close();

            return base.Suspend();
        }

        public override bool Resume()
        {
            Open();
            Start();

            return base.Resume();
        }

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string Path)
        {
            Log.DebugFormat("++ Notify [{0}] [{1}] [{2}]", notification, Class, Path);

            switch (notification)
            {
                case ScpDevice.Notified.Arrival:
                {
                    if (_device.State != DsState.Connected)
                    {
                        var arrived = new BthDongle();

                        if (arrived.Open(Path))
                        {
                            Log.DebugFormat("-- Device Arrival [{0}]", arrived.Local);

                            _device.Close();
                            _device = arrived;

                            _device.Arrival += On_Arrival;
                            _device.Report += On_Report;

                            if (m_Started) _device.Start();
                            break;
                        }

                        arrived.Close();
                        arrived.Dispose();
                    }
                }
                    break;

                case ScpDevice.Notified.Removal:

                    if (_device.Path == Path)
                    {
                        Log.DebugFormat("-- Device Removal [{0}]", _device.Local);

                        _device.Stop();
                    }
                    break;
            }

            return DsPadId.None;
        }
    }
}