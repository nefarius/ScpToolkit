using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using HidReport.Contract.Core;

namespace ScpControl.Shared.Core
{
    /// <summary>
    ///     Represents an extended HID Input Report ready to be sent to the virtual bus device.
    /// </summary>
    /// 
    [Serializable]
    public class ScpHidReport : EventArgs
    {
        #region Ctors
        public ScpHidReport(DsConnection connectionType, PhysicalAddress padMacAddress, DsModel model, DsPadId padId, DsState padState, HidReport.Core.HidReport hidReport)
        {
            ConnectionType = connectionType;
            _padMacAddress = padMacAddress.GetAddressBytes();
            Model = model;
            PadId = padId;
            PadState = padState;
            _hidReport = hidReport;
        }

        #endregion

        private readonly HidReport.Core.HidReport _hidReport;
        private readonly byte[] _padMacAddress;

        #region Public properties

        public IScpHidReport HidReport => _hidReport;

        public PhysicalAddress PadMacAddress
        {
            get { return new PhysicalAddress(_padMacAddress); }
        }

        public DsModel Model { get; }

        public DsPadId PadId { get; }
 
        public DsState PadState { get; }

        public DsConnection ConnectionType { get; }
        #endregion
    }
}
