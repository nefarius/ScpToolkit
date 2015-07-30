using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using MadMilkman.Ini;

namespace ScpControl.Utilities
{
    public class IniConfig
    {
        private const string CfgFile = "ScpControl.ini";
        private static readonly Lazy<IniConfig> LazyIinstance = new Lazy<IniConfig>(() => new IniConfig());
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Interprets the ScpControl.ini configuration file.
        /// </summary>
        private IniConfig()
        {
            var ini = new IniFile();
            var fullPath = Path.Combine(WorkingDirectory, CfgFile);

            if (!File.Exists(fullPath))
            {
                Log.FatalFormat("Configuration file {0} not found!", fullPath);
                return;
            }

            // parse data from INI
            try
            {
                ini.Load(fullPath);

                string[] values;

                BthDongle = new BthDongleCfg();
                {
                    ini.Sections["BthDongle"].Keys["SupportedNames"].TryParseValue(out values);
                    BthDongle.SupportedNames = values;

                    ini.Sections["BthDongle"].Keys["SupportedMacs"].TryParseValue(out values);
                    BthDongle.SupportedMacs = values;
                }

                BthDs3 = new BthDs3Cfg();
                {
                    ini.Sections["BthDs3"].Keys["SupportedNames"].TryParseValue(out values);
                    BthDs3.SupportedNames = values;

                    ini.Sections["BthDs3"].Keys["SupportedMacs"].TryParseValue(out values);
                    BthDs3.SupportedMacs = values;
                }
            }
            catch (Exception ex)
            {
                Log.FatalFormat("Error while parsing configuration file: {0}", ex);
            }
        }

        public static IniConfig Instance
        {
            get { return LazyIinstance.Value; }
        }

        public BthDongleCfg BthDongle { get; private set; }
        public BthDs3Cfg BthDs3 { get; private set; }

        public class BthDongleCfg
        {
            public IEnumerable<string> SupportedMacs { get; set; }
            public IEnumerable<string> SupportedNames { get; set; }
        }

        public class BthDs3Cfg
        {
            public IEnumerable<string> SupportedMacs { get; set; }
            public IEnumerable<string> SupportedNames { get; set; }
        }
    }
}