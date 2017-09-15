namespace HidReport.Contract.DsActors
{
    /// <summary>
    ///     Describes the possible states for a DualShock button.
    /// </summary>
    public interface IDsButtonState
    {
        bool IsPressed { get; set; }
        float Pressure { get; }
        byte Value { get; }
    }
}