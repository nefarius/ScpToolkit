using System;
using System.Text;

namespace ScpControl.Utilities
{
    /// <summary>
    ///     Utility class to generate random MAC addresses.
    /// </summary>
    /// <remarks>http://stackoverflow.com/questions/10161291/generate-a-random-mac-address</remarks>
    public static class MacAddressGenerator
    {
        public static string NewMacAddress
        {
            get
            {
                var sBuilder = new StringBuilder();
                var r = new Random();

                for (var i = 0; i < 6; i++)
                {
                    var number = r.Next(0, 255);
                    var b = Convert.ToByte(number);
                    if (i == 0)
                    {
                        b = (byte) ((b & 0xFE) | 0x02); //-->set locally administered and unicast
                    }
                    sBuilder.Append(string.Format("{0}", number.ToString("X2")));
                }

                return sBuilder.ToString();
            }
        }
    }
}
