using System;
using System.Reflection;
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
                    !(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist") ==
                    null && Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\VC\VCRedist") ==
                    null);
            }
        }

        public static string OsInfo
        {
            get
            {
                return string.Format("Microsoft {0} {1} ({2} {3})",
                    OsVersionInfo.Name, OsVersionInfo.Edition,
                    OsVersionInfo.Version, OsVersionInfo.OSBits);
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