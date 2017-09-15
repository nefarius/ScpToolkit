namespace HidReport.Contract.DsActors
{
    /// <summary>
    ///     Defines a DualShock axis state.
    /// </summary>
    public interface IDsAxisStateImmutable
    {
        /// <summary>
        ///     The current value of the axis in question.
        /// </summary>
        byte Value { get; }
        /// <summary>
        ///     True if the current value differs from the default value of the axis, false otherwise.
        /// </summary>
        bool IsEngaged { get; }
        /// <summary>
        ///     Gets the pressure value of the current button compatible with PCSX2s XInput/LilyPad mod.
        /// </summary>
        //TODO: to mapper float Pressure { get; }
        float Axis { get; }
    }
}