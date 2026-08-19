namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source is null || source.AnyEx() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsEmpty<T>(this IEnumerable<T> source) => source.AnyEx() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? enumerable) => enumerable.IsNullOrEmpty() == false;

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    public static bool AnyEx<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);

        return enumerable is IReadOnlyCollection<T> collection
            ? collection.Count > 0
            : enumerable.Any();
    }
}
