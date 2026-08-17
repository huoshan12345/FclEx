namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static IEnumerable<string> NotEmpty(this IEnumerable<string?> col)
        => col.Where(m => m.IsNotEmpty())!;

    public static bool AnyContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var items = enumerable.AsIReadOnlyCollection();
        return values.Any(m => items.Any(x => x.Contains(m, comparison)));
    }

    public static bool AnyContainsAll(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var items = enumerable.AsIReadOnlyCollection();
        return values.All(m => items.Any(x => x.Contains(m, comparison)));
    }

    public static bool AllContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var items = enumerable.AsIReadOnlyCollection();
        return values.Any(m => items.All(x => x.Contains(m, comparison)));
    }

    public static bool AllContainsAll(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var items = enumerable.AsIReadOnlyCollection();
        return values.All(m => items.All(x => x.Contains(m, comparison)));
    }

    public static bool AnyContains(this IEnumerable<string> enumerable, string value, StringComparison comparison = StringComparison.Ordinal)
        => enumerable.Any(m => m.Contains(value, comparison));

    public static bool AnyContainsIgnoreCase(this IEnumerable<string> enumerable, string value)
        => enumerable.AnyContains(value, StringComparison.OrdinalIgnoreCase);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    internal static string? FirstNotEmpty(this IEnumerable<string?> values, int? count, string? defaultValue = "")
    {
        return values.TryTake(count).FirstOrDefault(m => m.IsNotEmpty()) ?? defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    internal static string? FirstNotEmpty(this IEnumerable<string?> values, string? defaultValue = "")
    {
        return values.FirstOrDefault(m => m.IsNotEmpty()) ?? defaultValue;
    }
}