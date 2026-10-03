namespace FclEx.Extensions;

partial class StringExtensions
{
    extension(string)
    {
        public static string operator *(string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }
    }
}
