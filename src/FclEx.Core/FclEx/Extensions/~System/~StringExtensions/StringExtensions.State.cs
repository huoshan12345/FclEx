namespace FclEx.Extensions;

partial class StringExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotEmpty([NotNullWhen(true)] this string? str) => str.IsNullOrEmpty() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);

    public static string IfEmpty(this string? str, string defaultValue)
    {
        return str.IsNotEmpty() ? str : defaultValue;
    }
}
