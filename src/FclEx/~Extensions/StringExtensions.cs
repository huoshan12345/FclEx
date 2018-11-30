using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static System.Environment;

namespace FclEx
{
    public static class StringExtensions
    {
        public static string StripPrefix(this string text, string prefix)
        {
            return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
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

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        public static string RegexReplace(this string str, string rex, string replacement) => Regex.Replace(str, rex, replacement);

        public static string GetOrEmpty(this string str) => str ?? "";

        public static string JoinWith(this IEnumerable<string> strs, string separator = "") => string.Join(separator, strs);

        public static string JoinWith<T>(this IEnumerable<T> strs, string separator = "") => string.Join(separator, strs);

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static string FirstPart(this string str, char[] separators)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (separators.IsNullOrEmpty()) return str;
            return str.Split(separators, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static bool ContainsAny(this string src, IEnumerable<string> items,
            StringComparison comp = StringComparison.CurrentCulture)
            => items.Any(m => src.Contains(m, comp));

        public static bool ContainsAll(this string src, IEnumerable<string> items,
            StringComparison comp = StringComparison.CurrentCulture)
            => items.Any(m => src.Contains(m, comp));

        public static string EnsureEndWithNewLine(this string str)
        {
            return str.EndsWith(NewLine)
                ? str
                : str + NewLine;
        }

        public static string EnsureNotNull(this string str)
        {
            return str ?? "";
        }

        public static string Format(this string str, params object[] args)
        {
            return string.Format(str, args);
        }
    }
}
