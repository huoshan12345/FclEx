namespace FclEx.Http;

public class CommonWebRegexes
{
    public const string CallBackName = "_callback";
    public static Regex EmailCheck { get; } = new(@"[\da-zA-Z]+@[\da-zA-Z]+[\.][\da-zA-Z]{2,5}", RegexOptions.Compiled);
    public static Regex CallBackContent { get; } = new(@"(?<=" + CallBackName + @"\().+(?=\))", RegexOptions.Compiled);
    public static Regex JsonObject { get; } = new(@"{.+}", RegexOptions.Compiled);
}