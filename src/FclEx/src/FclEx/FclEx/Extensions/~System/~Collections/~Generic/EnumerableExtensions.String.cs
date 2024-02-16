namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith(this IEnumerable<string?> enumerable, string separator) 
        => string.Join(separator, enumerable);

    public static IEnumerable<string> NotEmpty(this IEnumerable<string?> col) 
        => col.Where(m => m.IsNotEmpty())!;

    public static bool ContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comp = StringComparison.Ordinal) 
        => enumerable.Any(m => m.ContainsAny(values, comp));

    public static bool ContainsAnyIgnoreCase(this IEnumerable<string> enumerable, IEnumerable<string> values)
        => enumerable.ContainsAny(values, StringComparison.OrdinalIgnoreCase);

    public static bool AnyContains(this IEnumerable<string> enumerable, string value, StringComparison comp = StringComparison.Ordinal)
        => enumerable.Any(m => m.Contains(value, comp));

    public static bool AnyContainsIgnoreCase(this IEnumerable<string> enumerable, string value)
        => enumerable.AnyContains(value, StringComparison.OrdinalIgnoreCase);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static string? FirstNotEmpty(this IEnumerable<string?> values, int? count = null, string? defaultValue = "")
    {
        return values.TryTake(count).FirstOrDefault(m => m.IsNotEmpty()) ?? defaultValue;
    }
}