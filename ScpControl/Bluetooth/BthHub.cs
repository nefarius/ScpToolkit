using System.ComponentModel;
using System.Net.NetworkInformation;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Utilities;

namespace ScpControl.Bluetooth
{
    /// <summary>
    ///     Represents a Bluetooth hub.
    /// </summary>
    public partial class BthHub : ScpHub
    {
        private BthDongle _device;

        #region Windows messaging

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string path)
        {
            Log.DebugFormat("++ Notify [{0}] [{1}] [{2}]", notification, Class, path);

            switch (notification)
            {
                case ScpDevice.Notified.Arrival:
                {
                    if (_device.State != DsState.Connected)
                    {
                        var arrived = new BthDongle();

                        if (arrived.Open(path))
                        {
                            Log.DebugFormat("-- Device Arrival [{0}]", arrived.BluetoothHostAddress.AsFriendlyName());

                            _device.Close();
                            _device = arrived;

                            _device.DeviceArrived += OnDeviceArrival;
                            _device.HidReportReceived += OnHidReportReceived;

                            if (m_Started) _device.Start();
                            break;
                        }

                        arrived.Close();
                        arrived.Dispose();
                    }
                }
                    break;

                case ScpDevice.Notified.Removal:

                    if (_device.Path == path)
                    {
                        Log.DebugFormat("-- Device Removal [{0}]", _device.BluetoothHostAddress.AsFriendlyName());

                        _device.Stop();
                    }
                    break;
            }

            return DsPadId.None;
        }

        #endregion

        #region Ctors

        public BthHub()
        {
            InitializeComponent();
        }

        public BthHub(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion

        #region Properties

        public string Dongle
        {
            get { return _device != null ? _device.ToString() : "<UNKNOWN>"; }
        }

        public PhysicalAddress BluetoothHostAddress
        {
            get { return _device.BluetoothHostAddress; }
        }

        public bool Pairable
        {
            get { return m_Started && _device.State == DsState.Connected && _device.Initialised; }
        }

        #endregion

        #region Actions

        public override bool Open()
        {
            _device = new BthDongle();

            _device.DeviceArrived += OnDeviceArrival;
            _device.HidReportReceived += OnHidReportReceived;

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

            if (_device != null && _device.State == DsState.Connected)
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

        #endregion
    }
}
