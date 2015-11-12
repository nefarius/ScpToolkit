using System.Collections.Generic;
using System.IO;
using CSScriptLibrary;
using ScpControl.Profiler;
using ScpControl.ScpCore;

namespace ScpControl.Plugins
{
    public class ScpPlugins : SingletonBase<ScpPlugins>
    {
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

        private IList<IScpMapperProfile> MapperProfiles { get; set; }

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