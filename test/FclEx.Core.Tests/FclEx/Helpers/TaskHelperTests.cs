using FclEx.Extensions.TaskExtensions;

namespace FclEx.Helpers;

public class TaskHelperTests
{
    [Fact]
    public async Task Repeat_Action_InvokesActionForEachRepetition()
    {
        var invocationCount = 0;

        await TaskHelper.Repeat(() => Interlocked.Increment(ref invocationCount), 5);

        Assert.Equal(5, invocationCount);
    }

    [Fact]
    public async Task Repeat_Func_InvokesFunctionForEachRepetition()
    {
        var invocationCount = 0;

        var results = await TaskHelper.Repeat(() => Interlocked.Increment(ref invocationCount), 5);

        Assert.Equal(5, invocationCount);
        Assert.Equal([1, 2, 3, 4, 5], results.OrderBy(m => m).ToArray());
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
