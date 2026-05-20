namespace FclEx;

public static class RegexesExtensions
{
    public const string CallbackName = "_callback";

    private static readonly Regex _emailCheck = new(@"[\da-zA-Z]+@[\da-zA-Z]+[\.][\da-zA-Z]{2,5}", RegexOptions.Compiled);
    private static readonly Regex _callbackContent = new("(?<=" + CallbackName + @"\().+(?=\))", RegexOptions.Compiled);
    private static readonly Regex _metaRefresh = new("""<meta +http-equiv="refresh" +content="(.+)"/>""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _metaRefreshUrl = new("""^\s*(\d+)(?:\s*;(?:\s*url\s*=)?\s*(?:["']\s*(.*?)\s*['"]|(.*?)))?\s*$""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _charSet = new("<meta[^<]*charset=([^<]*)[\"']", RegexOptions.Compiled);

    extension(Regexes)
    {
        public static string CallbackName => CallbackName;
        public static Regex EmailCheck => _emailCheck;
        public static Regex CallbackContent => _callbackContent;
        public static Regex MetaRefresh => _metaRefresh;
        public static Regex MetaRefreshUrl => _metaRefreshUrl;
        public static Regex CharSet => _charSet;
    }
}
