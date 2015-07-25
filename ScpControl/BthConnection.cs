using System;
using System.ComponentModel;

namespace ScpControl 
{
    public partial class BthConnection : Component, IEquatable<BthConnection>, IComparable<BthConnection> 
    {
        protected static UInt16 m_DCID = 0x40;

        protected BthHandle   m_HCI_Handle;
        protected BthHandle[] m_L2CAP_Cmd_Handle = new BthHandle[2];
        protected BthHandle[] m_L2CAP_Int_Handle = new BthHandle[2];
        protected BthHandle[] m_L2CAP_Svc_Handle = new BthHandle[2];

        protected Byte[] m_Local = new Byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        protected String m_Remote_Name = String.Empty, m_Mac = String.Empty;

        public virtual BthHandle HCI_Handle 
        {
            get { return m_HCI_Handle; }
        }

        public virtual Byte[] BD_Address 
        {
            get { return m_Local; }
            set { m_Local = value; m_Mac = String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2], m_Local[3], m_Local[4], m_Local[5]); }
        }

        public virtual String Remote_Name 
        {
            get { return m_Remote_Name; }
            set 
            { 
                m_Remote_Name = value;
                if (m_Remote_Name == "Wireless Controller") m_Model = DsModel.DS4;
            }
        }

        protected Boolean m_CanStartHid = false;
        public virtual Boolean CanStartHid 
        {
            get { return m_CanStartHid; }
            set { m_CanStartHid = value; }
        }

        protected Boolean m_CanStartSvc = false;
        public virtual Boolean CanStartSvc 
        {
            get { return m_CanStartSvc; }
            set { m_CanStartSvc = value; }
        }

        protected Boolean m_SvcStarted = false;
        public virtual Boolean SvcStarted 
        {
            get { return m_SvcStarted; }
            set { m_SvcStarted = value; }
        }

        protected Boolean m_ServiceBypass = false;
        public virtual Boolean ServiceByPass 
        {
            get { return m_ServiceBypass; }
            set { m_ServiceBypass = value; }
        }

        protected Boolean m_Started = false;
        public virtual Boolean Started 
        {
            get { return m_Started;  }
            set { m_Started = value; }
        }


        protected DsModel m_Model = DsModel.DS3;
        public virtual DsModel Model 
        {
            get { return m_Model;  }
        }

        public static UInt16 DCID 
        {
            get { return m_DCID; }
            set { if (value < 0xFFFF) m_DCID = value; else m_DCID = 0x40; }
        }


        public BthConnection() 
        {
            InitializeComponent();
        }

        public BthConnection(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthConnection(BthHandle HCI_Handle) 
        {
            InitializeComponent();

            m_HCI_Handle = HCI_Handle;
        }


        public virtual Byte[] Set(L2CAP.PSM ConnectionType, Byte Lsb, Byte Msb, UInt16 Dcid = 0) 
        {
            switch (ConnectionType)
            {
                case L2CAP.PSM.HID_Command:

                    m_L2CAP_Cmd_Handle[0] = new BthHandle(Lsb, Msb);
                    m_L2CAP_Cmd_Handle[1] = new BthHandle(DCID++);

                    return m_L2CAP_Cmd_Handle[1].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    CanStartSvc = true;

                    m_L2CAP_Int_Handle[0] = new BthHandle(Lsb, Msb);
                    m_L2CAP_Int_Handle[1] = new BthHandle(DCID++);

                    return m_L2CAP_Int_Handle[1].Bytes;

                case L2CAP.PSM.HID_Service:

                    SvcStarted = true; CanStartSvc = false;

                    m_L2CAP_Svc_Handle[0] = new BthHandle(Lsb, Msb);
                    m_L2CAP_Svc_Handle[1] = new BthHandle(Dcid);

                    return m_L2CAP_Svc_Handle[1].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }

        public virtual Byte[] Set(L2CAP.PSM ConnectionType, Byte[] Handle) 
        {
            return Set(ConnectionType, Handle[0], Handle[1]);
        }


        public virtual Byte[] Get_DCID(Byte Lsb, Byte Msb) 
        {
            if (m_L2CAP_Cmd_Handle[0].Equals(Lsb, Msb))
            {
                return m_L2CAP_Cmd_Handle[1].Bytes;
            }

            if (m_L2CAP_Int_Handle[0].Equals(Lsb, Msb))
            {
                return m_L2CAP_Int_Handle[1].Bytes;
            }

            if (m_L2CAP_Svc_Handle[0].Equals(Lsb, Msb))
            {
                return m_L2CAP_Svc_Handle[1].Bytes;
            }

            throw new Exception("L2CAP DCID Not Found");
        }

        public virtual Byte[] Get_DCID(L2CAP.PSM ConnectionType) 
        {
            switch (ConnectionType)
            {
                case L2CAP.PSM.HID_Command:

                    return m_L2CAP_Cmd_Handle[1].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    return m_L2CAP_Int_Handle[1].Bytes;

                case L2CAP.PSM.HID_Service:

                    return m_L2CAP_Svc_Handle[1].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }


        public virtual Byte[] Get_SCID(Byte Lsb, Byte Msb) 
        {
            try
            {
                if (m_L2CAP_Cmd_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Cmd_Handle[0].Bytes;
                }
            }
            catch { }

            try
            {
                if (m_L2CAP_Int_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Int_Handle[0].Bytes;
                }
            }
            catch { }

            try
            {
                if (m_L2CAP_Svc_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Svc_Handle[0].Bytes;
                }
            }
            catch { }

            throw new Exception("L2CAP SCID Not Found");
        }

        public virtual Byte[] Get_SCID(L2CAP.PSM ConnectionType) 
        {
            switch (ConnectionType)
            {
                case L2CAP.PSM.HID_Command:

                    return m_L2CAP_Cmd_Handle[0].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    return m_L2CAP_Int_Handle[0].Bytes;

                case L2CAP.PSM.HID_Service:

                    return m_L2CAP_Svc_Handle[0].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }


        public override string ToString() 
        {
            return String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2} - {6}",
                m_Local[5],
                m_Local[4],
                m_Local[3],
                m_Local[2],
                m_Local[1],
                m_Local[0],
                m_Remote_Name
                );
        }

        #region IEquatable<ScpBthConnection> Members

        public virtual bool Equals(BthConnection other) 
        {
            return m_HCI_Handle.Equals(other.m_HCI_Handle);
        }

        public virtual bool Equals(Byte Lsb, Byte Msb) 
        {
            return m_HCI_Handle.Equals(Lsb, Msb);
        }

        public virtual bool Equals(Byte[] other) 
        {
            return m_HCI_Handle.Equals(other);
        }

        #endregion

        #region IComparable<ScpBthConnection> Members

        public virtual int CompareTo(BthConnection other) 
        {
            return m_HCI_Handle.CompareTo(other.m_HCI_Handle);
        }

        #endregion
    }
}
