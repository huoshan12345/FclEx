namespace FclEx.Extensions;

public class ValueStopwatchExtensionsTests(ITestOutputHelper output)
{
    [Fact]
    public void ToSecondsString_Test()
    {
        var timeSpans = new TimeSpan[]
        {
            new(1, 1, 1, 1, 100),
            new(0, 1, 1, 1, 100),
        };

        foreach (var timeSpan in timeSpans)
        {
            output.WriteLine(timeSpan.ToString());
            output.WriteLine(timeSpan.ToSecondsString());
            output.WriteLine();
        }
    }

    [Fact]
    public async Task ElapsedSeconds_Test()
    {
        var watch = ValueStopwatch.StartNew();
        await Task.Delay(TimeSpan.FromMilliseconds(110));
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds > 0.1);
    }
}