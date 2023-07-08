namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith(this IEnumerable<string?> enumerable, string separator) 
        => string.Join(separator, enumerable);

    public static IEnumerable<string> Valid(this IEnumerable<string?> col) 
        => col.Where(m => m.IsValid())!;

    public static bool ContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comp = StringComparison.Ordinal) 
        => enumerable.Any(m => m.ContainsAny(values, comp));

    public static bool ContainsAnyIgnoreCase(this IEnumerable<string> enumerable, IEnumerable<string> values)
        => enumerable.ContainsAny(values, StringComparison.OrdinalIgnoreCase);
    
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static string? FirstValid(this IEnumerable<string?> values, int? count = null, string? defaultValue = "")
    {
        var q = values;
        if (count.HasValue)
            q = q.Take(count.Value);
        return q.FirstOrDefault(m => m.IsValid()) ?? defaultValue;
    }
}