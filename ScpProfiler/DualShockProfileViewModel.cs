using System.Collections.Generic;
using PropertyChanged;
using ScpControl.Shared.Core;

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

        public IReadOnlyList<DualShockProfile> Profiles { get; set; }
    }
}
