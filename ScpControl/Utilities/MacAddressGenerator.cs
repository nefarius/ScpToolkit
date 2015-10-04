using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                for (int i = 0; i < 6; i++)
                {
                    var number = r.Next(0, 255);
                    var b = Convert.ToByte(number);
                    if (i == 0)
                    {
                        b = SetBit(b, 6); //--> set locally administered
                        b = UnsetBit(b, 7); // --> set unicast 
                    }
                    sBuilder.Append(string.Format("{0}:", number.ToString("X2")));
                }

                return sBuilder.ToString().ToUpper().TrimEnd(':');
            }
        }

        private static byte SetBit(byte b, int bitNumber)
        {
            if (bitNumber < 8 && bitNumber > -1)
            {
                return (byte)(b | (byte)(0x01 << bitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + bitNumber + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        private static byte UnsetBit(byte b, int bitNumber)
        {
            if (bitNumber < 8 && bitNumber > -1)
            {
                return (byte)(b | (byte)(0x00 << bitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + bitNumber + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }
    }
}
