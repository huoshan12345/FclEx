namespace FclEx.Extensions;

public class ValueStopwatchExtensionsTests
{
    [Theory]
    [InlineData(0, 1, 2, 3, 500, "01:02:03")]
    [InlineData(0, 0, 0, 0, 999, "00:00:00")]
    [InlineData(1, 0, 0, 0, 0, "1.00:00:00")]
    [InlineData(1, 2, 3, 4, 5, "1.02:03:04")]
    [InlineData(0, 25, 0, 0, 0, "1.01:00:00")]
    [InlineData(0, 0, 0, 59, 0, "00:00:59")]
    public void ToSecondsString_FormatsCorrectly(int d, int h, int m, int s, int ms, string expected)
    {
        var ts = new TimeSpan(d, h, m, s, ms);
        var result = ts.ToSecondsString();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToSecondsString_HandlesNegativeTime()
    {
        var ts = TimeSpan.FromSeconds(-70);
        var result = ts.ToSecondsString();
        Assert.Equal("-00:01:10", result);
    }

    [RetryFact]
    public async Task ElapsedSeconds_Test()
    {
        var watch = ValueStopwatch.StartNew();
        await Task.Delay(TimeSpan.FromMilliseconds(110));
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds > 0.1, () => $"Elapsed time was {time.TotalSeconds} seconds");
    }
}