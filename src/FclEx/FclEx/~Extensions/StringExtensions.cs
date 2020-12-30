using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Chinese;
using Dawn;
using FclEx.Utils;
using static System.Environment;

namespace FclEx
{
    partial class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JoinWith(this IEnumerable<string> strs, string separator = "") => string.Join(separator, strs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsAny(this string src, IEnumerable<string> items, StringComparison comp = StringComparison.Ordinal)
            => items.Any(m => src.Contains(m, comp));

        public static bool ContainsAll(this string src, IEnumerable<string> items, StringComparison comp = StringComparison.Ordinal)
            => items.Any(m => src.Contains(m, comp));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string str, params object[] args) => string.Format(str, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fmt(this string str, params object[] args) => string.Format(str, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fmt(this string str, object arg0) => string.Format(str, arg0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Fmt(this string str, object arg0, object arg1) => string.Format(str, arg0, arg1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid([NotNullWhen(true)] this string? x) => !x.IsNullOrEmpty();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToUtf8Bytes(this string input) => input.ToBytes(Encoding.UTF8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this string input, Encoding? encoding = null) => (encoding ?? Encoding.UTF8).GetBytes(input);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("url")]
        public static string? UrlEncode(this string? url) => WebUtility.UrlEncode(url);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("url")]
        public static string? UrlDecode(this string? url) => WebUtility.UrlDecode(url);

        public static string TruncateSafely(this string? str, int maxLength)
        {
            if (maxLength <= 0)
                return string.Empty;

            if (str.IsNullOrEmpty() || maxLength >= str.Length)
                return str ?? string.Empty;

            var count = (maxLength + 1) / 2;
            return str.Substring(0, count) + " ... " + str.Substring(str.Length - count);
        }

        private static readonly Regex RegexOfXmlProlog = new(@"^<\?xml.+\?>");
        private static readonly Regex RegexOfXmlStart = new(@"^<\S+>");
        private static readonly Regex RegexOfXmlEnd = new(@"</\S+>$");

        public static bool IsPossibleXml([NotNullWhen(true)] this string? data)
        {
            /*  
                XML documents must have a root element
                XML elements must have a closing tag
                XML tags are case sensitive
                XML elements must be properly nested
                XML attribute values must be quoted
                
                <?xml version="1.0" encoding="UTF-8"?> 
                The XML prolog is optional. If it exists, it must come first in the document.
                
                <root>
                  <child>
                    <subchild>.....</subchild>
                  </child>
                </root>
             */

            if (!data.IsValid())
                return false;

            if (!RegexOfXmlProlog.IsMatch(data) && !RegexOfXmlStart.IsMatch(data))
                return false;

            if (!RegexOfXmlEnd.IsMatch(data))
                return false;

            return true;
        }

        public static bool IsPossibleHtml([NotNullWhen(true)] this string? data)
        {
            if (!data.IsValid())
                return false;

            return true;
        }

        public static (string Left, string Right) SplitTwo(this string? str, string separator)
        {
            if (str.IsNullOrEmpty())
                return ("", "");

            var index = str.IndexOf(separator, StringComparison.Ordinal);
            if (index < 0) return (str, "");
            return (str[..index], str[(index + separator.Length)..]);
        }

        public static string ToZhCn(this string str)
        {
            return ChineseConverter.ToSimplified(str);
        }

        public static string GetSub(this string str, char[] separators, Func<string[], string> selector)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (separators.IsNullOrEmpty())
                return str;

            var subs = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return selector(subs);
        }

        public static string FirstSub(this string str, char[] separators)
        {
            return str.GetSub(separators, arr => arr.First());
        }

        public static string LastSub(this string str, char[] separators)
        {
            return str.GetSub(separators, arr => arr.Last());
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
