using System;
using System.ComponentModel;
using System.Reflection;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl.Bluetooth
{
    public partial class BthConnection : Component, IEquatable<BthConnection>, IComparable<BthConnection>
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static ushort m_DCID = 0x40;
        protected BthHandle m_HCI_Handle;
        private BthHandle[] m_L2CAP_Cmd_Handle = new BthHandle[2];
        private BthHandle[] m_L2CAP_Int_Handle = new BthHandle[2];
        private BthHandle[] m_L2CAP_Svc_Handle = new BthHandle[2];
        protected byte[] LocalMac = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private DsModel _model = DsModel.DS3;
        protected string m_Remote_Name = string.Empty, MacDisplayName = string.Empty;

        public BthConnection()
        {
            InitializeComponent();
        }

        public BthConnection(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public BthConnection(BthHandle hciHandle)
        {
            InitializeComponent();

            m_HCI_Handle = hciHandle;
        }

        public virtual BthHandle HciHandle
        {
            get { return m_HCI_Handle; }
        }

        public virtual byte[] BD_Address
        {
            get { return LocalMac; }
            set
            {
                LocalMac = value;
                MacDisplayName = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", LocalMac[0], LocalMac[1], LocalMac[2],
                    LocalMac[3], LocalMac[4], LocalMac[5]);
            }
        }

        public virtual string RemoteName
        {
            get { return m_Remote_Name; }
            set
            {
                m_Remote_Name = value;
                if (m_Remote_Name == "Wireless Controller") _model = DsModel.DS4;
            }
        }

        public virtual bool CanStartHid { get; set; }

        public virtual bool CanStartSvc { get; set; }

        public virtual bool SvcStarted { get; set; }

        public virtual bool ServiceByPass { get; set; }

        public virtual bool Started { get; set; }

        public virtual DsModel Model
        {
            get { return _model; }
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

        public virtual byte[] Set(L2CAP.PSM connectionType, byte Lsb, byte Msb, ushort Dcid = 0)
        {
            switch (connectionType)
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

        public virtual byte[] Set(L2CAP.PSM connectionType, byte[] handle)
        {
            return Set(connectionType, handle[0], handle[1]);
        }

        /// <summary>
        ///     Destination Channel Identifier.
        /// </summary>
        /// <remarks>Used as the device local end point for an L2CAP transmission. It represents the channel endpoint on the device receiving the message. It is a device local name only. See also SCID.</remarks>
        /// <param name="lsb"></param>
        /// <param name="msb"></param>
        /// <returns></returns>
        public virtual byte[] Get_DCID(byte lsb, byte msb)
        {
            if (m_L2CAP_Cmd_Handle[0].Equals(lsb, msb))
            {
                return m_L2CAP_Cmd_Handle[1].Bytes;
            }

            if (m_L2CAP_Int_Handle[0].Equals(lsb, msb))
            {
                return m_L2CAP_Int_Handle[1].Bytes;
            }

            if (m_L2CAP_Svc_Handle[0].Equals(lsb, msb))
            {
                return m_L2CAP_Svc_Handle[1].Bytes;
            }

            throw new Exception("L2CAP DCID Not Found");
        }

        /// <summary>
        ///     Destination Channel Identifier.
        /// </summary>
        /// <remarks>Used as the device local end point for an L2CAP transmission. It represents the channel endpoint on the device receiving the message. It is a device local name only. See also SCID.</remarks>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        public virtual byte[] Get_DCID(L2CAP.PSM connectionType)
        {
            switch (connectionType)
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

        /// <summary>
        ///     Source Channel Identifier.
        /// </summary>
        /// <remarks>Used in the L2CAP layer to indicate the channel endpoint on the device sending the L2CAP message. It is a device local name only.</remarks>
        /// <param name="lsb"></param>
        /// <param name="msb"></param>
        /// <returns></returns>
        public virtual byte[] Get_SCID(byte lsb, byte msb)
        {
            if (m_L2CAP_Cmd_Handle[1] != null && m_L2CAP_Cmd_Handle[1].Equals(lsb, msb))
            {
                return m_L2CAP_Cmd_Handle[0].Bytes;
            }

            if (m_L2CAP_Int_Handle[1] != null && m_L2CAP_Int_Handle[1].Equals(lsb, msb))
            {
                return m_L2CAP_Int_Handle[0].Bytes;
            }

            if (m_L2CAP_Svc_Handle[1] != null && m_L2CAP_Svc_Handle[1].Equals(lsb, msb))
            {
                return m_L2CAP_Svc_Handle[0].Bytes;
            }

            throw new Exception("L2CAP SCID Not Found");
        }

        /// <summary>
        ///     Source Channel Identifier.
        /// </summary>
        /// <remarks>Used in the L2CAP layer to indicate the channel endpoint on the device sending the L2CAP message. It is a device local name only.</remarks>
        /// <param name="connectionType"></param>
        /// <returns>Source Channel Identifier.</returns>
        public virtual byte[] Get_SCID(L2CAP.PSM connectionType)
        {
            switch (connectionType)
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
                LocalMac[5],
                LocalMac[4],
                LocalMac[3],
                LocalMac[2],
                LocalMac[1],
                LocalMac[0],
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