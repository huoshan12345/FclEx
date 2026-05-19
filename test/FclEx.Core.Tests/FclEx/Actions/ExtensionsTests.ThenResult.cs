namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task ThenResult_ReceivesSuccessResult()
    {
        OperationResult<int> observed = default;

        var (success, value, _, _) = await SuccessAction.Create(3)
            .ThenResult<int, string>(r =>
            {
                observed = r;
                return SuccessAction.Create($"value {r.Value}");
            })
            .ExecuteAsync();

        Assert.True(success);
        Assert.True(observed.IsSuccess);
        Assert.Equal(3, observed.Value);
        Assert.Equal("value 3", value);
    }

    [Fact]
    public async Task ThenResult_ReceivesErrorResult()
    {
        OperationResult<int> observed = default;

        var (success, value, _, _) = await ErrorAction.Create<int>("error")
            .ThenResult<int, string>(r =>
            {
                observed = r;
                return SuccessAction.Create($"handled {r.Exception?.Message}");
            })
            .ExecuteAsync();

        Assert.True(success);
        Assert.True(observed.IsError);
        Assert.Equal("error", observed.Exception?.Message);
        Assert.Equal("handled error", value);
    }

    [Fact]
    public async Task ThenResult_WhenNextActionIsNull_ReturnsNullNextError()
    {
        var (success, _, ex, _) = await SuccessAction.Create(1)
            .ThenResult<int, string>((Func<OperationResult<int>, IAction<string>>)(_ => null!))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal(Constants.NullNextError, ex?.Message);
    }

    [Fact]
    public async Task ThenResult_AddsElapsedFromBothActions()
    {
        var (success, value, _, elapsed) = await SuccessAction.Create(1, TimeSpan.FromSeconds(2))
            .ThenResult<int, string>(_ => SuccessAction.Create("next", TimeSpan.FromSeconds(3)))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal("next", value);
        Assert.Equal(TimeSpan.FromSeconds(5), elapsed);
    }

    [Fact]
    public async Task ThenResult_WithValueFactory_ConvertsValueToSuccessResult()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .ThenResult<int, string>((Func<OperationResult<int>, string>)(r => r.IsSuccess ? "ok" : "error"))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal("ok", value);
    }

    [Fact]
    public async Task ThenResult_WithOperationResultFactory_UsesReturnedResult()
    {
        var (success, _, ex, _) = await SuccessAction.Create(1)
            .ThenResult<int, string>(_ => Operation.Error<string>("mapped"))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("mapped", ex?.Message);
    }

    [Fact]
    public async Task ThenResult_WithTaskFactory_AwaitsReturnedValue()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .ThenResult(_ => Task.FromResult("async"))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal("async", value);
    }

    [Fact]
    public async Task ThenResult_WithTaskOperationResultFactory_UsesReturnedResult()
    {
        var (success, _, ex, _) = await SuccessAction.Create(1)
            .ThenResult<int, string>(_ => Task.FromResult(Operation.Error<string>("async error")))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("async error", ex?.Message);
    }

    [Fact]
    public async Task ThenResult_WithActionCallback_ReturnsUnitSuccess()
    {
        var invoked = false;

        var (success, value, _, _) = await SuccessAction.Create(1)
            .ThenResult(r =>
            {
                Assert.True(r.IsSuccess);
                invoked = true;
            })
            .ExecuteAsync();

        Assert.True(success);
        Assert.True(invoked);
        Assert.Equal(Unit.Default, value);
    }

    [Fact]
    public async Task ThenResultIf_ChoosesTrueBranch()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .ThenResultIf(r => r.IsSuccess, _ => SuccessAction.Create(2), _ => SuccessAction.Create(3))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }

    [Fact]
    public async Task ThenResultIf_ChoosesFalseBranch()
    {
        var (success, value, _, _) = await ErrorAction.Create<int>("error")
            .ThenResultIf(r => r.IsSuccess, _ => SuccessAction.Create(2), _ => SuccessAction.Create(3))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(3, value);
    }

    [Fact]
    public async Task ThenResultIf_WithSingleNext_LeavesResultUnchangedWhenConditionIsFalse()
    {
        var (success, _, ex, _) = await ErrorAction.Create<int>("error")
            .ThenResultIf(r => r.IsSuccess, _ => SuccessAction.Create(2))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("error", ex?.Message);
    }
}
