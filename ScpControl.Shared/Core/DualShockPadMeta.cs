using System.Net.NetworkInformation;
using System.Runtime.Serialization;

namespace ScpControl.Shared.Core
{
    [DataContract]
    public class DualShockPadMeta
    {
        public DsPadId PadId { get; set; }
        public DsState PadState { get; set; }
        public DsConnection ConnectionType { get; set; }
        public DsModel Model { get; set; }
        public PhysicalAddress PadMacAddress { get; set; }
        public byte BatteryStatus { get; set; }
    }
}
