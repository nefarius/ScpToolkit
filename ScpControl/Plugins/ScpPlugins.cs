using System;
using System.Collections.Generic;
using System.IO;
using CSScriptLibrary;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using Ds3Button = ScpControl.Profiler.Ds3Button;

namespace ScpControl.Plugins
{
    public class ScpPlugins : SingletonBase<ScpPlugins>
    {
        public IList<IScpMapperProfile> MapperProfiles { get; private set; }

        private DateTime _lastTimestamp = DateTime.Now;

        private ScpPlugins()
        {
            MapperProfiles = new List<IScpMapperProfile>();

            var mapperScriptsDir = Path.Combine(GlobalConfiguration.AppDirectory, @"Plugins\Mapper");

            foreach (var file in Directory.GetFiles(mapperScriptsDir, "*.script.cs"))
            {
                MapperProfiles.Add(
                    CSScript.Evaluator.LoadFile<IScpMapperProfile>(file).TryAlignToInterface<IScpMapperProfile>());
            }
        }

        public void Process(ScpHidReport report)
        {
            foreach (var mapperProfile in MapperProfiles)
            {
                mapperProfile.Process(report);
            }
        }
    }

    public interface IScpMapperProfile
    {
        void Process(ScpHidReport report);
    }
}
