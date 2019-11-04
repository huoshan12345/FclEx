using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Dawn;
using FclEx.Utils;
using static System.Environment;

namespace FclEx
{
    public static class StringExtensions
    {
        public static string TrimStart(this string text, string prefix)
        {
            Guard.Argument(text, nameof(text)).NotNull();
            return text.IsValid() && prefix.IsValid() && text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
        }

        public static string TrimEnd(this string text, string suffix)
        {
            Guard.Argument(text, nameof(text)).NotNull();
            return text.IsValid() && suffix.IsValid() && text.EndsWith(suffix) ? text.Substring(0, text.Length - suffix.Length) : text;
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        public static string GetOrEmpty(this string str) => str ?? "";

        public static string JoinWith(this IEnumerable<string> strs, string separator = "") => string.Join(separator, strs);

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsAny(this string src, IEnumerable<string> items,
            StringComparison comp = StringComparison.CurrentCulture)
            => items.Any(m => src.Contains(m, comp));

        public static bool ContainsAll(this string src, IEnumerable<string> items,
            StringComparison comp = StringComparison.CurrentCulture)
            => items.Any(m => src.Contains(m, comp));

        public static string Format(this string str, params object[] args) => string.Format(str, args);

        public static string Fmt(this string str, params object[] args) => string.Format(str, args);

        public static string Fmt(this string str, object arg0) => string.Format(str, arg0);

        public static string Fmt(this string str, object arg0, object arg1) => string.Format(str, arg0, arg1);

        public static bool IsValid(this string x)
        {
            return !x.IsNullOrEmpty();
        }

        public static string IfEmpty(this string x, string y)
        {
            return x.IsValid() ? x : y;
        }

        public static string IfEmpty(this string x, string y, string z)
        {
            return x.IsValid()
                ? x
                : y.IsValid()
                    ? y
                    : z;

        }

        public static string LeftPart(this string strVal, char needle)
        {
            if (strVal == null) return null;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Substring(0, pos);
        }

        public static string LeftPart(this string strVal, string needle)
        {
            if (strVal == null) return null;
            var pos = strVal.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            return pos == -1
                ? strVal
                : strVal.Substring(0, pos);
        }

        public static string RightPart(this string strVal, char needle)
        {
            if (strVal == null) return null;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Substring(pos + 1);
        }

        public static string RightPart(this string strVal, string needle)
        {
            if (strVal == null) return null;
            var pos = strVal.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            return pos == -1
                ? strVal
                : strVal.Substring(pos + needle.Length);
        }
    }
}
