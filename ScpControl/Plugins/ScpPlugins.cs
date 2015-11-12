using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSScriptLibrary;
using log4net;
using ScpControl.Profiler;
using ScpControl.ScpCore;

namespace ScpControl.Plugins
{
    public class ScpPlugins : SingletonBase<ScpPlugins>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ScpPlugins()
        {
            #region Profile scripts

            Log.Debug("Initializing profile mapping scripts");

            MapperProfiles = new List<IScpMapperProfile>();

            var mapperScriptsDir = Path.Combine(GlobalConfiguration.AppDirectory, @"Plugins\Mapper");

            if (!Directory.Exists(mapperScriptsDir)) return;

            foreach (var file in Directory.GetFiles(mapperScriptsDir, "*.script.cs"))
            {
                try
                {
                    var plugin =
                        CSScript.Evaluator.LoadFile<IScpMapperProfile>(file).TryAlignToInterface<IScpMapperProfile>();

                    MapperProfiles.Add(plugin);

                    Log.InfoFormat("Successfully loaded profile plugin {0}", plugin.Name);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Couldn't load plugin script \"{0}\": {1}", file, ex);
                }
            }

            #endregion
        }

        private IList<IScpMapperProfile> MapperProfiles { get; set; }

        public void Process(ScpHidReport report)
        {
            foreach (var mapperProfile in MapperProfiles.Where(p => p.IsActive))
            {
                mapperProfile.Process(report);
            }
        }
    }
}