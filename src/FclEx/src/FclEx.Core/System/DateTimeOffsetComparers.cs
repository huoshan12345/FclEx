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
    private readonly TimeSpan _precision;


    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeOffsetPrecisionEqualityComparer"/> class
    /// with the specified precision.
    /// </summary>
    /// <param name="precision">The precision to use when comparing two <see cref="DateTimeOffset"/> values.</param>
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
        if (_precision != TimeSpan.Zero)
            throw new NotSupportedException($"This comparer with non-zero precision '{_precision}' does not support GetHashCode.");

        return obj.GetHashCode();
    }
}

/// <summary>
/// Provides an equality comparer for <see cref="DateTimeOffset"/> objects with millisecond-level precision.
/// </summary>
/// <remarks>
/// This comparer considers two <see cref="DateTimeOffset"/> values equal if the absolute difference 
/// between them is less than or equal to one millisecond. 
/// It is a specialized implementation of <see cref="PrecisionDateTimeOffsetComparer"/> with a precision of 1 millisecond.
/// </remarks>
public class MillisecondsPrecisionComparer : PrecisionDateTimeOffsetComparer
{
    public static readonly MillisecondsPrecisionComparer Instance = new();

    public MillisecondsPrecisionComparer() : base(TimeSpan.FromMilliseconds(1))
    {
    }
}
