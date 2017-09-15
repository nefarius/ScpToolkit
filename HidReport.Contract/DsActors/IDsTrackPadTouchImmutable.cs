namespace HidReport.Contract.DsActors
{
    public interface IDsTrackPadTouchImmutable
    {
        int Id { get; }
        bool IsActive { get; }
        int X { get; }
        int Y { get; }
    }
}