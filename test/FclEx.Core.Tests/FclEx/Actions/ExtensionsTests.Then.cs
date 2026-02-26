namespace FclEx.Actions;

partial class ExtensionsTests
{
    [Fact]
    public async Task Then_OperationResult_T()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(_ => Operation.Success(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task Then_OperationResult()
    {
        var (success, _, _, _) = await SuccessAction.Create(1)
            .Then(_ => Operation.Success())
            .ExecuteAsync();

        Assert.True(success);
    }

    [Fact]
    public async Task Then_Action()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(_ => SuccessAction.Create(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task Then_Func_Action()
    {
        var (success, value, _, _) = await SuccessAction.Create(1)
            .Then(m => SuccessAction.Create(m + 1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }
}