namespace FclEx.Extensions.TaskExtensions;

public class ContinueTests
{
    [Fact]
    public async Task Catch_ShouldReturnResult_WhenTaskSucceeds()
    {
        var task = Task.FromResult(42);
        var result = await task.Catch(ex => Task.FromResult(-1));
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Catch_ShouldInvokeAction_WhenTaskFaults()
    {
        var task = Task.FromException<int>(new InvalidOperationException("Test exception"));
        var result = await task.Catch(ex => Task.FromResult(99));
        Assert.Equal(99, result);
    }

    [Fact]
    public async Task Catch_ShouldInvokeAction_WhenTaskIsCanceled()
    {
        var cts = new CancellationTokenSource();
        var task = Task.Run(() => {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
            return 0;
        }, cts.Token);

        var result = await task.Catch(ex => Task.FromResult(88));
        Assert.Equal(88, result);
    }

    [Fact]
    public async Task Catch_ShouldRethrowException_WhenActionThrows()
    {
        var task = Task.FromException<int>(new InvalidOperationException("Test exception"));

        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await task.Catch<int>(ex => throw new ApplicationException("Action failed")));
    }

    [Fact]
    public async Task Catch_ShouldHandleCancellationAndRecover_WhenActionResolves()
    {
        var cts = new CancellationTokenSource();
        var task = Task.Run(() => {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
            return 0;
        }, cts.Token);

        var result = await task.Catch(ex => Task.FromResult(77));
        Assert.Equal(77, result);
    }
}