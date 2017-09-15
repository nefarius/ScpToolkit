using System;
using HidReport.Contract.DsActors;

namespace HidReport.DsActors
{
    [Serializable]
    public class DsGyroscope : IDsGyroscopeImmutable
    {
        public short Roll { get; set; }
        public short Yaw { get; set; }
        public short Pitch { get; set; }
    }
}
