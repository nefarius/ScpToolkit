using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using ScpControl.Database;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;

namespace ScpControl.Profiler
{
    [ImplementPropertyChanged]
    public class DualShockProfileManager : SingletonBase<DualShockProfileManager>
    {
        private DualShockProfileManager()
        {
            LoadProfiles();
        }

        /// <summary>
        ///     Gets a list of all available profiles loaded from memory.
        /// </summary>
        public IReadOnlyList<DualShockProfile> Profiles { get; private set; }

        /// <summary>
        ///     Reads all XML files from Profiles directory.
        /// </summary>
        private void LoadProfiles()
        {
            lock (this)
            {
                try
                {
                    using (var db = new ScpDb())
                    {
                        Profiles =
                            db.Engine.GetAllDbEntities<DualShockProfile>(ScpDb.TableProfiles)
                                .Select(p => p.Value)
                                .ToList()
                                .AsReadOnly();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        ///     Feeds the supplied HID report through all loaded mapping profiles.
        /// </summary>
        /// <param name="report">The extended HID report.</param>
        public void PassThroughAllProfiles(ScpHidReport report)
        {
            try
            {
                foreach (var profile in Profiles.Where(p => p.IsActive))
                {
                    profile.Remap(report);
                }
            }
            catch // TODO: remove!
            {
            }
        }

        /// <summary>
        ///     Stores a new <see cref="DualShockProfile" /> or overwrites an existing one.
        /// </summary>
        /// <param name="profile">The <see cref="DualShockProfile" /> to save.</param>
        public void SubmitProfile(DualShockProfile profile)
        {
            lock (this)
            {
                using (var db = new ScpDb())
                {
                    db.Engine.PutDbEntity(ScpDb.TableProfiles, profile.Id.ToString(), profile);
                }
            }

            LoadProfiles();
        }

        /// <summary>
        ///     Removes a given <see cref="DualShockProfile" />.
        /// </summary>
        /// <param name="profile">The <see cref="DualShockProfile" />to remove.</param>
        public void RemoveProfile(DualShockProfile profile)
        {
            lock (this)
            {
                using (var db = new ScpDb())
                {
                    db.Engine.DeleteDbEntity(ScpDb.TableProfiles, profile.Id.ToString());
                }
            }

            LoadProfiles();
        }
    }
}
