using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace ScpDriver.Utilities
{
    public enum OsType
    {
        Invalid,
        Xp,
        Vista,
        Win7,
        Win8,
        Win81,
        Default
    };

    public static class OsInfoHelper
    {
        public static string OsInfo()
        {
            var info = string.Empty;

            using (var mos = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
            {
                foreach (var mo in mos.Get().Cast<ManagementObject>())
                {
                    info = Regex.Replace(mo.GetPropertyValue("Caption").ToString(), "[^A-Za-z0-9 ]", "").Trim();

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

            return info;
        }

        public static OsType OsParse(string info)
        {
            var Valid = OsType.Invalid;

            var architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToUpper().Trim();

            if (Environment.Is64BitOperatingSystem == Environment.Is64BitProcess &&
                (architecture == "X86" || architecture == "AMD64"))
            {
                Valid = OsType.Default;

                if (!string.IsNullOrEmpty(info))
                {
                    var Token = info.Split(' ');

                    if (Token[0].ToUpper().Trim() == "MICROSOFT" && Token[1].ToUpper().Trim() == "WINDOWS")
                    {
                        switch (Token[2].ToUpper().Trim())
                        {
                            case "XP":

                                if (!Environment.Is64BitOperatingSystem) Valid = OsType.Xp;
                                break;

                            case "VISTA":

                                Valid = OsType.Vista;
                                break;

                            case "7":

                                Valid = OsType.Win7;
                                break;

                            case "8":

                                Valid = OsType.Win8;
                                break;

                            case "81":

                                Valid = OsType.Win81;
                                break;

                            case "SERVER":

                                switch (Token[3].ToUpper().Trim())
                                {
                                    case "2008":

                                        if (Token[4].ToUpper().Trim() == "R2")
                                        {
                                            Valid = OsType.Win7;
                                        }
                                        else
                                        {
                                            Valid = OsType.Vista;
                                        }
                                        break;

                                    case "2012":

                                        Valid = OsType.Win8;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            return Valid;
        }
    }
}