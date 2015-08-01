using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ScpControl.Utilities
{
    public static class StringExtensions
    {
        public static IEnumerable<byte> ToBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string ToUnicode(this byte[] input)
        {
            return Encoding.Unicode.GetString(input);
        }

        public static string ToUtf8(this byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }
    }
}
