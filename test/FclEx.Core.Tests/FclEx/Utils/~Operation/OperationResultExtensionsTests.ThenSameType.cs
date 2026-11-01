namespace FclEx.Utils;

public partial class OperationResultExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_ReturnsNextResultAndAddsElapsed(bool asynchronous)
    {
        var source = Task.FromResult(Operation.Success(2, TimeSpan.FromSeconds(3)));
        var result = await ChainSameType(source, asynchronous,
            value => Operation.Success(value + 5, TimeSpan.FromSeconds(4)));

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(7), result.Elapsed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_NextErrorPreservesExceptionAndAddsElapsed(bool asynchronous)
    {
        var exception = new InvalidOperationException("next failed");
        var source = Task.FromResult(Operation.Success(2, TimeSpan.FromSeconds(3)));
        var result = await ChainSameType(source, asynchronous,
            _ => Operation.Error<int>(exception, TimeSpan.FromSeconds(4)));

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(TimeSpan.FromSeconds(7), result.Elapsed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_SourceErrorSkipsCallbackAndPreservesElapsed(bool asynchronous)
    {
        var exception = new InvalidOperationException("source failed");
        var source = Task.FromResult(Operation.Error<int>(exception, TimeSpan.FromSeconds(3)));
        var called = false;
        var result = await ChainSameType(source, asynchronous, value =>
        {
            called = true;
            return Operation.Success(value);
        });

        Assert.False(called);
        Assert.Same(exception, result.Exception);
        Assert.Equal(TimeSpan.FromSeconds(3), result.Elapsed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_FaultedSourceTaskBecomesErrorAndSkipsCallback(bool asynchronous)
    {
        var exception = new InvalidOperationException("source fault");
        var result = await ChainSameType(Task.FromException<OperationResult<int>>(exception), asynchronous,
            _ => throw new InvalidOperationException("Callback must not run."));

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_CanceledSourceTaskBecomesCanceledResult(bool asynchronous)
    {
        var token = new CancellationToken(true);
        var result = await ChainSameType(Task.FromCanceled<OperationResult<int>>(token), asynchronous,
            _ => throw new InvalidOperationException("Callback must not run."));

        Assert.True(result.IsCanceled());
        Assert.Equal(token, Assert.IsAssignableFrom<OperationCanceledException>(result.Exception).CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_CallbackExceptionFaultsReturnedTask(bool asynchronous)
    {
        var exception = new InvalidOperationException("callback failed");
        var task = ChainSameType(Task.FromResult(Operation.Success(2)), asynchronous, _ => throw exception);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Same(exception, actual);
        Assert.True(task.IsFaulted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Then_SameType_NullCallbackIsRejectedSynchronously(bool asynchronous)
    {
        var source = Task.FromResult(Operation.Success(2));
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = asynchronous
                ? source.Then<int>((Func<int, Task<OperationResult<int>>>)null!)
                : source.Then<int>((Func<int, OperationResult<int>>)null!);
        });

        Assert.Equal("next", exception.ParamName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Then_SameType_NullSourceTaskFaultsReturnedTask(bool asynchronous)
    {
        var task = ChainSameType(null!, asynchronous, value => Operation.Success(value));
        await Assert.ThrowsAsync<ArgumentNullException>(() => task);
    }

    [Fact]
    public async Task Then_SameType_AwaitsSourceAndAsynchronousCallback()
    {
        var source = new TaskCompletionSource<OperationResult<int>>();
        var next = new TaskCompletionSource<OperationResult<int>>();
        var called = false;
        var task = source.Task.Then<int>(value =>
        {
            called = true;
            Assert.Equal(2, value);
            return next.Task;
        });

        Assert.False(called);
        Assert.False(task.IsCompleted);
        source.SetResult(Operation.Success(2, TimeSpan.FromSeconds(3)));
        Assert.False(task.IsCompleted);
        next.SetResult(Operation.Success(7, TimeSpan.FromSeconds(4)));

        var result = await task;
        Assert.True(called);
        Assert.Equal(7, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(7), result.Elapsed);
    }

    [Fact]
    public async Task Then_SameType_FaultedNextTaskPropagatesException()
    {
        var exception = new InvalidOperationException("next task failed");
        var task = Task.FromResult(Operation.Success(2))
            .Then<int>(_ => Task.FromException<OperationResult<int>>(exception));

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Same(exception, actual);
        Assert.True(task.IsFaulted);
    }

    [Fact]
    public async Task Then_SameType_CanceledNextTaskCancelsReturnedTask()
    {
        var token = new CancellationToken(true);
        var task = Task.FromResult(Operation.Success(2))
            .Then<int>(_ => Task.FromCanceled<OperationResult<int>>(token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal(token, exception.CancellationToken);
        Assert.True(task.IsCanceled);
    }

    [Fact]
    public async Task Then_SameType_ExplicitTypeArgumentAllowsImplicitResultConversions()
    {
        var source = Task.FromResult(Operation.Success(2, TimeSpan.FromSeconds(3)));
        var synchronous = await source.Then<int>(_ => "sync error");
        var asynchronous = await source.Then<int>(async _ =>
        {
            await Task.Yield();
            return "async error";
        });

        Assert.True(synchronous.IsError);
        Assert.Equal("sync error", synchronous.Exception.Message);
        Assert.True(asynchronous.IsError);
        Assert.Equal("async error", asynchronous.Exception.Message);
    }

    private static Task<OperationResult<int>> ChainSameType(
        Task<OperationResult<int>> source, bool asynchronous, Func<int, OperationResult<int>> next)
    {
        return asynchronous
            ? source.Then<int>(async value =>
            {
                await Task.Yield();
                return next(value);
            })
            : source.Then<int>(next);
    }
}
