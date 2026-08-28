namespace FclEx.Actions;

public class PipelineActionTests
{
    [Fact]
    public async Task ExecuteAsync_WhenExecuteActionSucceeds_ReturnsValue()
    {
        var action = new TestPipelineAction(_ => Operation.Success(5, TimeSpan.FromSeconds(1)));

        var (success, value, _, elapsed) = await action.ExecuteAsync();

        Assert.True(success);
        Assert.Equal(5, value);
        Assert.True(elapsed >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecuteActionReturnsError_CallsHandleError()
    {
        var original = new InvalidOperationException("original");
        var replacement = new ApplicationException("handled");
        var action = new TestPipelineAction(
            _ => Operation.Error<int>(original),
            handleError: e =>
            {
                Assert.Same(original, e);
                return Operation.Error<int>(replacement);
            });

        var (success, _, ex, _) = await action.ExecuteAsync();

        Assert.False(success);
        Assert.Same(replacement, ex);
        Assert.Equal(1, action.HandleErrorCount);
        Assert.Equal(0, action.HandleCancellationCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecuteActionThrowsError_CallsHandleError()
    {
        var action = new TestPipelineAction(_ => throw new InvalidOperationException("boom"));

        var (success, _, ex, _) = await action.ExecuteAsync();

        Assert.False(success);
        Assert.Equal("handled error: boom", ex?.Message);
        Assert.Equal(1, action.HandleErrorCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecuteActionReturnsCancellation_CallsHandleCancellation()
    {
        var action = new TestPipelineAction(_ => Operation.Cancel<int>(new OperationCanceledException("cancel")));

        var (success, _, ex, _) = await action.ExecuteAsync();

        Assert.False(success);
        Assert.IsType<OperationCanceledException>(ex);
        Assert.Equal("handled cancellation: cancel", ex?.Message);
        Assert.Equal(0, action.HandleErrorCount);
        Assert.Equal(1, action.HandleCancellationCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecuteActionThrowsCancellation_CallsHandleCancellation()
    {
        var action = new TestPipelineAction(_ => throw new OperationCanceledException("cancel"));
        var (success, _, ex, _) = await action.ExecuteAsync();

        Assert.False(success);
        Assert.IsType<OperationCanceledException>(ex);
        Assert.Equal("handled cancellation: cancel", ex?.Message);
        Assert.Equal(0, action.HandleErrorCount);
        Assert.Equal(1, action.HandleCancellationCount);
    }

    [Fact]
    public void GetName_DefaultsToTypeLongName()
    {
        var action = new TestPipelineAction(_ => Operation.Success(1));
        var type = typeof(TestPipelineAction);
        Assert.Equal(type.DeclaringType?.FullName + "." + type.Name, action.GetName());
    }

    private sealed class TestPipelineAction(
        Func<CancellationToken, OperationResult<int>> execute,
        Func<Exception, OperationResult<int>>? handleError = null,
        Func<Exception, OperationResult<int>>? handleCancellation = null) : PipelineAction<int>
    {
        public int HandleErrorCount { get; private set; }

        public int HandleCancellationCount { get; private set; }

        public override Task<OperationResult<int>> ExecuteCoreAsync(CancellationToken token = default)
        {
            return execute(token);
        }

        public override Task<OperationResult<int>> HandleErrorAsync(Exception ex)
        {
            HandleErrorCount++;
            return handleError?.Invoke(ex) ?? Operation.Error<int>($"handled error: {ex.Message}");
        }

        public override Task<OperationResult<int>> HandleCancellationAsync(Exception ex)
        {
            HandleCancellationCount++;
            return handleCancellation?.Invoke(ex) ?? Operation.Cancel<int>(new OperationCanceledException($"handled cancellation: {ex.Message}"));
        }
    }
}
