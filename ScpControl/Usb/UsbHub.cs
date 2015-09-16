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

        public override Boolean Open()
        {
            for (Byte pad = 0; pad < _devices.Length; pad++)
            {
                _devices[pad] = new UsbDevice { PadId = (DsPadId)pad };
            }

            return base.Open();
        }

        public override Boolean Start()
        {
            m_Started = true;

            Byte index = 0;

            // enumerate DS4 devices
            for (Byte instance = 0; instance < _devices.Length && index < _devices.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs4();
                    current.PadId = (DsPadId)index;

                    if (current.Open(instance))
                    {
                        if (LogArrival(current))
                        {
                            current.HidReportReceived += new EventHandler<ReportEventArgs>(OnHidReportReceived);

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
            for (Byte instance = 0; instance < _devices.Length && index < _devices.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs3();
                    current.PadId = (DsPadId)index;

                    if (current.Open(instance))
                    {
                        if (LogArrival(current))
                        {
                            current.HidReportReceived += new EventHandler<ReportEventArgs>(OnHidReportReceived);

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

        public override Boolean Stop()
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

        public override Boolean Close()
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

        public override Boolean Suspend()
        {
            Stop();
            Close();

            return base.Suspend();
        }

        public override Boolean Resume()
        {
            Open();
            Start();

            return base.Resume();
        }

        public override DsPadId Notify(ScpDevice.Notified notification, String Class, String Path)
        {
            Log.InfoFormat("++ Notify [{0}] [{1}] [{2}]", notification, Class, Path);

            switch (notification)
            {
                case ScpDevice.Notified.Arrival:
                    {
                        UsbDevice arrived = new UsbDevice();

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

                            if (LogArrival(arrived))
                            {
                                if (_devices[(Byte)arrived.PadId].IsShutdown)
                                {
                                    _devices[(Byte)arrived.PadId].IsShutdown = false;

                                    _devices[(Byte)arrived.PadId].Close();
                                    _devices[(Byte)arrived.PadId] = arrived;

                                    return arrived.PadId;
                                }
                                else
                                {
                                    arrived.HidReportReceived += new EventHandler<ReportEventArgs>(OnHidReportReceived);

                                    _devices[(Byte)arrived.PadId].Close();
                                    _devices[(Byte)arrived.PadId] = arrived;

                                    if (m_Started) arrived.Start();
                                    return arrived.PadId;
                                }
                            }
                        }

                        arrived.Close();
                    }
                    break;

                case ScpDevice.Notified.Removal:
                    {
                        foreach (UsbDevice t in _devices.Where(t => t.State == DsState.Connected && Path == t.Path))
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
    }
}
