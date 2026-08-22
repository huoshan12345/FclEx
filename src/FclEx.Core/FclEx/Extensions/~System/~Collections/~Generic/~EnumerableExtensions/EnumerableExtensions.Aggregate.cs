namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>Calculates the arithmetic mean of selected time spans with tick precision.</summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Selects the duration from each source item.</param>
    /// <returns>The average duration, truncated toward zero when the exact average is between ticks.</returns>
    /// <remarks>
    /// Ticks are accumulated as <see cref="decimal"/> values so ordinary <see cref="long"/> tick values do not lose
    /// precision through floating-point conversion or overflow while being summed.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty.</exception>
    public static TimeSpan Average<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
    {
        var ticks = (long)source.Average(m => (decimal)selector(m).Ticks);
        return TimeSpan.FromTicks(ticks);
    }

    public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
    {
        var ticks = source.Sum(m => selector(m).Ticks);
        return TimeSpan.FromTicks(ticks);
    }

#if NET7_0_OR_GREATER
    public static T Sum<T>(this IEnumerable<T> enumerable) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        return enumerable.Aggregate(T.AdditiveIdentity, (current, item) => current + item);
    }
#endif
}
