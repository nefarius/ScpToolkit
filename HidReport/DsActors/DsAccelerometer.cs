using System;
using HidReport.Contract.DsActors;

namespace HidReport.DsActors
{
    [Serializable]
    public class DsAccelerometer : IDsAccelerometerImmutable
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
    }
}