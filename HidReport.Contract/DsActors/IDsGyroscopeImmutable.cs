namespace HidReport.Contract.DsActors
{
    public interface IDsGyroscopeImmutable
    {
        short Pitch { get; }
        short Roll { get; }
        short Yaw { get; }
    }
}