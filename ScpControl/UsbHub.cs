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

            for (Byte Instance = 0; Instance < _device.Length && Index < _device.Length; Instance++)
            {
                try
                {
                    UsbDevice Current = new UsbDs4();
                    Current.PadId = (DsPadId)Index;

                    if (Current.Open(Instance))
                    {
                        if (LogArrival(Current))
                        {
                            Current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            _device[Index++] = Current;
                        }
                        else Current.Close();
                    }
                    else Current.Close();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                    break;
                }
            }

            for (Byte Instance = 0; Instance < _device.Length && Index < _device.Length; Instance++)
            {
                try
                {
                    UsbDevice Current = new UsbDs3();
                    Current.PadId = (DsPadId) Index;

                    if (Current.Open(Instance))
                    {
                        if (LogArrival(Current))
                        {
                            Current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            _device[Index++] = Current;
                        }
                        else Current.Close();
                    }
                    else Current.Close();
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
                        UsbDevice Arrived = new UsbDevice();

                        if (Class.ToUpper() == UsbDs3.USB_CLASS_GUID.ToUpper()) { Arrived = new UsbDs3(); Log.Debug("-- DS3 Arrival Event"); }
                        if (Class.ToUpper() == UsbDs4.USB_CLASS_GUID.ToUpper()) { Arrived = new UsbDs4(); Log.Debug("-- DS4 Arrival Event"); }

                        if (Arrived.Open(Path))
                        {
                            Log.DebugFormat("-- Device Arrival [{0}]", Arrived.Local);

                            if (LogArrival(Arrived))
                            {
                                if (_device[(Byte)Arrived.PadId].IsShutdown)
                                {
                                    _device[(Byte)Arrived.PadId].IsShutdown = false;

                                    _device[(Byte)Arrived.PadId].Close();
                                    _device[(Byte)Arrived.PadId] = Arrived;

                                    return Arrived.PadId;
                                }
                                else
                                {
                                    Arrived.Report += new EventHandler<ReportEventArgs>(On_Report);

                                    _device[(Byte)Arrived.PadId].Close();
                                    _device[(Byte)Arrived.PadId] = Arrived;

                                    if (m_Started) Arrived.Start();
                                    return Arrived.PadId;
                                }
                            }
                        }

                        Arrived.Close();
                    }
                    break;

                case ScpDevice.Notified.Removal:
                    {
                        for (Int32 Index = 0; Index < _device.Length; Index++)
                        {
                            if (_device[Index].State == DsState.Connected && Path == _device[Index].Path)
                            {
                                Log.DebugFormat("-- Device Removal [{0}]", _device[Index].Local);

                                _device[Index].Stop();
                            }
                        }
                    }
                    break;
            }

            return DsPadId.None;
        }
    }
}
