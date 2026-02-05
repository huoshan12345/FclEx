namespace System;

/// <summary>
/// Provides a custom equality comparison for <see cref="DateTimeOffset"/> objects
/// with a specified level of precision.
/// </summary>
/// <remarks>
/// This comparer determines equality based on whether the absolute difference
/// between two <see cref="DateTimeOffset"/> values is within the configured precision.
/// It supports equality checks through the <see cref="Equals"/> method but does not
/// fully implement <see cref="GetHashCode"/> when a non-zero precision is specified,
/// as hash codes cannot account for precision-based equality reliably.
/// </remarks>
public class PrecisionDateTimeOffsetComparer : IEqualityComparer<DateTimeOffset>
{
    /// <summary>
    /// It is a specialized implementation of <see cref="PrecisionDateTimeOffsetComparer"/> with a precision of 1 millisecond.
    /// </summary>
    public static readonly PrecisionDateTimeOffsetComparer Millisecond = new(TimeSpan.FromMilliseconds(1));

    private readonly TimeSpan _precision;

    public PrecisionDateTimeOffsetComparer(TimeSpan precision)
    {
        _precision = precision;
    }

    public bool Equals(DateTimeOffset x, DateTimeOffset y)
    {
        if (x == y)
            return true;

        var timeSpan = (x - y).Duration();
        return timeSpan <= _precision;
    }

    public int GetHashCode(DateTimeOffset obj)
    {
        return _precision == TimeSpan.Zero
            ? obj.GetHashCode()
            : throw new NotSupportedException($"This comparer with non-zero precision '{_precision}' does not support GetHashCode.");
    }
}
