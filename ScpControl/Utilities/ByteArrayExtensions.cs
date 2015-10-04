using System.Linq;
using System.Text;

namespace ScpControl.Utilities
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] array)
        {
            return ToHexString(array, array.Length);
        }

        public static string ToHexString(this byte[] array, int length)
        {
            var sb = new StringBuilder();

            foreach (var b in array.Take(length))
            {
                sb.AppendFormat("0x{0:X2} ", b);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
