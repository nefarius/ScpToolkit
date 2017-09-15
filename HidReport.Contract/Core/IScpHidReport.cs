using HidReport.Contract.DsActors;
using HidReport.Contract.Enums;

namespace HidReport.Contract.Core
{
    public interface IScpHidReport
    {
        DsBattery BatteryStatus { get; }
        bool IsPadActive { get; }
        uint PacketCounter { get; }
        byte ReportId { get; }
        IDsAccelerometerImmutable Motion { get; }
        IDsGyroscopeImmutable Orientation { get; }
        IDsTrackPadTouchImmutable TrackPadTouch0 { get; }
        IDsTrackPadTouchImmutable TrackPadTouch1 { get; }
        IDsButtonState this[ButtonsEnum button] { get; }
        IDsAxisStateImmutable this[AxesEnum axis] { get; }

    }
}