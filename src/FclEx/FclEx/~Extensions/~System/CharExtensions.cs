using System.Globalization;

namespace FclEx
{
    public static class CharExtensions
    {
        public static char ToUpper(this char c, CultureInfo culture) => char.ToUpper(c, culture);
        public static char ToUpper(this char c) => char.ToUpper(c);
        public static char ToUpperInvariant(this char c) => char.ToUpperInvariant(c);
        public static char ToLower(this char c, CultureInfo culture) => char.ToLower(c, culture);
        public static char ToLower(this char c) => char.ToLower(c);
        public static char ToLowerInvariant(this char c) => char.ToLowerInvariant(c);
        public static bool IsDigit(this char ch) => char.IsDigit(ch);
        public static bool IsLetter(this char ch) => char.IsLetter(ch);
        public static bool IsLetterOrDigit(this char ch) => char.IsLetterOrDigit(ch);
    }
}
