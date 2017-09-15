using System;
using HidReport.Contract.DsActors;

namespace HidReport.DsActors
{
    [Serializable]
    public class DsTrackPadTouch : IDsTrackPadTouchImmutable
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
