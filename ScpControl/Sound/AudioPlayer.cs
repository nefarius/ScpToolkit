using System;
using System.IO;
using System.Reflection;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl.Sound
{
    public class AudioPlayer
    {
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly Lazy<AudioPlayer> LazyInstance = new Lazy<AudioPlayer>(() => new AudioPlayer());
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly dynamic _soundEngine;

        /// <summary>
        ///     Initializes the irrKlang engine.
        /// </summary>
        private AudioPlayer()
        {
            // build path depending on process architecture
            var irrKlangPath = Path.Combine(WorkingDirectory,
                (Environment.Is64BitProcess)
                    ? @"irrKlang\amd64\irrKlang.NET4.dll"
                    : @"irrKlang\x86\irrKlang.NET4.dll");
            Log.DebugFormat("Loading irrKlang engine from {0}", irrKlangPath);

            // load assembly
            var irrKlangAssembly = Assembly.LoadFile(irrKlangPath);

            // get type of ISoundEngine class
            var soundEngineType = irrKlangAssembly.GetType("IrrKlang.ISoundEngine");

            // instantiate  ISoundEngine
            _soundEngine = Activator.CreateInstance(soundEngineType);
        }

        public static AudioPlayer Instance
        {
            get { return LazyInstance.Value; }
        }

        // TODO: remove
        public void PlayMediaFile(string filename)
        {
            PlayCustomFile(Path.Combine(WorkingDirectory, @"Media", filename));
        }

        public void PlayCustomFile(string path)
        {
            if (!GlobalConfiguration.Instance.SoundsEnabled || !File.Exists(path))
                return;

            _soundEngine.Play2D(path);
        }
    }
}