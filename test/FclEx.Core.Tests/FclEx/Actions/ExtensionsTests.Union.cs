namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task Union_ChainsTupleValuesAcrossSuccessfulActions()
    {
        var (successful, result, _, elapsed) = await SuccessAction.Create(1, TimeSpan.FromSeconds(1))
            .Union(r => SuccessAction.Create(1 + r, TimeSpan.FromSeconds(2)))
            .Union((a, b) => SuccessAction.Create(1 + a + b, TimeSpan.FromSeconds(3)))
            .ExecuteAsync();

        Assert.True(successful);
        Assert.Equal((1, 2, 4), result);
        Assert.Equal(TimeSpan.FromSeconds(6), elapsed);
    }

    [Fact]
    public async Task Union_WhenFirstActionFails_DoesNotCreateNextAction()
    {
        var invoked = false;

        var (successful, _, ex, elapsed) = await ErrorAction.Create<int>("error", TimeSpan.FromSeconds(4))
            .Union(r =>
            {
                invoked = true;
                return SuccessAction.Create(1 + r);
            })
            .ExecuteAsync();

        Assert.False(successful);
        Assert.False(invoked);
        Assert.Equal("error", ex?.Message);
        Assert.Equal(TimeSpan.FromSeconds(4), elapsed);
    }

    [Fact]
    public async Task Union_WhenMiddleActionFails_DoesNotCreateFinalAction()
    {
        var invoked = false;

        var (successful, _, ex, _) = await SuccessAction.Create(1)
            .Union(r =>
            {
                Assert.Equal(1, r);
                return ErrorAction.Create<int>("error");
            })
            .Union((_, _) =>
            {
                invoked = true;
                return SuccessAction.Create(1);
            })
            .ExecuteAsync();

        Assert.False(successful);
        Assert.False(invoked);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task Union_WhenFinalActionFails_ReturnsFinalError()
    {
        var (successful, _, ex, _) = await SuccessAction.Create(1)
            .Union(r => SuccessAction.Create(1 + r))
            .Union((a, b) =>
            {
                Assert.Equal(1, a);
                Assert.Equal(2, b);
                return ErrorAction.Create<int>("error");
            })
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task Union_WhenMultipleActionsCanFail_ReturnsFirstError()
    {
        var (successful, _, ex, _) = await ErrorAction.Create<int>("error1")
            .Union(_ => ErrorAction.Create<int>("error2"))
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error1", ex?.Message);
    }

    [Fact]
    public async Task Union_WithOperationResultFactory_ReturnsTuple()
    {
        var (successful, result, _, _) = await SuccessAction.Create("a")
            .Union(v => Operation.Success(v + "b"))
            .ExecuteAsync();

        Assert.True(successful);
        Assert.Equal(("a", "ab"), result);
    }

    [Fact]
    public async Task Union_WhenNextActionIsNull_ReturnsNullNextError()
    {
        var (successful, _, ex, _) = await SuccessAction.Create(1)
            .Union<int, string>(_ => null!)
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal(Constants.NullNextError, ex?.Message);
    }

    [Fact]
    public async Task UnionAction_WhenConfiguredToAllowNullNext_ReturnsPreviousValueWithDefaultNext()
    {
        var action = new UnionAction<int, string>(
            SuccessAction.Create(7, TimeSpan.FromSeconds(2)),
            _ => null,
            errorWhenNextNull: false);

        var (successful, result, _, elapsed) = await action.ExecuteAsync();

        Assert.True(successful);
        Assert.Equal((7, default(string)), result);
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
    }

    [Fact]
    public async Task UnionAction_WhenConfiguredToKeepPreviousOnNextError_ReturnsPreviousValueWithDefaultNext()
    {
        var action = new UnionAction<int, string>(
            SuccessAction.Create(7, TimeSpan.FromSeconds(2)),
            _ => ErrorAction.Create<string>("next failed", TimeSpan.FromSeconds(5)),
            prevWhenNextError: true);

        var (successful, result, _, elapsed) = await action.ExecuteAsync();

        Assert.True(successful);
        Assert.Equal((7, default(string)), result);
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
    }
}
