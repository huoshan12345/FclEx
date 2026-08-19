namespace FclEx.Extensions.TaskExtensions;

public record InternalClass(int Value);

public sealed class SingleConsumptionValueTaskSource<T>(T result) : IValueTaskSource<T>
{
    public int ConsumptionCount { get; private set; }

    public ValueTask<T> CreateValueTask() => new(this, 0);

    public T GetResult(short token)
    {
        Assert.Equal((short)0, token);
        if (++ConsumptionCount != 1)
            throw new InvalidOperationException("The value task source was consumed more than once.");
        return result;
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        Assert.Equal((short)0, token);
        return ValueTaskSourceStatus.Succeeded;
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        Assert.Equal((short)0, token);
        continuation(state);
    }
}


public class AwaitObjectTests
{
    [RetryFact]
    public async Task AwaitObject_Task_Tests()
    {
        var task = Task.CompletedTask;
        var result = await Task.AwaitObject(task);
        Assert.Null(result);
    }

    [RetryFact]
    public async Task AwaitObject_TaskOfInternalClass_Tests()
    {
        var task = Task.FromResult(new InternalClass(1));
        var result = await Task.AwaitObject(task);
        Assert.True(result is InternalClass { Value: 1 });
    }

    [RetryFact]
    public async Task AwaitObject_ValueTask_Tests()
    {
        var task = ValueTask.CompletedTask;
        var result = await Task.AwaitObject(task);
        Assert.Null(result);
    }

    [RetryFact]
    public async Task AwaitObject_ValueTaskOfInternalClass_Tests()
    {
        var task = ValueTask.FromResult(new InternalClass(1));
        var result = await Task.AwaitObject(task);
        Assert.True(result is InternalClass { Value: 1 });
    }

    [Fact]
    public async Task AwaitObject_ShouldConsumeValueTaskSourceOnlyOnce()
    {
        var source = new SingleConsumptionValueTaskSource<InternalClass>(new InternalClass(42));

        var result = await Task.AwaitObject(source.CreateValueTask());

        Assert.Equal(new InternalClass(42), result);
        Assert.Equal(1, source.ConsumptionCount);
    }

}
