namespace FclEx.Utils;

partial class OperationResultExtensionsTests
{
    [Fact]
    public async Task Task_OperationResult_T_Next_Func_T_Task_OperationResult_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Next(m => Operation.CreateSuccess(m + "y").ToTask());

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperationResult_T_Next_Func_T_Task_OperationResult_TNext_Canceled()
    {
        {
            var result = await Operation.Cancel.ToTask()
                .Next(m => Operation.CreateSuccess(m + "y").ToTask());

            Assert.False(result.Success);
            Assert.True(result.IsCanceled());
        }
        {
            var token = new CancellationToken(true);
            var result = await Task.FromCanceled<OperationResult<string>>(token)
                .Next(m => Operation.CreateSuccess(m + "y").ToTask());

            Assert.False(result.Success);
            Assert.True(result.IsCanceled());
        }
    }

    [Fact]
    public async Task Task_OperationResult_T_Next_Func_T_OperationResult_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Next(m => Operation.CreateSuccess(m + "y"));

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperationResult_T_Task_T_Next_Func_T_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Next(m => m + "y");

        Assert.True(result.Success);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Task_OperationResult_T_ThrowIfError()
    {
        {
            var task = Operation.ExecuteAsync((Func<int>)(() => throw new SimpleException("x")))
                .ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);
        }
        {
            var task = Operation.ExecuteAsync(() => Operation.CreateError("x"))
                .ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);
        }
        {
            var task = Throw().ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);

            static async Task<OperationResult<int>> Throw()
            {
                await Task.Yield();
                throw new SimpleException("x");
            }
        }
    }
}