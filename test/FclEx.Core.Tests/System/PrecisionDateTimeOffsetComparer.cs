namespace System;

public class PrecisionDateTimeOffsetComparerTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenDatesAreExactlyEqual()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.FromMilliseconds(1));
        var date1 = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var date2 = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        Assert.True(comparer.Equals(date1, date2));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenDatesAreWithinPrecision()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.FromMilliseconds(10));
        var date1 = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var date2 = date1.AddMilliseconds(5);

        Assert.True(comparer.Equals(date1, date2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDatesAreOutsidePrecision()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.FromMilliseconds(10));
        var date1 = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var date2 = date1.AddMilliseconds(15);

        Assert.False(comparer.Equals(date1, date2));
    }

    [Fact]
    public void GetHashCode_ShouldThrowException_WhenPrecisionIsNonZero()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.FromMilliseconds(10));
        var date = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<NotSupportedException>(() => comparer.GetHashCode(date));
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHash_WhenPrecisionIsZero()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.Zero);
        var date = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var hash1 = comparer.GetHashCode(date);
        var hash2 = comparer.GetHashCode(date);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDatesDifferSignificantly()
    {
        var comparer = new PrecisionDateTimeOffsetComparer(TimeSpan.FromMinutes(1));
        var date1 = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var date2 = date1.AddMinutes(2);

        Assert.False(comparer.Equals(date1, date2));
    }
}