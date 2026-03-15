using FclEx.Actions;

namespace FclEx.Utils.OperationResult;

partial class OperationResultExtensionsTests
{
    [Fact]
    public async Task Then_Func_T_Task_OperationResult_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Then(m => Task.FromResult(Operation.Success(m + "y")));

        Assert.True(result.IsSuccess);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Then_Func_T_Task_OperationResult_TNext_Canceled()
    {
        {
            var result = await Task.FromResult(Operation.Cancel())
                .Then(m => Operation.Success(m + "y"));

            Assert.False(result.IsSuccess);
            Assert.True(result.IsCanceled());
        }
        {
            var token = new CancellationToken(true);
            var result = await Task.FromCanceled<OperationResult<string>>(token)
                .Then(m => Task.FromResult(Operation.Success(m + "y")));

            Assert.False(result.IsSuccess);
            Assert.True(result.IsCanceled());
        }
    }

    [Fact]
    public async Task Then_T_Next_Func_T_OperationResult_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Then(m => Operation.Success(m + "y"));

        Assert.True(result.IsSuccess);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task Then_T_Task_T_Next_Func_T_TNext()
    {
        var result = await Operation.ExecuteAsync(() => "x")
            .Then(m => m + "y");

        Assert.True(result.IsSuccess);
        Assert.Equal("xy", result.Value);
    }

    [Fact]
    public async Task ThrowIfError_Test()
    {
        {
            var task = Operation.ExecuteAsync((Func<int>)(() => throw new SimpleException("x")))
                .ThrowIfError();

            var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
            Assert.Equal("x", ex.Message);
        }
        {
            var task = Operation.ExecuteAsync(() => Operation.Error("x"))
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

    [Fact]
    public async Task Then_Unit_T_Task_Unit_Next_Func_Unit_TNext()
    {
        var result = await Operation.Action(t => Operation.ExecuteAsync(() => "x"))
            .Then(m => Operation.ExecuteAsync(() => Unit.Default).Then(_ => m))
            .Then(m => Operation.SuccessAction(m + "y"))
            .ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("xy", result.Value);
    }
}