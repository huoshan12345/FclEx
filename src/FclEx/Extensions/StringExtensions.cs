using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;

namespace FclEx.Extensions
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

        public static byte[] Base64StringToBytes(this string base64String) => Convert.FromBase64String(base64String);

        public static byte[] ToUtf8Bytes(this string input) => Encoding.UTF8.GetBytes(input);

        public static byte[] ToBytes(this string input, Encoding encoding) => encoding.GetBytes(input);

        public static string UrlEncode(this string url) => WebUtility.UrlEncode(url);

        public static string UrlDecode(this string url) => WebUtility.UrlDecode(url);
    }
}
