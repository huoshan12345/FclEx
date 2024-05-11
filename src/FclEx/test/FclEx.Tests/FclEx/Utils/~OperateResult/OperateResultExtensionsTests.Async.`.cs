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
    public async Task Task_OperateResult_T_Next_Func_T_Task_OperateResult_TNext_Canceled()
    {
        {
            var result = await Operate.Cancel.ToTask()
                .Next(m => Operate.CreateSuccess(m + "y").ToTask());

            Assert.False(result.Success);
            Assert.True(result.IsCanceled());
        }
        {
            var token = new CancellationToken(true);
            var result = await Task.FromCanceled<OperateResult<string>>(token)
                .Next(m => Operate.CreateSuccess(m + "y").ToTask());

            Assert.False(result.Success);
            Assert.True(result.IsCanceled());
        }
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
        {
            var task = Operate.ExecuteAsync((Func<int>)(() => throw new SimpleException("x")))
                .ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);
        }
        {
            var task = Operate.ExecuteAsync(() => Operate.CreateError("x"))
                .ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);
        }
        {
            var task = Throw().ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);

            static async Task<OperateResult<int>> Throw()
            {
                await Task.Yield();
                throw new SimpleException("x");
            }
        }
    }
}