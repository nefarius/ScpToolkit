using System.Collections.Generic;
using ScpControl.Shared.Core;

namespace ScpProfiler
{
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
