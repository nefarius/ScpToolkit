using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using MadMilkman.Ini;
using ScpControl.ScpCore;

namespace ScpControl.Utilities
{
    public class IniConfig
    {
        private const string CfgFile = "ScpControl.ini";
        private static readonly Lazy<IniConfig> LazyIinstance = new Lazy<IniConfig>(() => new IniConfig());
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
            var fullPath = Path.Combine(GlobalConfiguration.AppDirectory, CfgFile);

            if (!File.Exists(fullPath))
            {
                Log.FatalFormat("Configuration file {0} not found!", fullPath);
                return;
            }

            // parse data from INI
            try
            {
                ini.Load(fullPath);
                
                Hci = new HciCfg()
                {
                    SupportedNames = ini.Sections["Host Controller Interface"].Keys.Where(k => k.Name == "SupportedName").Select(v => v.Value),
                    GenuineMacAddresses = ini.Sections["Host Controller Interface"].Keys.Where(k => k.Name == "GenuineMacAddress").Select(v => v.Value)
                };

                Ds3Driver = new Ds3DriverCfg()
                {
                    DeviceGuid = ini.Sections["DualShock 3 Controllers"].Keys["DeviceGuid"].Value,
                    HardwareIds = ini.Sections["DualShock 3 Controllers"].Keys.Where(k => k.Name == "HardwareId").Select(v => v.Value)
                };

                Ds4Driver = new Ds4DriverCfg()
                {
                    DeviceGuid = ini.Sections["DualShock 4 Controllers"].Keys["DeviceGuid"].Value,
                    HardwareIds = ini.Sections["DualShock 4 Controllers"].Keys.Where(k => k.Name == "HardwareId").Select(v => v.Value)
                };

                BthDongleDriver = new BthDongleDriverCfg()
                {
                    DeviceGuid = ini.Sections["Bluetooth Dongles"].Keys["DeviceGuid"].Value,
                    HardwareIds = ini.Sections["Bluetooth Dongles"].Keys.Where(k => k.Name == "HardwareId").Select(v => v.Value)
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

        public HciCfg Hci { get; private set; }
        public Ds3DriverCfg Ds3Driver { get; private set; }
        public Ds4DriverCfg Ds4Driver { get; private set; }
        public BthDongleDriverCfg BthDongleDriver { get; private set; }

        public class HciCfg
        {
            public IEnumerable<string> SupportedNames { get; set; }
            public IEnumerable<string> GenuineMacAddresses { get; set; }
        }

        public class Ds3DriverCfg
        {
            public string DeviceGuid { get; set; }
            public IEnumerable<string> HardwareIds { get; set; }
        }

        public class Ds4DriverCfg
        {
            public string DeviceGuid { get; set; }
            public IEnumerable<string> HardwareIds { get; set; }
        }

        public class BthDongleDriverCfg
        {
            public string DeviceGuid { get; set; }
            public IEnumerable<string> HardwareIds { get; set; }
        }
    }
}