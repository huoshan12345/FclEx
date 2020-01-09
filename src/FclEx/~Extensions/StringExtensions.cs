using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Dawn;
using FclEx.Utils;
using static System.Environment;

namespace FclEx
{
    partial class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetOrEmpty(this string str) => str ?? "";

        public static string JoinWith(this IEnumerable<string> strs, string separator = "") => string.Join(separator, strs);

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ContainsAny(this string src, IEnumerable<string> items, StringComparison comp = StringComparison.Ordinal)
            => items.Any(m => src.Contains(m, comp));

        public static bool ContainsAll(this string src, IEnumerable<string> items, StringComparison comp = StringComparison.Ordinal)
            => items.Any(m => src.Contains(m, comp));

        public static string Format(this string str, params object[] args) => string.Format(str, args);

        public static string Fmt(this string str, params object[] args) => string.Format(str, args);

        public static string Fmt(this string str, object arg0) => string.Format(str, arg0);

        public static string Fmt(this string str, object arg0, object arg1) => string.Format(str, arg0, arg1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this string x)
        {
            return !x.IsNullOrEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IfEmpty(this string x, string y)
        {
            return x.IsValid() ? x : y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IfEmpty(this string x, string y, string z)
        {
            return x.IsValid()
                ? x
                : y.IsValid()
                    ? y
                    : z;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToUtf8Bytes(this string input) => input.ToBytes(Encoding.UTF8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this string input, Encoding encoding = null) => (encoding ?? Encoding.UTF8).GetBytes(input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string UrlEncode(this string url) => WebUtility.UrlEncode(url);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string UrlDecode(this string url) => WebUtility.UrlDecode(url);
    }
}
