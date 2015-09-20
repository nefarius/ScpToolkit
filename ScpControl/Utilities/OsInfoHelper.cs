using System;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Microsoft.Win32;

namespace ScpControl.Utilities
{
    public enum OsType
    {
        Invalid,
        Xp,
        Vista,
        Win7,
        Win8,
        Win81,
        Win10,
        Default
    };

    /// <summary>
    ///     Utility class to query current operating system information.
    /// </summary>
    public static class OsInfoHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsVc2013Installed
        {
            get
            {
                return
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\vc\Servicing\12.0\RuntimeMinimum") !=
                    null;
            }
        }

        public static bool IsVc2010Installed
        {
            get
            {
                return
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\vc\Servicing\10.0\red") !=
                    null;
            }
        }

        public static string OsInfo
        {
            get
            {
                var info = string.Empty;

                try
                {
                    using (var mos = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
                    {
                        foreach (var mo in mos.Get().Cast<ManagementObject>())
                        {
                            info =
                                Regex.Replace(mo.GetPropertyValue("Caption").ToString(), @"[^A-Za-z0-9 \.]", "").Trim();

                            var spv = mo.GetPropertyValue("ServicePackMajorVersion");

                            if (spv != null && spv.ToString() != "0")
                            {
                                info += " Service Pack " + spv;
                            }

                            info = string.Format("{0} ({1} {2})", info, Environment.OSVersion.Version,
                                Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));

                            mo.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Couldn't query operating system information: {0}", ex);
                }

                return info;
            }
        }

        public static OsType OsParse(string info)
        {
            var valid = OsType.Invalid;

            var architecture =
                (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "UNKNOWN").ToUpper().Trim();

            if (Environment.Is64BitOperatingSystem == Environment.Is64BitProcess &&
                (architecture == "X86" || architecture == "AMD64"))
            {
                valid = OsType.Default;

                if (string.IsNullOrEmpty(info)) return valid;

                var token = info.Split(' ');

                if (token[0].ToUpper().Trim() != "MICROSOFT" || token[1].ToUpper().Trim() != "WINDOWS") return valid;

                switch (token[2].ToUpper().Trim())
                {
                    case "XP":

                        if (!Environment.Is64BitOperatingSystem) valid = OsType.Xp;
                        break;

                    case "VISTA":

                        valid = OsType.Vista;
                        break;

                    case "7":

                        valid = OsType.Win7;
                        break;

                    case "8":

                        valid = OsType.Win8;
                        break;

                    case "8.1":

                        valid = OsType.Win81;
                        break;

                    case "10":

                        valid = OsType.Win10;
                        break;

                    case "SERVER":

                        switch (token[3].ToUpper().Trim())
                        {
                            case "2008":

                                valid = token[4].ToUpper().Trim() == "R2" ? OsType.Win7 : OsType.Vista;
                                break;

                            case "2012":

                                valid = OsType.Win8;
                                break;
                        }
                        break;
                }
            }

            return valid;
        }
    }
}