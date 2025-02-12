namespace FclEx.Actions;

partial class ExtensionsTests
{
    [Fact]
    public async Task Next_OperationResult_T()
    {
        var (success, value, _, _) = await ResultAction.Create(1)
            .Next(Operation.CreateSuccess(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task Next_OperationResult()
    {
        var (success, _, _, _) = await ResultAction.Create(1)
            .Next(Operation.Success())
            .ExecuteAsync();

        Assert.True(success);
    }

    [Fact]
    public async Task Next_Action()
    {
        var (success, value, _, _) = await ResultAction.Create(1)
            .Next(ResultAction.Create(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task Next_Func_Action()
    {
        var (success, value, _, _) = await ResultAction.Create(1)
            .Next(m => ResultAction.Create(m + 1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(2, value);
    }
}