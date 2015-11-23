using PropertyChanged;
using ScpControl.Profiler;

namespace ScpProfiler
{
    [ImplementPropertyChanged]
    public class DualShockProfileViewModel
    {
        public DualShockProfileViewModel()
        {
            CurrentProfile = new DualShockProfile();
        }

        public DualShockProfile CurrentProfile { get; set; }
    }
}
