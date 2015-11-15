namespace ScpControl.Profiler
{
    public class DsAccelerometer
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
    }

    public class DsGyroscope
    {
        public short Roll { get; set; }
        public short Yaw { get; set; }
        public short Pitch { get; set; }
    }
}
