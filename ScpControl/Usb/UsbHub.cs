using System;
using System.ComponentModel;
using System.Linq;
using ScpControl.ScpCore;
using ScpControl.Sound;

namespace ScpControl.Usb
{
    /// <summary>
    ///     Represents an USB hub.
    /// </summary>
    public partial class UsbHub : ScpHub
    {
        private readonly UsbDevice[] _devices = new UsbDevice[4];

        public override bool Open()
        {
            for (byte pad = 0; pad < _devices.Length; pad++)
            {
                _devices[pad] = new UsbDevice {PadId = (DsPadId) pad};
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
                    current.PadId = (DsPadId) index;

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
                    current.PadId = (DsPadId) index;

                    if (current.Open(instance))
                    {
                        #region Afterglow AP.2 Wireless Controller for PS3 workaround

                        // if Afterglow AP.2 Wireless Controller for PS3 is detected...
                        if (current.VendorId == 0x0E6F && current.ProductId == 0x0214)
                        {
                            Log.InfoFormat(
                                "Afterglow AP.2 Wireless Controller for PS3 detected [VID: {0:X}] [PID: {1:X}], workaround applied",
                                current.VendorId, current.ProductId);
                            // ...close device...
                            current.Close();
                            // ...and create customized object
                            current = new UsbDs3Afterglow();

                            // open and continue plug-in procedure on success
                            if (!current.Open(instance))
                                continue;
                        }

                        #endregion

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
                foreach (var t in _devices.Where(t => t.State == DsState.Connected))
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

        public override DsPadId Notify(ScpDevice.Notified notification, string Class, string Path)
        {
            Log.InfoFormat("++ Notify [{0}] [{1}] [{2}]", notification, Class, Path);

            switch (notification)
            {
                case ScpDevice.Notified.Arrival:
                {
                    var arrived = new UsbDevice();

                    if (string.Equals(Class, UsbDs3.USB_CLASS_GUID, StringComparison.CurrentCultureIgnoreCase))
                    {
                        arrived = new UsbDs3();
                        Log.Debug("-- DS3 Arrival Event");
                    }

                    if (string.Equals(Class, UsbDs4.USB_CLASS_GUID, StringComparison.CurrentCultureIgnoreCase))
                    {
                        arrived = new UsbDs4();
                        Log.Debug("-- DS4 Arrival Event");
                    }

                    Log.InfoFormat("Arrival event for GUID {0} received", Class);

                    if (arrived.Open(Path))
                    {
                        Log.InfoFormat("-- Device Arrival [{0}]", arrived.Local);

                        #region Afterglow AP.2 Wireless Controller for PS3 workaround

                        // if Afterglow AP.2 Wireless Controller for PS3 is detected...
                        if (arrived.VendorId == 0x0E6F && arrived.ProductId == 0x0214)
                        {
                            Log.InfoFormat(
                                "Afterglow AP.2 Wireless Controller for PS3 detected [VID: {0:X}] [PID: {1:X}], workaround applied",
                                arrived.VendorId, arrived.ProductId);
                            // ...close device...
                            arrived.Close();
                            // ...and create customized object
                            arrived = new UsbDs3Afterglow();

                            // open and continue plug-in procedure on success
                            if (!arrived.Open(Path))
                                break;
                        }

                        #endregion

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

                    arrived.Close();
                }
                    break;

                case ScpDevice.Notified.Removal:
                {
                    foreach (var t in _devices.Where(t => t.State == DsState.Connected && Path == t.Path))
                    {
                        Log.InfoFormat("-- Device Removal [{0}]", t.Local);

                        AudioPlayer.Instance.PlayCustomFile(GlobalConfiguration.Instance.UsbDisconnectSoundFile);

                        t.Stop();
                    }
                }
                    break;
            }

            return DsPadId.None;
        }

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