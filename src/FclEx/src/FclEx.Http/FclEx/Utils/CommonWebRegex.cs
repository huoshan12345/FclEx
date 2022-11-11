using System.Text.RegularExpressions;

namespace FclEx.Utils;

public class CommonWebRegex
{
    public const string CallBackName = "_callback";
    public static Regex EmailCheck { get; } = new(@"[\da-zA-Z]+@[\da-zA-Z]+[\.][\da-zA-Z]{2,5}", RegexOptions.Compiled);
    public static Regex GetCallBackContent { get; } = new(@"(?<=" + CallBackName + @"\().+(?=\))", RegexOptions.Compiled);
    public static Regex GetJson { get; } = new(@"{.+}", RegexOptions.Compiled);
}