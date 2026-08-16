namespace FclEx.Extensions;

public static class CharExtensions
{
    public static char ToUpper(this char c, CultureInfo culture) => char.ToUpper(c, culture);
    public static char ToUpper(this char c) => char.ToUpper(c);
    public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
    public static char ToLower(this char c, CultureInfo culture) => char.ToLower(c, culture);
    public static char ToLower(this char c) => char.ToLower(c);
    public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);
    public static bool IsDigit(this char c) => char.IsDigit(c);
    public static bool IsAsciiDigit(this char c) => c is >= '0' and <= '9';
    public static bool IsLetter(this char c) => char.IsLetter(c);
    public static bool IsLetterOrDigit(this char c) => char.IsLetterOrDigit(c);
    public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);
    public static bool IsEnglishUppercase(this char c) => c is >= 'A' and <= 'Z';
    public static bool IsEnglishLowercase(this char c) => c is >= 'a' and <= 'z';
    public static bool IsHex(this char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
}