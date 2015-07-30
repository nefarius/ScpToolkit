using System;
using System.ComponentModel;
using System.Reflection;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl
{
    public partial class BthConnection : Component, IEquatable<BthConnection>, IComparable<BthConnection>
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static ushort m_DCID = 0x40;
        protected BthHandle m_HCI_Handle;
        private BthHandle[] m_L2CAP_Cmd_Handle = new BthHandle[2];
        private BthHandle[] m_L2CAP_Int_Handle = new BthHandle[2];
        private BthHandle[] m_L2CAP_Svc_Handle = new BthHandle[2];
        protected byte[] m_Local = new byte[6] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        private DsModel m_Model = DsModel.DS3;
        protected string m_Remote_Name = string.Empty, m_Mac = string.Empty;

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

        public virtual BthHandle HCI_Handle
        {
            get { return m_HCI_Handle; }
        }

        public virtual byte[] BD_Address
        {
            get { return m_Local; }
            set
            {
                m_Local = value;
                m_Mac = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }
        }

        public virtual string Remote_Name
        {
            get { return m_Remote_Name; }
            set
            {
                m_Remote_Name = value;
                if (m_Remote_Name == "Wireless Controller") m_Model = DsModel.DS4;
            }
        }

        public virtual bool CanStartHid { get; set; }

        public virtual bool CanStartSvc { get; set; }

        public virtual bool SvcStarted { get; set; }

        public virtual bool ServiceByPass { get; set; }

        public virtual bool Started { get; set; }

        public virtual DsModel Model
        {
            get { return m_Model; }
        }

        public static ushort DCID
        {
            get { return m_DCID; }
            set
            {
                if (value < 0xFFFF) m_DCID = value;
                else m_DCID = 0x40;
            }
        }

        #region IComparable<ScpBthConnection> Members

        public virtual int CompareTo(BthConnection other)
        {
            return m_HCI_Handle.CompareTo(other.m_HCI_Handle);
        }

        #endregion

        public virtual byte[] Set(L2CAP.PSM ConnectionType, byte Lsb, byte Msb, ushort Dcid = 0)
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

                    SvcStarted = true;
                    CanStartSvc = false;

                    m_L2CAP_Svc_Handle[0] = new BthHandle(Lsb, Msb);
                    m_L2CAP_Svc_Handle[1] = new BthHandle(Dcid);

                    return m_L2CAP_Svc_Handle[1].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }

        public virtual byte[] Set(L2CAP.PSM ConnectionType, byte[] Handle)
        {
            return Set(ConnectionType, Handle[0], Handle[1]);
        }

        public virtual byte[] Get_DCID(byte Lsb, byte Msb)
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

        public virtual byte[] Get_DCID(L2CAP.PSM ConnectionType)
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

        public virtual byte[] Get_SCID(byte Lsb, byte Msb)
        {
            try
            {
                if (m_L2CAP_Cmd_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Cmd_Handle[0].Bytes;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            try
            {
                if (m_L2CAP_Int_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Int_Handle[0].Bytes;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            try
            {
                if (m_L2CAP_Svc_Handle[1].Equals(Lsb, Msb))
                {
                    return m_L2CAP_Svc_Handle[0].Bytes;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            throw new Exception("L2CAP SCID Not Found");
        }

        public virtual byte[] Get_SCID(L2CAP.PSM ConnectionType)
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
            return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2} - {6}",
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

        public virtual bool Equals(byte Lsb, byte Msb)
        {
            return m_HCI_Handle.Equals(Lsb, Msb);
        }

        public virtual bool Equals(byte[] other)
        {
            return m_HCI_Handle.Equals(other);
        }

        #endregion
    }
}