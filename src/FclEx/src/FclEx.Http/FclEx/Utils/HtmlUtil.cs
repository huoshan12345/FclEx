using System.Text.RegularExpressions;
using FclEx.Extensions;

namespace FclEx.Utils;

public static class HtmlUtil
{
    public static Regex RegexOfMetaRefresh { get; } = new Regex(@"<meta +http-equiv=""refresh"" +content=""(.+)""/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static Regex RegexOfMetaRefreshUrl { get; } = new Regex(@"^\s*(\d+)(?:\s*;(?:\s*url\s*=)?\s*(?:[""']\s*(.*?)\s*['""]|(.*?)))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static Regex RegOfCharSet { get; } = new Regex("<meta[^<]*charset=([^<]*)[\"']", RegexOptions.Compiled);
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

    public static string? GetMetaRefreshUrl(string html)
    {
        if (html.IsNullOrWhiteSpace())
            return null;

        var match = RegexOfMetaRefresh.Match(html);
        if (match.Success)
        {
            var refresh = match.Groups[1].Value;

            refresh = refresh.Replace("&#x27;", "'")
                .Replace("&#39;", "'")
                .Replace("&#x22;", "\"")
                .Replace("&#34;", "\"")
                .Trim();
            var nextMatch = RegexOfMetaRefreshUrl.Match(refresh);
            if (nextMatch.Success)
            {
                var g = nextMatch.Groups;
                return (g[2].Value, g[3].Value).FirstValid();
            }

            return null;
        }
        else
        {
            return null;
        }
    }
}