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
    public static partial class StringExtensions
    {
        public static string GetUntil(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.IndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(0, location) : str;
        }

        public static string GetWhile(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.IndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(0, location + stopAt.Length) : str;
        }

        public static string TrimEndWhile(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.LastIndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(0, location) : str;
        }

        public static string TrimEndUntil(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.LastIndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(0, location + stopAt.Length) : str;
        }

        public static string TrimStartUntil(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.IndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(location) : str;
        }

        public static string TrimStartWhile(this string str, string stopAt, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            Guard.Argument(stopAt, nameof(stopAt)).NotNull();

            var location = str.IndexOf(stopAt, comp);
            return location >= 0 ? str.Substring(location + stopAt.Length) : str;
        }

        public static string TrimStart(this string str, string prefix, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            return str.IsValid() && prefix.IsValid() && str.StartsWith(prefix, comp) ? str.Substring(prefix.Length) : str;
        }

        public static string TrimEnd(this string str, string suffix, StringComparison comp = StringComparison.Ordinal)
        {
            Guard.Argument(str, nameof(str)).NotNull();
            return str.IsValid() && suffix.IsValid() && str.EndsWith(suffix, comp) ? str.Substring(0, str.Length - suffix.Length) : str;
        }
    }
}
