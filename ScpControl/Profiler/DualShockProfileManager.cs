using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl.Profiler
{
    public class DualShockProfileManager : SingletonBase<DualShockProfileManager>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DualShockProfileManager()
        {
            var profiles = new List<DualShockProfile>();

            foreach (var file in Directory.GetFiles(Path.Combine(GlobalConfiguration.AppDirectory, "Profiles"), "*.xml")
                )
            {
                Log.InfoFormat("Loading profile from file {0}", file);

                var profile = DualShockProfile.Load(file);
                profiles.Add(profile);

                Log.InfoFormat("Successfully loaded profile {0}", profile.Name);
            }

            Profiles = profiles.AsReadOnly();
        }

        /// <summary>
        ///     Gets a list of all available profiles loaded from memory.
        /// </summary>
        public IReadOnlyList<DualShockProfile> Profiles { get; private set; }

        /// <summary>
        ///     Feeds the supplied HID report through all loaded mapping profiles.
        /// </summary>
        /// <param name="report">The extended HID report.</param>
        public void PassThroughAllProfiles(ScpHidReport report)
        {
            foreach (var profile in Profiles)
            {
                profile.Remap(report);
            }
        }
    }
}