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

        public static byte[] HexTobytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        public static byte[] Base64ToBytes(this string base64String) => Convert.FromBase64String(base64String);
    }
}
