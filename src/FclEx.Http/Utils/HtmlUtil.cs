using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FclEx.Utils
{
    public static class HtmlUtil
    {
        private static Regex RegOfCharSet { get; } = new Regex("<meta[^<]*charset=([^<]*)[\"']", RegexOptions.Compiled);
        private static char[] TrimChars { get; } = { '\'', '"', ';' };

        public static string? GetMetaCharSet(string html)
        {
            if (html.IsNullOrEmpty())
                return null;

            var match = RegOfCharSet.Match(html);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim(TrimChars);
            }
            else
            {
                return null;
            }
        }
    }
}
