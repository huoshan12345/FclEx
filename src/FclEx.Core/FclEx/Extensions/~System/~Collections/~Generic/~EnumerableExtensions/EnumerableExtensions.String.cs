namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static IEnumerable<string> NotEmpty(this IEnumerable<string?> col)
        => col.Where(m => m.IsNotEmpty())!;

    /// <summary>Determines whether any source string contains any supplied value.</summary>
    /// <remarks>The source sequence is materialized once so it may be a one-shot sequence.</remarks>
    public static bool AnyContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var vals = values.AsIReadOnlyCollection();
        return enumerable.Any(m => vals.Any(x => m.Contains(x, comparison)));
    }

    /// <summary>Determines whether a source string exists for every supplied value.</summary>
    /// <remarks>The source sequence is materialized once so it may be a one-shot sequence.</remarks>
    public static bool AnyContainsAll(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var vals = values.AsIReadOnlyCollection();
        return enumerable.Any(m => vals.All(x => m.Contains(x, comparison)));
    }

    /// <summary>Determines whether every source string contains at least one supplied value.</summary>
    /// <remarks>The source sequence is materialized once so it may be a one-shot sequence.</remarks>
    public static bool AllContainsAny(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var vals = values.AsIReadOnlyCollection();
        return enumerable.All(m => vals.Any(x => m.Contains(x, comparison)));
    }

    /// <summary>Determines whether every source string contains every supplied value.</summary>
    /// <remarks>The source sequence is materialized once so it may be a one-shot sequence.</remarks>
    public static bool AllContainsAll(this IEnumerable<string> enumerable, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
    {
        var vals = values.AsIReadOnlyCollection();
        return enumerable.All(m => vals.All(x => m.Contains(x, comparison)));
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
