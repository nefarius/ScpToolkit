using System;
using System.ComponentModel;
using System.Linq;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;
using ScpControl.Sound;
using ScpControl.Usb.Ds3;
using ScpControl.Usb.Ds4;
using ScpControl.Usb.Gamepads;
using ScpControl.Utilities;

namespace ScpControl.Usb
{
    /// <summary>
    ///     Represents an Usb hub.
    /// </summary>
    public partial class UsbHub : ScpHub
    {
        private readonly UsbDevice[] _devices = new UsbDevice[4];

        #region Actions

        public override bool Open()
        {
            for (byte pad = 0; pad < _devices.Length; pad++)
            {
                _devices[pad] = new UsbDevice { PadId = (DsPadId)pad };
            }

            return base.Open();
        }

        public override bool Start()
        {
            m_Started = true;

            byte index = 0;

            // enumerate DS4 devices
            for (byte instance = 0; instance < _devices.Length && index < _devices.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs4();
                    current.PadId = (DsPadId)index;

                    if (current.Open(instance))
                    {
                        if (LogArrival(current))
                        {
                            current.HidReportReceived += OnHidReportReceived;

                            _devices[index++] = current;
                        }
                        else current.Close();
                    }
                    else current.Close();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                    break;
                }
            }

            // enumerate DS3 devices
            for (byte instance = 0; instance < _devices.Length && index < _devices.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs3();
                    current.PadId = (DsPadId)index;

                    if (current.Open(instance))
                    {
                        if (!Apply3RdPartyWorkaroundsForDs3(ref current, instance)) continue;

                        // notify bus of new device
                        if (LogArrival(current))
                        {
                            // listen for HID reports
                            current.HidReportReceived += OnHidReportReceived;

                            _devices[index++] = current;
                        }
                        else current.Close();
                    }
                    else current.Close();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                    break;
                }
            }

            var hidDevices = UsbGenericGamepad.LocalHidDevices;
        
            // enumerate generic devices
            for (byte instance = 0; instance < hidDevices.Count && index < _devices.Length; instance++)
            {
                try
                {
                    // try to create supported gamepad
                    var current = UsbGenericGamepad.DeviceFactory(hidDevices[instance].DevicePath);

                    // try next on fail
                    if (current == null) continue;

                    // try next if opening failed
                    if (!current.Open(hidDevices[instance].DevicePath)) continue;

                    current.PadId = (DsPadId) index;

                    // notify bus of new device
                    if (LogArrival(current))
                    {
                        // listen for HID reports
                        current.HidReportReceived += OnHidReportReceived;
                        current.Start();

                        _devices[index++] = current;
                    }
                    else current.Close();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                    break;
                }
            }

            try
            {
                for (index = 0; index < _devices.Length; index++)
                {
                    if (_devices[index].State == DsState.Reserved)
                    {
                        _devices[index].Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return base.Start();
        }

        public override bool Stop()
        {
            m_Started = false;

            try
            {
                foreach (var t in _devices.Where(t => t != null && t.State == DsState.Connected))
                {
                    t.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return base.Stop();
        }

        public override bool Close()
        {
            m_Started = false;

            try
            {
                foreach (var t in _devices.Where(t => t.State == DsState.Connected))
                {
                    t.Close();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return base.Close();
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

        #region Windows messaging

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string path)
        {
            Log.DebugFormat("++ Notify [{0}] [{1}] [{2}]", notification, Class, path);

            var classGuid = Guid.Parse(Class);

            switch (notification)
            {
                case ScpDevice.Notified.Arrival:
                    {
                        var arrived = new UsbDevice();

                        if (classGuid == UsbDs3.DeviceClassGuid)
                        {
                            arrived = new UsbDs3();
                            Log.Info("DualShock 3 plugged in via Usb");
                        }

                        if (classGuid == UsbDs4.DeviceClassGuid)
                        {
                            arrived = new UsbDs4();
                            Log.Info("DualShock 4 plugged in via Usb");
                        }

                        if (classGuid == UsbGenericGamepad.DeviceClassGuid)
                        {
                            arrived = UsbGenericGamepad.DeviceFactory(path);

                            // unknown or unsupported device
                            if (arrived == null)
                                break;

                            Log.Debug("Generic Gamepad plugged in via Usb");
                        }

                        Log.DebugFormat("Arrival event for GUID {0} received", classGuid);

                        if (arrived.Open(path))
                        {
                            Log.DebugFormat("Device MAC address: {0}", arrived.DeviceAddress.AsFriendlyName());

                            if (!Apply3RdPartyWorkaroundsForDs3(ref arrived, path: path)) break;

                            if (LogArrival(arrived))
                            {
                                if (_devices[(byte) arrived.PadId].IsShutdown)
                                {
                                    _devices[(byte) arrived.PadId].IsShutdown = false;

                                    _devices[(byte) arrived.PadId].Close();
                                    _devices[(byte) arrived.PadId] = arrived;

                                    return arrived.PadId;
                                }
                                arrived.HidReportReceived += OnHidReportReceived;

                                _devices[(byte) arrived.PadId].Close();
                                _devices[(byte) arrived.PadId] = arrived;

                                if (m_Started) arrived.Start();
                                return arrived.PadId;
                            }
                        }
                        else
                        {
                            Log.FatalFormat("Couldn't open device {0}", path);
                        }

                        arrived.Close();
                    }
                    break;

                case ScpDevice.Notified.Removal:
                    {
                        foreach (var t in _devices.Where(t => t.State == DsState.Connected && path == t.Path))
                        {
                            Log.InfoFormat("Device with MAC address {0} unplugged from Usb", t.DeviceAddress.AsFriendlyName());

                            // play disconnect sound
                            if (GlobalConfiguration.Instance.IsUsbDisconnectSoundEnabled)
                                AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.UsbDisconnectSoundFile);

                            t.Stop();
                        }
                    }
                    break;
            }

            return DsPadId.None;
        }

        #endregion

        #region Ctors

        public UsbHub()
        {
            InitializeComponent();
        }

        public UsbHub(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #endregion
    }
}
