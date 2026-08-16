namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task ThenWith_ChainsTupleValuesAcrossSuccessfulActions()
    {
        var (successful, result, _, elapsed) = await SuccessAction.Create(1, TimeSpan.FromSeconds(1))
            .ThenWith(r => SuccessAction.Create(1 + r, TimeSpan.FromSeconds(2)))
            .ThenWith((a, b) => SuccessAction.Create(1 + a + b, TimeSpan.FromSeconds(3)))
            .ExecuteAsync();

        Assert.True(successful);
        Assert.Equal((1, 2, 4), result);
        Assert.Equal(TimeSpan.FromSeconds(6), elapsed);
    }

    [Fact]
    public async Task ThenWith_WhenFirstActionFails_DoesNotCreateNextAction()
    {
        var invoked = false;

        var (successful, _, ex, elapsed) = await ErrorAction.Create<int>("error", TimeSpan.FromSeconds(4))
            .ThenWith(r =>
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
    public async Task ThenWith_WhenMiddleActionFails_DoesNotCreateFinalAction()
    {
        var invoked = false;

        var (successful, _, ex, _) = await SuccessAction.Create(1)
            .ThenWith(r =>
            {
                Assert.Equal(1, r);
                return ErrorAction.Create<int>("error");
            })
            .ThenWith((_, _) =>
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
    public async Task ThenWith_WhenFinalActionFails_ReturnsFinalErrorAndAccumulatesElapsed()
    {
        var (successful, _, ex, elapsed) = await SuccessAction.Create(1, TimeSpan.FromSeconds(1))
            .ThenWith(r => SuccessAction.Create(1 + r, TimeSpan.FromSeconds(2)))
            .ThenWith((a, b) =>
            {
                Assert.Equal(1, a);
                Assert.Equal(2, b);
                return ErrorAction.Create<int>("error", TimeSpan.FromSeconds(3));
            })
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error", ex?.Message);
        Assert.Equal(TimeSpan.FromSeconds(6), elapsed);
    }

    [Fact]
    public async Task ThenWith_WhenMultipleActionsCanFail_ReturnsFirstError()
    {
        var (successful, _, ex, _) = await ErrorAction.Create<int>("error1")
            .ThenWith(_ => ErrorAction.Create<int>("error2"))
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error1", ex?.Message);
    }

    [Fact]
    public async Task ThenWith_WithOperationResultFactory_ReturnsTuple()
    {
        var (successful, result, _, _) = await SuccessAction.Create("a")
            .ThenWith(v => Operation.Success(v + "b"))
            .ExecuteAsync();

        Assert.True(successful);
        Assert.Equal(("a", "ab"), result);
    }

    [Fact]
    public async Task ThenWith_WhenNextActionIsNull_ReturnsNullNextError()
    {
        var (successful, _, ex, _) = await SuccessAction.Create(1)
            .ThenWith<int, string>((Func<int, IAction<string>>)(_ => null!))
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal(Constants.NullNextError, ex?.Message);
    }
}
