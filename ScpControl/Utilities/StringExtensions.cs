using System.Collections.Generic;
using System.Text;

namespace ScpControl.Utilities
{
    public static class StringExtensions
    {
        public static IEnumerable<byte> ToBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string ToUtf8(this byte[] input)
        {
            return input == null ? string.Empty : Encoding.UTF8.GetString(input);
        }
    }
}
