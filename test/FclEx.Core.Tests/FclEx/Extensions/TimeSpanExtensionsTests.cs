namespace FclEx.Extensions;

public class TimeSpanExtensionsTests
{
    [Fact]
    public void Multiply_Int_ReturnsProduct()
    {
        var result = TimeSpan.FromSeconds(2).Multiply(3);

        Assert.Equal(TimeSpan.FromSeconds(6), result);
    }

    [Fact]
    public void Multiply_Int_ThrowsWhenResultOverflows()
    {
        Assert.Throws<OverflowException>(() => TimeSpan.MaxValue.Multiply(2));
    }

    [Fact]
    public void Multiply_Double_ReturnsTruncatedTickProduct()
    {
        var result = TimeSpan.FromTicks(3).Multiply(0.5);
        Assert.Equal(TimeSpan.FromTicks(2), result);
    }

    [Fact]
    public void Multiply_Double_ThrowsForNaNFactor()
    {
        Assert.Throws<ArgumentException>(() => TimeSpan.FromSeconds(1).Multiply(double.NaN));
    }

    [Fact]
    public void Multiply_Double_ThrowsWhenResultIsNaN()
    {
        Assert.Throws<OverflowException>(() => TimeSpan.Zero.Multiply(double.PositiveInfinity));
    }

    [Fact]
    public void Multiply_Double_ThrowsWhenResultOverflows()
    {
        Assert.Throws<OverflowException>(() => TimeSpan.MaxValue.Multiply(2.0));
    }

    [Fact]
    public void Multiply_OperatorsReturnProducts()
    {
        Assert.Equal(TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(2) * 3);
        Assert.Equal(TimeSpan.FromTicks(15), TimeSpan.FromTicks(10) * 1.5);
    }

    [Fact]
    public void With_NoArgumentsPreservesPositiveTicks()
    {
        var timeSpan = TimeSpan.FromTicks(123456789);

        var result = timeSpan.With();

        Assert.Equal(timeSpan, result);
    }

    [Fact]
    public void With_NoArgumentsPreservesNegativeTicks()
    {
        var timeSpan = TimeSpan.FromTicks(-123456789);

        var result = timeSpan.With();

        Assert.Equal(timeSpan, result);
    }

    [Fact]
    public void With_ReplacesSelectedComponentAndPreservesTickPrecision()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.With(hours: 10);

        Assert.Equal(TimeSpan.New(1, 10, 3, 4, 5, 6, 7), result);
    }

    [Fact]
    public void With_ReplacesMicrosecondsAndKeepsTickComponent()
    {
        var timeSpan = TimeSpan.New(0, 0, 0, 0, 0, 1, 2);

        var result = timeSpan.With(microseconds: 3);

        Assert.Equal(TimeSpan.New(0, 0, 0, 0, 0, 3, 2), result);
    }

    [Fact]
    public void With_ReplacesTickComponent()
    {
        var timeSpan = TimeSpan.New(0, 0, 0, 0, 0, 1, 2);

        var result = timeSpan.With(ticks: 9);

        Assert.Equal(TimeSpan.New(0, 0, 0, 0, 0, 1, 9), result);
    }

    [Fact]
    public void TruncateToMilliseconds_RemovesSubMillisecondTicks()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.TruncateToMilliseconds();

        Assert.Equal(TimeSpan.New(1, 2, 3, 4, 5, 0, 0), result);
    }

    [Fact]
    public void TruncateToSeconds_RemovesSubSecondTicks()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.TruncateToSeconds();

        Assert.Equal(TimeSpan.New(1, 2, 3, 4, 0, 0, 0), result);
    }

    [Fact]
    public void TruncateToMinutes_RemovesSubMinuteTicks()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.TruncateToMinutes();

        Assert.Equal(TimeSpan.New(1, 2, 3, 0, 0, 0, 0), result);
    }

    [Fact]
    public void TruncateToHours_RemovesSubHourTicks()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.TruncateToHours();

        Assert.Equal(TimeSpan.New(1, 2, 0, 0, 0, 0, 0), result);
    }

    [Fact]
    public void TruncateToDays_RemovesSubDayTicks()
    {
        var timeSpan = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var result = timeSpan.TruncateToDays();

        Assert.Equal(TimeSpan.New(1, 0, 0, 0, 0, 0, 0), result);
    }

    [Fact]
    public void TruncateToSeconds_NegativeValueTruncatesTowardZero()
    {
        var timeSpan = TimeSpan.FromMilliseconds(-1500);

        var result = timeSpan.TruncateToSeconds();

        Assert.Equal(TimeSpan.FromSeconds(-1), result);
    }

    [Fact]
    public void TotalWholeUnits_ReturnTruncatedCounts()
    {
        var timeSpan = new TimeSpan(1, 2, 3, 4, 567);

        Assert.Equal(timeSpan.Ticks / TimeSpan.TicksPerMillisecond, timeSpan.TotalWholeMilliseconds());
        Assert.Equal(timeSpan.Ticks / TimeSpan.TicksPerSecond, timeSpan.TotalWholeSeconds());
        Assert.Equal(timeSpan.Ticks / TimeSpan.TicksPerMinute, timeSpan.TotalWholeMinutes());
        Assert.Equal(timeSpan.Ticks / TimeSpan.TicksPerHour, timeSpan.TotalWholeHours());
        Assert.Equal(timeSpan.Ticks / TimeSpan.TicksPerDay, timeSpan.TotalWholeDays());
    }

    [Fact]
    public void TotalWholeUnits_NegativeValueTruncatesTowardZero()
    {
        var timeSpan = TimeSpan.FromMilliseconds(-1500);

        Assert.Equal(-1, timeSpan.TotalWholeSeconds());
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(5000, "0s")]
    [InlineData(10000000, "1s")]
    [InlineData(620000000, "1m2s")]
    [InlineData(937840000000, "1d2h3m4s")]
    [InlineData(-700000000, "-1m10s")]
    public void ToCompactString_ReturnsExpectedText(long ticks, string expected)
    {
        var result = TimeSpan.FromTicks(ticks).ToCompactString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void New_CreatesTimeSpanFromTickComponents()
    {
        var result = TimeSpan.New(1, 2, 3, 4, 5, 6, 7);

        var expectedTicks =
            TimeSpan.TicksPerDay +
            2 * TimeSpan.TicksPerHour +
            3 * TimeSpan.TicksPerMinute +
            4 * TimeSpan.TicksPerSecond +
            5 * TimeSpan.TicksPerMillisecond +
            6 * TimeSpan.TicksPerMicrosecond +
            7;
        Assert.Equal(TimeSpan.FromTicks(expectedTicks), result);
    }

    [Fact]
    public void New_ThrowsWhenResultOverflows()
    {
        Assert.Throws<OverflowException>(() => TimeSpan.New(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue));
    }

    [Fact]
    public void TicksPerMicrosecond_ReturnsTen()
    {
        Assert.Equal(10, TimeSpan.TicksPerMicrosecond);
    }
}
