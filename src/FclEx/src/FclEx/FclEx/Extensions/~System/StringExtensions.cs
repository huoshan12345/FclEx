using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using static System.Environment;

namespace FclEx.Extensions;

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

    /// <summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
    /// <param name="source"></param>
    /// <param name="value">The string to seek.</param>
    /// <param name="compareOptions"></param>
    /// <param name="compareInfo"></param>
    public static bool Contains(this string source, string value, CompareOptions compareOptions, CompareInfo? compareInfo = null)
    {
        compareInfo ??= CultureInfo.InvariantCulture.CompareInfo;
        return compareInfo.IndexOf(source, value, compareOptions) >= 0;
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
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlEncode(this string? value) => WebUtility.UrlEncode(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlDecode(this string? value) => WebUtility.UrlDecode(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UriEscape(this string value) => Uri.EscapeDataString(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UriUnescape(this string value) => Uri.UnescapeDataString(value);

    public static string Truncate(this string? str, int maxLength)
    {
        if (maxLength <= 0)
            return string.Empty;

        if (str.IsNullOrEmpty() || maxLength >= str.Length)
            return str ?? string.Empty;

        return str.Substring(0, maxLength) + "...";
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

    public static string Sub(this string str, char[] separators, Func<string[], string> selector)
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
        return str.Sub(separators, arr => arr.First());
    }

    public static string LastSub(this string str, char[] separators)
    {
        return str.Sub(separators, arr => arr.Last());
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

    public static Uri ToUri(this string str, UriKind uriKind = UriKind.Absolute) => new(str, uriKind);

    public static string IfEmpty(this string? str, string defaultValue)
    {
        return str.IsValid() ? str : defaultValue;
    }

    public static readonly char[] NewLineChars = { '\r', '\n' };
    public const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries;

    public static string[] SplitToLines(this string? str, StringSplitOptions options = SplitOptions)
    {
        return string.IsNullOrEmpty(str)
            ? Array.Empty<string>()
            : str!.Split(NewLineChars, options);
    }

    public static bool EndsWithAny(this string str, IEnumerable<string> patterns, StringComparison comparison = StringComparison.Ordinal)
    {
        return patterns.Any(m => str.EndsWith(m, comparison));
    }

    public static bool ContainsAny(this string src, IEnumerable<string> items, CompareOptions comp = CompareOptions.Ordinal)
    {
        return items.Any(m => src.Contains(m, comp));
    }
}