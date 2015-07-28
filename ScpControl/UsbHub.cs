using System;
using System.ComponentModel;

namespace ScpControl
{
    public partial class UsbHub : ScpHub
    {
        private readonly UsbDevice[] _device = new UsbDevice[4];

        public UsbHub()
        {
            InitializeComponent();
        }

        public UsbHub(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public override Boolean Open()
        {
            for (Byte Pad = 0; Pad < _device.Length; Pad++)
            {
                _device[Pad] = new UsbDevice();

                _device[Pad].PadId = (DsPadId)Pad;
            }

            return base.Open();
        }

        public override Boolean Start()
        {
            m_Started = true;

            Byte Index = 0;

            // enumerate DS4 devices
            for (Byte instance = 0; instance < _device.Length && Index < _device.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs4();
                    current.PadId = (DsPadId)Index;

                    if (current.Open(instance))
                    {
                        if (LogArrival(current))
                        {
                            current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            _device[Index++] = current;
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
            for (Byte instance = 0; instance < _device.Length && Index < _device.Length; instance++)
            {
                try
                {
                    UsbDevice current = new UsbDs3();
                    current.PadId = (DsPadId)Index;

                    if (current.Open(instance))
                    {
                        if (LogArrival(current))
                        {
                            current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            _device[Index++] = current;
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
                for (Index = 0; Index < _device.Length; Index++)
                {
                    if (_device[Index].State == DsState.Reserved)
                    {
                        _device[Index].Start();
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
                for (Int32 Index = 0; Index < _device.Length; Index++)
                {
                    if (_device[Index].State == DsState.Connected)
                    {
                        _device[Index].Stop();
                    }
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
                for (Int32 Index = 0; Index < _device.Length; Index++)
                {
                    if (_device[Index].State == DsState.Connected)
                    {
                        _device[Index].Close();
                    }
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

        public override DsPadId Notify(ScpDevice.Notified Notification, String Class, String Path)
        {
            Log.DebugFormat("++ Notify [{0}] [{1}] [{2}]", Notification, Class, Path);

            switch (Notification)
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

                        Log.DebugFormat("Arrival event for unknown GUID {0} received", Class);

                        if (arrived.Open(Path))
                        {
                            Log.DebugFormat("-- Device Arrival [{0}]", arrived.Local);

                            if (LogArrival(arrived))
                            {
                                if (_device[(Byte)arrived.PadId].IsShutdown)
                                {
                                    _device[(Byte)arrived.PadId].IsShutdown = false;

                                    _device[(Byte)arrived.PadId].Close();
                                    _device[(Byte)arrived.PadId] = arrived;

                                    return arrived.PadId;
                                }
                                else
                                {
                                    arrived.Report += new EventHandler<ReportEventArgs>(On_Report);

                                    _device[(Byte)arrived.PadId].Close();
                                    _device[(Byte)arrived.PadId] = arrived;

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
                        for (Int32 index = 0; index < _device.Length; index++)
                        {
                            if (_device[index].State == DsState.Connected && Path == _device[index].Path)
                            {
                                Log.DebugFormat("-- Device Removal [{0}]", _device[index].Local);

                                _device[index].Stop();
                            }
                        }
                    }
                    break;
            }

            return DsPadId.None;
        }
    }
}
