namespace FclEx.Utils;

partial class OperateResultExtensionsTests
{
    [Fact]
    public async Task Task_OperateResult_T_Next_Func_T_Task_OperateResult_TNext()
    {
        var result = await Operate.ExecuteAsync(() => "x")
            .Next(m => Operate.CreateSuccess(m + "y").ToTask());

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperateResult_T_Next_Func_T_OperateResult_TNext()
    {
        var result = await Operate.ExecuteAsync(() => "x")
            .Next(m => Operate.CreateSuccess(m + "y"));

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperateResult_T_Task_T_Next_Func_T_TNext()
    {
        var result = await Operate.ExecuteAsync(() => "x")
            .Next(m => m + "y");

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperateResult_T_ThrowIfError()
    {
        var task = Operate.ExecuteAsync((Func<int>)(() => throw new InvalidOperationException()))
            .ThrowIfError();

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }
}