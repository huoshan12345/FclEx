namespace FclEx.Extensions.EnumerableExtensions;

public class WhenAnySuccessTests
{
    [Fact]
    public async Task WhenAnySuccess_Test()
    {
        var numbers = new[] { 0, 0 };
        var tasks = numbers.Select((m, i) => Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            return Interlocked.Increment(ref numbers[i]);
        }));
        var result = await tasks.WhenAnySuccess(m => m > 0, () => 0);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task WhenAnySuccess_DefaultValue_Test()
    {
        var tasks = Enumerable.Range(1, 3).Select((m, i) => Task.Run<int>(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new Exception();
        }));

        var result = await tasks.WhenAnySuccess(m => m > 0, () => 0);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Empty_Sequence_Uses_Default_Result_Factory()
    {
        var result = await Array.Empty<Task<int>>().WhenAnySuccess(_ => true, () => 42);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Empty_Sequence_Without_Default_Result_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Array.Empty<Task<int>>().WhenAnySuccess(_ => true));
    }

    [Fact]
    public async Task NonGeneric_Empty_Sequence_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Array.Empty<Task>().WhenAnySuccess());
    }

    [Fact]
    public async Task Success_After_Failed_Tasks_Is_Returned()
    {
        var successfulTask = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int>[] tasks =
        [
            Task.FromException<int>(new InvalidOperationException("failed")),
            successfulTask.Task,
        ];
        var aggregation = tasks.WhenAnySuccess(value => value > 0);

        successfulTask.SetResult(42);

        Assert.Equal(42, await aggregation);
    }

    [Fact]
    public async Task NonGeneric_Success_After_Failed_Task_Completes()
    {
        var successfulTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task[] tasks =
        [
            Task.FromException(new InvalidOperationException("failed")),
            successfulTask.Task,
        ];
        var aggregation = tasks.WhenAnySuccess();

        successfulTask.SetResult(true);

        await aggregation;
    }

    [Fact]
    public async Task Predicate_Exception_Faults_Returned_Task()
    {
        var expected = new InvalidOperationException("predicate");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new[] { Task.FromResult(1) }.WhenAnySuccess<int>(_ => throw expected));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task Default_Result_Factory_Exception_Faults_Returned_Task()
    {
        var expected = new InvalidOperationException("default result");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Array.Empty<Task<int>>().WhenAnySuccess(_ => true, () => throw expected));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task WhenAllOrError_Empty_Sequence_Completes_Successfully()
    {
        await Array.Empty<Task>().WhenAllOrError();
    }

    [Fact]
    public async Task WhenAllOrError_Propagates_Error_Without_Waiting_For_Pending_Task()
    {
        var pendingTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var expected = new InvalidOperationException("failed");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new Task[] { pendingTask.Task, Task.FromException(expected) }.WhenAllOrError());

        Assert.Same(expected, actual);
        Assert.False(pendingTask.Task.IsCompleted);
    }

    [Fact]
    public async Task WhenAllOrError_Propagates_Cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new[] { Task.FromCanceled(cancellation.Token) }.WhenAllOrError());
    }
}
