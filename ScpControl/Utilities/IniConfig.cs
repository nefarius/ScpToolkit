using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IniParser;
using log4net;

namespace ScpControl.Utilities
{
    public class IniConfig
    {
        private const string CfgFile = "ScpControl.ini";
        private static readonly Lazy<IniConfig> LazyIinstance = new Lazy<IniConfig>(() => new IniConfig());
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IniConfig()
        {
            var parser = new FileIniDataParser();
            var fullPath = Path.Combine(WorkingDirectory, CfgFile);

            if (!File.Exists(fullPath))
            {
                Log.FatalFormat("Configuration file {0} not found!", fullPath);
                return;
            }

            var data = parser.ReadFile(fullPath);

            BthDongle = new BthDongleCfg
            {
                SupportedNames =
                    data["BthDongle"].Where(k => k.KeyName.Equals("SupportedName")).Select(v => v.Value),
                SupportedMacs = data["BthDongle"].Where(k => k.KeyName.Equals("SupportedMac")).Select(v => v.Value)
            };

            BthDs3 = new BthDs3Cfg
            {
                SupportedNames =
                    data["BthDs3"].Where(k => k.KeyName.Equals("SupportedName")).Select(v => v.Value),
                SupportedMacs = data["BthDs3"].Where(k => k.KeyName.Equals("SupportedMac")).Select(v => v.Value)
            };
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