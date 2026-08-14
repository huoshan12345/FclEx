namespace FclEx.Extensions.TaskExtensions;

public class BasicTests
{
    [RetryFact]
    public async Task DelaySafely_CancellationEndsDelayWithoutCancelingReturnedTask()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.1));
        var watch = ValueStopwatch.StartNew();
        await Task.DelaySafely(TimeSpan.FromSeconds(10), cts.Token);
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds < 1, time.ToString());
    }
}
