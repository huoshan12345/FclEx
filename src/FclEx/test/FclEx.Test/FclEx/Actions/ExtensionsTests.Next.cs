namespace FclEx.Actions;

partial class ExtensionsTests
{
    [Fact]
    public async Task Next_OperateResult_T()
    {
        var (success, value, _, _) = await ResultAction.Create(1)
            .Next(Operate.CreateSuccess(1))
            .ExecuteAsync();

        Assert.True(success);
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task Next_OperateResult()
    {
        var (success, _, _, _) = await ResultAction.Create(1)
            .Next(Operate.Success)
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