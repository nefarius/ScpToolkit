using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var iniOpts = new IniOptions()
            {
                CommentStarter = IniCommentStarter.Semicolon,
                KeyDuplicate = IniDuplication.Allowed
            };

            var ini = new IniFile(iniOpts);
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

                BthDongle = new BthDongleCfg()
                {
                    SupportedNames =
                        ini.Sections["BthDongle"].Keys.Where(k => k.Name == "SupportedName").Select(v => v.Value),
                    SupportedMacs = ini.Sections["BthDongle"].Keys.Where(k => k.Name == "SupportedMac").Select(v => v.Value)
                };

                BthDs3 = new BthDs3Cfg()
                {
                    SupportedNames =
                        ini.Sections["BthDs3"].Keys.Where(k => k.Name == "SupportedName").Select(v => v.Value),
                    SupportedMacs = ini.Sections["BthDs3"].Keys.Where(k => k.Name == "SupportedMac").Select(v => v.Value)
                };
                
                Hci = new HciCfg()
                {
                    SupportedNames = ini.Sections["HCI"].Keys.Where(k => k.Name == "SupportedName").Select(v => v.Value)
                };
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
        public HciCfg Hci { get; private set; }

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

        public class HciCfg
        {
            public IEnumerable<string> SupportedNames { get; set; }
        }
    }
}