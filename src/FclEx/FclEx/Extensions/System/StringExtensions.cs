using System;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Environment;

namespace FclEx.Extensions.System
{
    public static class StringExtensions
    {
        public static string FirstPart(this string str, char[] separators)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (separators.IsNullOrEmpty()) return str;
            return str.Split(separators, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static string RegexReplace(this string str, string rex, string replacement)
            => Regex.Replace(str, rex, replacement);

        public static string EnsureEndWithNewLine(this string str)
        {
            return str.EndsWith(NewLine)
                ? str
                : str + NewLine;
        }

        public static byte[] ToBytesFromHex(this string hex)
        {
            if (hex == null) throw new ArgumentNullException(nameof(hex));
            if (hex.Length == 0) return Array.Empty<byte>();
            if (hex.Length % 2 == 1) throw new Exception("The binary key cannot have an odd number of digits");

            var len = hex.Length >> 1;
            var arr = new byte[len];

            for (var i = 0; i < len; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;

            static int GetHexVal(char hex)
            {
                var val = (int)hex;
                //For uppercase A-F letters:
                //return val - (val < 58 ? 48 : 55);
                //For lowercase a-f letters:
                //return val - (val < 58 ? 48 : 87);
                //Or the two combined, but a bit slower:
                return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
            }
        }

        public static byte[] ToBytesFromBase64(this string base64String) => Convert.FromBase64String(base64String);
    }
}
