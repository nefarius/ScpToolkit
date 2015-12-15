using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Threading;
using System.Xml;
using log4net;
using Libarius.Filesystem;
using PropertyChanged;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;

namespace ScpControl.Profiler
{
    [ImplementPropertyChanged]
    public class DualShockProfileManager : SingletonBase<DualShockProfileManager>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FileSystemWatcher _fswProfileFiles = new FileSystemWatcher(ProfilesPath, ProfileFileFilter);

        private DualShockProfileManager()
        {
            LoadProfiles();

            _fswProfileFiles.Changed += FswProfileFilesOnChanged;
            _fswProfileFiles.Created += FswProfileFilesOnChanged;
            _fswProfileFiles.Renamed += FswProfileFilesOnChanged;
            _fswProfileFiles.Deleted += FswProfileFilesOnChanged;

            _fswProfileFiles.Error +=
                (sender, args) =>
                {
                    Log.ErrorFormat("Unexpected error in Profiles FileSystemWatcher: {0}", args.GetException());
                };

            _fswProfileFiles.EnableRaisingEvents = true;
        }

        public static string ProfileFileFilter
        {
            get { return "*.xml"; }
        }

        public static string ProfilesPath
        {
            get
            {
                var profiles = Path.Combine(GlobalConfiguration.AppDirectory, "Profiles");

                if (!Directory.Exists(profiles))
                    Directory.CreateDirectory(profiles);

                return profiles;
            }
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
                var profiles = new List<DualShockProfile>();

                foreach (var file in Directory.GetFiles(ProfilesPath, ProfileFileFilter)
                    )
                {
                    Log.InfoFormat("Loading profile from file {0}", file);

                    try
                    {
                        var profile = Load(file);
                        profiles.Add(profile);

                        Log.InfoFormat("Successfully loaded profile {0}", profile.Name);
                    }
                    catch (SerializationException ex)
                    {
                        Log.ErrorFormat("Couldn't load profile from file {0}, maybe it's damaged or outdated", file);
                        Log.DebugFormat("Profile load error: {0}", ex);
                    }
                }

                Profiles = profiles.AsReadOnly();
            }
        }

        private void FswProfileFilesOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (fileSystemEventArgs.ChangeType != WatcherChangeTypes.Deleted)
            {
                // file might still be written to, just wait until it's handles are closed
                while (FilesystemHelper.IsFileLocked(new FileInfo(fileSystemEventArgs.FullPath)))
                {
                    Thread.Sleep(100);
                }
            }

            LoadProfiles();
        }

        /// <summary>
        ///     Feeds the supplied HID report through all loaded mapping profiles.
        /// </summary>
        /// <param name="report">The extended HID report.</param>
        public void PassThroughAllProfiles(ScpHidReport report)
        {
            foreach (var profile in Profiles.Where(p => p.IsActive))
            {
                profile.Remap(report);
            }
        }

        /// <summary>
        ///     Stores a new <see cref="DualShockProfile" /> or overwrites an existing one.
        /// </summary>
        /// <param name="profile">The <see cref="DualShockProfile" /> to save.</param>
        public void SubmitProfile(DualShockProfile profile)
        {
            Save(profile, Path.Combine(GlobalConfiguration.ProfilesPath, profile.FileName));
        }

        /// <summary>
        ///     Removes a given <see cref="DualShockProfile" />.
        /// </summary>
        /// <param name="profile">The <see cref="DualShockProfile" />to remove.</param>
        public void RemoveProfile(DualShockProfile profile)
        {
            File.Delete(Path.Combine(GlobalConfiguration.ProfilesPath, profile.FileName));
        }

        /// <summary>
        ///     Loads a <see cref="DualShockProfile"/> from a file.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns>The deserialized <see cref="DualShockProfile"/>.</returns>
        private static DualShockProfile Load(string file)
        {
            var serializer = new DataContractSerializer(typeof (DualShockProfile));

            using (var fs = File.OpenText(file))
            {
                using (var xml = XmlReader.Create(fs))
                {
                    return (DualShockProfile) serializer.ReadObject(xml);
                }
            }
        }

        /// <summary>
        ///     Saves a <see cref="DualShockProfile"/> as a file.
        /// </summary>
        /// <param name="profile">The <see cref="DualShockProfile"/> to save.</param>
        /// <param name="file">The file name to save the <see cref="DualShockProfile"/> to.</param>
        private static void Save(DualShockProfile profile, string file)
        {
            var serializer = new DataContractSerializer(profile.GetType());

            var path = Path.GetDirectoryName(file) ?? GlobalConfiguration.AppDirectory;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var xml = XmlWriter.Create(file, new XmlWriterSettings {Indent = true}))
            {
                serializer.WriteObject(xml, profile);
            }
        }
    }
}