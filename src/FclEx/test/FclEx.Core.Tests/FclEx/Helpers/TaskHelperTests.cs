namespace FclEx.Helpers;

public class TaskHelperTests
{
    [RetryFact]
    public async Task Delay_WithToken_Test()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.1));
        var watch = ValueStopwatch.StartNew();
        await TaskHelper.Delay(10, cts.Token);
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds < 1, time.ToString());
    }

    [RetryFact]
    public async Task DelayMilli_WithToken_Test()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.1));
        var watch = ValueStopwatch.StartNew();
        await TaskHelper.DelayMilli(10 * 1000, cts.Token);
        var time = watch.GetElapsedTime();
        Assert.True(time.TotalSeconds < 1, time.ToString());
    }

    [RetryFact]
    public async Task AwaitObject_Task_Tests()
    {
        var task = Task.CompletedTask;
        var result = await TaskHelper.AwaitObject(task);
        Assert.Null(result);
    }

    [RetryFact]
    public async Task AwaitObject_TaskOfInternalClass_Tests()
    {
        var task = Task.FromResult(new InternalClass(1));
        var result = await TaskHelper.AwaitObject(task);
        Assert.True(result is InternalClass { Value: 1 });
    }

    [RetryFact]
    public async Task AwaitObject_ValueTask_Tests()
    {
        var task = ValueTask.CompletedTask;
        var result = await TaskHelper.AwaitObject(task);
        Assert.Null(result);
    }

    [RetryFact]
    public async Task AwaitObject_ValueTaskOfInternalClass_Tests()
    {
        var task = ValueTask.FromResult(new InternalClass(1));
        var result = await TaskHelper.AwaitObject(task);
        Assert.True(result is InternalClass { Value: 1 });
    }

    public record InternalClass(int Value);
}