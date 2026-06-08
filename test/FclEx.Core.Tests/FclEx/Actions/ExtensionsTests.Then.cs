namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task Then_WithOperationResult_UsesPreviousValue()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(v => Operation.Success(v + 1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }

    [Fact]
    public async Task Then_WithOperationResultWithoutValue_ReturnsUnit()
    {
        var invoked = false;

        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(_ => Operation.Success())
            .OnValue(_ => invoked = true)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(Unit.Default, value);
        Assert.True(invoked);
    }

    [Fact]
    public async Task Then_WithAction_ExecutesNextAfterSuccess()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(SuccessAction.Create(2))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }

    [Fact]
    public async Task Then_WithFuncAction_UsesPreviousValue()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(m => SuccessAction.Create(m + 1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }

    [Fact]
    public async Task Then_WhenPreviousFails_DoesNotCreateNextAction()
    {
        var invoked = false;

        var (success, _, ex, elapsed) = await ErrorAction.Create<int>("first", TimeSpan.FromSeconds(3))
            .Then<int, string>(_ =>
            {
                invoked = true;
                return SuccessAction.Create("next");
            })
            .ExecuteAsync();

        Assert.False(success);
        Assert.False(invoked);
        Assert.Equal("first", ex?.Message);
        Assert.Equal(TimeSpan.FromSeconds(3), elapsed);
    }

    [Fact]
    public async Task Then_WhenNextActionIsNull_ReturnsNullNextError()
    {
        var (success, _, ex, _) = await SuccessAction.Create(1)
            .Then<int, string>((Func<int, IAction<string>>)(_ => null!))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal(Constants.NullNextError, ex?.Message);
    }

    [Fact]
    public async Task Then_AddsElapsedFromBothActions()
    {
        var (success, value, _, elapsed) = await SuccessAction.Create(1, TimeSpan.FromSeconds(1))
            .Then(_ => SuccessAction.Create("next", TimeSpan.FromSeconds(2)))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal("next", value);
        Assert.Equal(TimeSpan.FromSeconds(3), elapsed);
    }

    [Fact]
    public async Task Then_WithTaskValue_ConvertsValueToSuccessResult()
    {
        var (success, value, _, _) = await SuccessAction.Create(2)
            .Then(v => Task.FromResult(v * 3))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(6, value);
    }

    [Fact]
    public async Task Then_WithTaskOperationResult_UsesReturnedResult()
    {
        var (success, _, ex, _) = await SuccessAction.Create(2)
            .Then(_ => Task.FromResult(Operation.Error<string>("task error")))
            .ExecuteAsync();

        Assert.False(success);
        Assert.Equal("task error", ex?.Message);
    }

    [Fact]
    public async Task ThenIf_ChoosesTrueBranch()
    {
        var (success, value, _, _) = await SuccessAction.Create(4)
            .ThenIf(v => v % 2 == 0, v => SuccessAction.Create(v / 2), v => SuccessAction.Create(v * 2))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }

    [Fact]
    public async Task ThenIf_ChoosesFalseBranch()
    {
        var (success, value, _, _) = await SuccessAction.Create(5)
            .ThenIf(v => v % 2 == 0, v => SuccessAction.Create(v / 2), v => SuccessAction.Create(v * 2))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(10, value);
    }

    [Fact]
    public async Task ThenOptional_WhenFuncReturnsNull_KeepsOriginalValue()
    {
        var (success, value, _, _) = await SuccessAction.Create(5)
            .ThenOptional(_ => null)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Then_RejectsNullNextDelegates()
    {
        var action = SuccessAction.Create(1);

        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, OperationResult<string>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, Task<string>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, Task<OperationResult<string>>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.ThenOptional(null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, IEnumerable<IAction<string>>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, IEnumerable<OperationResult<string>>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = action.Then<int, string>((Func<int, IEnumerable<Task<OperationResult<string>>>>)null!); });
    }

    [Fact]
    public async Task Then_WithActionSequence_CanRunInSeries()
    {
        var order = new List<int>();

        var (success, value, _, _) = await SuccessAction.Create(0)
            .Then(_ => new[]
            {
                Operation.Action<int>(_ =>
                {
                    order.Add(1);
                    return 1;
                }),
                Operation.Action<int>(_ =>
                {
                    order.Add(2);
                    return 2;
                }),
            }, parallel: false)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 1, 2 }, value);
        Assert.Equal(new[] { 1, 2 }, order);
    }

    [Fact]
    public async Task Then_WithOperationResultSequence_ReturnsArray()
    {
        var (success, value, _, _) = await SuccessAction.Create(10)
            .Then(v => new[] { Operation.Success(v + 1), Operation.Success(v + 2) }, parallel: false)
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(new[] { 11, 12 }, value);
    }
}
