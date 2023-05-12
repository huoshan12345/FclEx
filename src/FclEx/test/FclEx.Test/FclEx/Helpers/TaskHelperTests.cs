using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Helpers;

public class TaskHelperTests
{
    [Fact]
    public async Task Delay_WithToken_Test()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
        var watch = ValueStopwatch.StartNew();
        await TaskHelper.Delay(10, cts.Token);
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds < 5, time.TotalSeconds.ToString());
    }

    [Fact]
    public async Task DelayMilli_WithToken_Test()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
        var watch = ValueStopwatch.StartNew();
        await TaskHelper.DelayMilli(10 * 1000, cts.Token);
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds < 5, time.TotalSeconds.ToString());
    }
}