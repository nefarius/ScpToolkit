using ScpControl.Profiler;

namespace ScpControl.Plugins
{
    /// <summary>
    ///     Describes an object which can manipulate the gamepad data.
    /// </summary>
    public interface IScpMapperProfile
    {
        bool IsActive { get; }
        string Name { get; }
        string Description { get; }
        string Author { get; }

        void Process(ScpHidReport report);
    }
}
