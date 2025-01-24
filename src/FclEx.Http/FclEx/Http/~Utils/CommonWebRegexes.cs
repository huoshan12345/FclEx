namespace FclEx.Http;

public class CommonWebRegexes
{
    public const string CallBackName = "_callback";
    public static Regex EmailCheck { get; } = new(@"[\da-zA-Z]+@[\da-zA-Z]+[\.][\da-zA-Z]{2,5}", RegexOptions.Compiled);
    public static Regex CallBackContent { get; } = new(@"(?<=" + CallBackName + @"\().+(?=\))", RegexOptions.Compiled);
    public static Regex MetaRefresh { get; } = new(@"<meta +http-equiv=""refresh"" +content=""(.+)""/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static Regex MetaRefreshUrl { get; } = new(@"^\s*(\d+)(?:\s*;(?:\s*url\s*=)?\s*(?:[""']\s*(.*?)\s*['""]|(.*?)))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static Regex CharSet { get; } = new("<meta[^<]*charset=([^<]*)[\"']", RegexOptions.Compiled);
}