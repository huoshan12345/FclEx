using System.Threading.Tasks.Sources;

namespace FclEx.Helpers;

public class TaskHelperTests
{
    private sealed class SingleConsumptionValueTaskSource<T>(T result) : IValueTaskSource<T>
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

    [Fact]
    public async Task AwaitObject_ShouldConsumeValueTaskSourceOnlyOnce()
    {
        var source = new SingleConsumptionValueTaskSource<InternalClass>(new InternalClass(42));

        var result = await TaskHelper.AwaitObject(source.CreateValueTask());

        Assert.Equal(new InternalClass(42), result);
        Assert.Equal(1, source.ConsumptionCount);
    }
    
    [Fact]
    public async Task RunAsync_ShouldCancelTheOperationAndThrowOnTimeout()
    {
        CancellationToken operationToken = default;

        await Assert.ThrowsAsync<TimeoutException>(() => TaskHelper.RunAsync(
            async token =>
            {
                operationToken = token;
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            },
            TimeSpan.FromMilliseconds(100)));

        Assert.True(operationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task RunAsync_ShouldStopWaitingWhenTheOperationIgnoresTimeoutCancellation()
    {
        var operation = new TaskCompletionSource<object?>();

        await Assert.ThrowsAsync<TimeoutException>(() => TaskHelper.RunAsync(
            _ => operation.Task,
            TimeSpan.FromMilliseconds(100)));

        operation.TrySetResult(null);
    }

    [Fact]
    public async Task RunAsync_ShouldPropagateCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var invoked = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => TaskHelper.RunAsync(
            _ =>
            {
                invoked = true;
                return Task.CompletedTask;
            },
            cancellationToken: cancellation.Token));

        Assert.False(invoked);
    }

    [Fact]
    public async Task RunAsync_ValueTask_ShouldReturnResult()
    {
        var result = await TaskHelper.RunAsync(_ => ValueTask.FromResult(42).AsTask());

        Assert.Equal(42, result);
    }

#if !NET6_0_OR_GREATER
    [Fact]
    public async Task WaitAsync_ShouldPreferAnAlreadyCompletedTaskOverCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Task.CompletedTask.WaitAsync(cancellation.Token);
    }

    [Fact]
    public async Task WaitAsync_ShouldThrowTimeoutExceptionWhenTheTimeoutExpires()
    {
        var task = new TaskCompletionSource<object?>().Task;

        await Assert.ThrowsAsync<TimeoutException>(() => task.WaitAsync(TimeSpan.FromMilliseconds(20)));
    }
#endif

    public record InternalClass(int Value);
}
