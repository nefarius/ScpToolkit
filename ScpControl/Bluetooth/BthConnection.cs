using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Reflection;
using log4net;
using ScpControl.Shared.Core;
using ScpControl.Utilities;

namespace ScpControl.Bluetooth
{
    public partial class BthConnection : Component, IEquatable<BthConnection>, IComparable<BthConnection>
    {
        #region IComparable<ScpBthConnection> Members

        public virtual int CompareTo(BthConnection other)
        {
            return HciHandle.CompareTo(other.HciHandle);
        }

        #endregion

        #region Public methods

        public virtual byte[] SetConnectionType(L2CAP.PSM connectionType, byte lsb, byte msb, ushort dcid = 0)
        {
            switch (connectionType)
            {
                case L2CAP.PSM.HID_Command:

                    _l2CapCommandHandle[0] = new BthHandle(lsb, msb);
                    _l2CapCommandHandle[1] = new BthHandle(Dcid++);

                    return _l2CapCommandHandle[1].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    CanStartService = true;

                    _l2CapInterruptHandle[0] = new BthHandle(lsb, msb);
                    _l2CapInterruptHandle[1] = new BthHandle(Dcid++);

                    return _l2CapInterruptHandle[1].Bytes;

                case L2CAP.PSM.HID_Service:

                    IsServiceStarted = true;
                    CanStartService = false;

                    _l2CapServiceHandle[0] = new BthHandle(lsb, msb);
                    _l2CapServiceHandle[1] = new BthHandle(dcid);

                    return _l2CapServiceHandle[1].Bytes;
            }

            throw new ArgumentException("Invalid L2CAP Connection Type");
        }

        public virtual byte[] SetConnectionType(L2CAP.PSM connectionType, byte[] handle)
        {
            return SetConnectionType(connectionType, handle[0], handle[1]);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", DeviceAddress.AsFriendlyName(), _remoteName);
        }

        #endregion

        #region Private/protected fields

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static ushort _dcid = 0x40;
        private readonly BthHandle[] _l2CapCommandHandle = new BthHandle[2];
        private readonly BthHandle[] _l2CapInterruptHandle = new BthHandle[2];
        private readonly BthHandle[] _l2CapServiceHandle = new BthHandle[2];
        private DsModel _model = DsModel.DS3;
        private string _remoteName = string.Empty;

        #endregion

        #region Ctors

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

            HciHandle = hciHandle;
            DeviceAddress = PhysicalAddress.None;
        }

        #endregion

        #region Public properties

        public BthHandle HciHandle { get; private set; }

        public PhysicalAddress DeviceAddress { get; set; }

        public virtual string RemoteName
        {
            get { return _remoteName; }
            set
            {
                _remoteName = value;
                if (!string.IsNullOrEmpty(_remoteName) && _remoteName.Equals(BthDs4.GenuineProductName))
                    _model = DsModel.DS4;
            }
        }

        public bool CanStartHid { get; set; }
        public bool CanStartService { get; set; }
        public bool IsServiceStarted { get; set; }
        public bool IsStarted { get; set; }
        public bool IsFake { get; set; }

        public virtual DsModel Model
        {
            get { return _model; }
        }

        /// <summary>
        ///     Gets or sets the global Destination Channel Identifier.
        /// </summary>
        public static ushort Dcid
        {
            get { return _dcid; }
            set { _dcid = (ushort) (value < 0xFFFF ? value : 0x40); }
        }

        #endregion

        #region Channel identifier helper methods

        /// <summary>
        ///     Destination Channel Identifier.
        /// </summary>
        /// <remarks>
        ///     Used as the device local end point for an L2CAP transmission. It represents the channel endpoint on the device
        ///     receiving the message. It is a device local name only. See also SCID.
        /// </remarks>
        /// <param name="lsb"></param>
        /// <param name="msb"></param>
        /// <returns></returns>
        public virtual byte[] Get_DCID(byte lsb, byte msb)
        {
            if (_l2CapCommandHandle[0].Equals(lsb, msb))
            {
                return _l2CapCommandHandle[1].Bytes;
            }

            if (_l2CapInterruptHandle[0].Equals(lsb, msb))
            {
                return _l2CapInterruptHandle[1].Bytes;
            }

            if (_l2CapServiceHandle[0].Equals(lsb, msb))
            {
                return _l2CapServiceHandle[1].Bytes;
            }

            throw new Exception("L2CAP DCID Not Found");
        }

        /// <summary>
        ///     Destination Channel Identifier.
        /// </summary>
        /// <remarks>
        ///     Used as the device local end point for an L2CAP transmission. It represents the channel endpoint on the device
        ///     receiving the message. It is a device local name only. See also SCID.
        /// </remarks>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        public virtual byte[] Get_DCID(L2CAP.PSM connectionType)
        {
            switch (connectionType)
            {
                case L2CAP.PSM.HID_Command:

                    return _l2CapCommandHandle[1].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    return _l2CapInterruptHandle[1].Bytes;

                case L2CAP.PSM.HID_Service:

                    return _l2CapServiceHandle[1].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }

        /// <summary>
        ///     Source Channel Identifier.
        /// </summary>
        /// <remarks>
        ///     Used in the L2CAP layer to indicate the channel endpoint on the device sending the L2CAP message. It is a
        ///     device local name only.
        /// </remarks>
        /// <param name="lsb">Least significant byte.</param>
        /// <param name="msb">Most significant byte.</param>
        /// <returns></returns>
        public byte[] Get_SCID(byte lsb, byte msb)
        {
            if (_l2CapCommandHandle[1] != null && _l2CapCommandHandle[1].Equals(lsb, msb))
            {
                return _l2CapCommandHandle[0].Bytes;
            }

            if (_l2CapInterruptHandle[1] != null && _l2CapInterruptHandle[1].Equals(lsb, msb))
            {
                return _l2CapInterruptHandle[0].Bytes;
            }

            if (_l2CapServiceHandle[1] != null && _l2CapServiceHandle[1].Equals(lsb, msb))
            {
                return _l2CapServiceHandle[0].Bytes;
            }

            throw new Exception("L2CAP SCID Not Found");
        }

        public byte[] Get_SCID(byte[] buffer)
        {
            return Get_SCID(buffer[0], buffer[1]);
        }

        /// <summary>
        ///     Source Channel Identifier.
        /// </summary>
        /// <remarks>
        ///     Used in the L2CAP layer to indicate the channel endpoint on the device sending the L2CAP message. It is a
        ///     device local name only.
        /// </remarks>
        /// <param name="connectionType"></param>
        /// <returns>Source Channel Identifier.</returns>
        public byte[] Get_SCID(L2CAP.PSM connectionType)
        {
            switch (connectionType)
            {
                case L2CAP.PSM.HID_Command:

                    return _l2CapCommandHandle[0].Bytes;

                case L2CAP.PSM.HID_Interrupt:

                    return _l2CapInterruptHandle[0].Bytes;

                case L2CAP.PSM.HID_Service:

                    return _l2CapServiceHandle[0].Bytes;
            }

            throw new Exception("Invalid L2CAP Connection Type");
        }

        #endregion

        #region IEquatable<ScpBthConnection> Members

        public virtual bool Equals(BthConnection other)
        {
            return HciHandle.Equals(other.HciHandle);
        }

        public virtual bool Equals(byte lsb, byte msb)
        {
            return HciHandle.Equals(lsb, msb);
        }

        public virtual bool Equals(byte[] other)
        {
            return HciHandle.Equals(other);
        }

        #endregion
    }
}
