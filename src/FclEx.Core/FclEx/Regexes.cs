namespace FclEx;

public static class Regexes
{
    public static readonly Regex AutoPropertyBackingField = new(RegexPatterns.AutoPropertyBackingField, RegexOptions.Compiled);
    public static readonly Regex Md5 = new(RegexPatterns.Md5, RegexOptions.Compiled);
}
