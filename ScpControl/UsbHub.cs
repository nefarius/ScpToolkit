using System;
using System.ComponentModel;

namespace ScpControl 
{
    public partial class UsbHub : ScpHub 
    {
        protected UsbDevice[] Device = new UsbDevice[4];


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
            for (Byte Pad = 0; Pad < Device.Length; Pad++)
            {
                Device[Pad] = new UsbDevice();

                Device[Pad].PadId = (DsPadId) Pad;
            }

            return base.Open();
        }

        public override Boolean Start() 
        {
            m_Started = true;

            Byte Index = 0;

            for (Byte Instance = 0; Instance < Device.Length && Index < Device.Length; Instance++)
            {
                try
                {
                    UsbDevice Current = new UsbDs4();
                    Current.PadId = (DsPadId)Index;

                    if (Current.Open(Instance))
                    {
                        if (LogArrival(Current))
                        {
                            Current.Debug  += new EventHandler<DebugEventArgs> (On_Debug);
                            Current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            Device[Index++] = Current;
                        }
                        else Current.Close();
                    }
                    else Current.Close();
                }
                catch { break; }
            }

            for (Byte Instance = 0; Instance < Device.Length && Index < Device.Length; Instance++)
            {
                try
                {
                    UsbDevice Current = new UsbDs3();
                    Current.PadId = (DsPadId)Index;

                    if (Current.Open(Instance))
                    {
                        if (LogArrival(Current))
                        {
                            Current.Debug  += new EventHandler<DebugEventArgs> (On_Debug);
                            Current.Report += new EventHandler<ReportEventArgs>(On_Report);

                            Device[Index++] = Current;
                        }
                        else Current.Close();
                    }
                    else Current.Close();
                }
                catch { break; }
            }

            try
            {
                for (Index = 0; Index < Device.Length; Index++)
                {
                    if (Device[Index].State == DsState.Reserved)
                    {
                        Device[Index].Start();
                    }
                }
            }
            catch { }

            return base.Start();
        }

        public override Boolean Stop()  
        {
            m_Started = false;

            try
            {
                for (Int32 Index = 0; Index < Device.Length; Index++)
                {
                    if (Device[Index].State == DsState.Connected)
                    {
                        Device[Index].Stop();
                    }
                }
            }
            catch { }

            return base.Stop();
        }

        public override Boolean Close() 
        {
            m_Started = false;

            try
            {
                for (Int32 Index = 0; Index < Device.Length; Index++)
                {
                    if (Device[Index].State == DsState.Connected)
                    {
                        Device[Index].Close();
                    }
                }
            }
            catch { }

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
            LogDebug(String.Format("++ Notify [{0}] [{1}] [{2}]", Notification, Class, Path));

            switch (Notification)
            {
                case ScpDevice.Notified.Arrival:
                    {
                        UsbDevice Arrived = new UsbDevice();

                        if (Class.ToUpper() == UsbDs3.USB_CLASS_GUID.ToUpper()) { Arrived = new UsbDs3(); LogDebug("-- DS3 Arrival Event"); }
                        if (Class.ToUpper() == UsbDs4.USB_CLASS_GUID.ToUpper()) { Arrived = new UsbDs4(); LogDebug("-- DS4 Arrival Event"); }

                        if (Arrived.Open(Path))
                        {
                            LogDebug(String.Format("-- Device Arrival [{0}]", Arrived.Local));

                            if (LogArrival(Arrived))
                            {
                                if (Device[(Byte) Arrived.PadId].IsShutdown)
                                {
                                    Device[(Byte) Arrived.PadId].IsShutdown = false;

                                    Device[(Byte) Arrived.PadId].Close();
                                    Device[(Byte) Arrived.PadId] = Arrived;

                                    return Arrived.PadId;
                                }
                                else
                                {
                                    Arrived.Debug  += new EventHandler<DebugEventArgs> (On_Debug );
                                    Arrived.Report += new EventHandler<ReportEventArgs>(On_Report);

                                    Device[(Byte) Arrived.PadId].Close();
                                    Device[(Byte) Arrived.PadId] = Arrived;

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
                        for (Int32 Index = 0; Index < Device.Length; Index++)
                        {
                            if (Device[Index].State == DsState.Connected && Path == Device[Index].Path)
                            {
                                LogDebug(String.Format("-- Device Removal [{0}]", Device[Index].Local));

                                Device[Index].Stop();
                            }
                        }
                    }
                    break;
            }

            return DsPadId.None;
        }
    }
}
