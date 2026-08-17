namespace FclEx.Extensions.EnumerableExtensions;

public class AverageTimeSpanTests
{
    [Fact]
    public void Average_PreservesTickPrecisionNearTimeSpanMaximum()
    {
        var values = new[]
        {
            TimeSpan.MaxValue,
            TimeSpan.MaxValue - TimeSpan.FromTicks(2),
        };

        var average = values.Average(value => value);

        Assert.Equal(TimeSpan.MaxValue - TimeSpan.FromTicks(1), average);
    }
}
