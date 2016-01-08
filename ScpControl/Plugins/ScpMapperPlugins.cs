using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CSScriptLibrary;
using log4net;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;

namespace ScpControl.Plugins
{
    public class ScpMapperPlugins : SingletonBase<ScpMapperPlugins>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IList<IScpMapperProfile> MapperProfiles = new List<IScpMapperProfile>();

        private ScpMapperPlugins()
        {
            #region Profile scripts

            Log.Debug("Initializing profile mapping scripts");

            var mapperScriptsDir = Path.Combine(GlobalConfiguration.AppDirectory, @"Plugins\Mapper");

            if (!Directory.Exists(mapperScriptsDir)) return;

            foreach (var file in Directory.GetFiles(mapperScriptsDir, "*.script.cs"))
            {
                try
                {
                    var plugin =
                        CSScript.Evaluator.LoadFile<IScpMapperProfile>(file).TryAlignToInterface<IScpMapperProfile>();

                    if (!plugin.IsActive) continue;

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

        public void Process(ScpHidReport report)
        {
            try
            {
                foreach (var mapperProfile in MapperProfiles)
                {
                    mapperProfile.Process(report);
                }
            }
            catch // TODO: remove!
            { }
        }
    }
}
