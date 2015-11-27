using System.Collections.Generic;
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

        public IReadOnlyList<DualShockProfile> Profiles
        {
            get { return DualShockProfileManager.Instance.Profiles; }
        }
    }
}
