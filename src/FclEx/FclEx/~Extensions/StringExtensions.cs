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
    }
}
