namespace FclEx.Extensions;

partial class StringExtensions
{
    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlEncode(this string? value) => HttpUtility.UrlEncode(value);

    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlDecode(this string? value) => HttpUtility.UrlDecode(value);

    [MethodImpl(AggressiveInlining)]
    public static string UriEscape(this string value) => Uri.EscapeDataString(value);

    [MethodImpl(AggressiveInlining)]
    public static string UriUnescape(this string value) => Uri.UnescapeDataString(value);
}
