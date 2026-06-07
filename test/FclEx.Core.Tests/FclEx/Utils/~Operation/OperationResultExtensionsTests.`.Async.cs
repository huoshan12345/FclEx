using FclEx.Actions;

namespace FclEx.Utils;

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
    public async Task Then_OperationResult_Func_T_Task_OperationResult_TNext_ThrownException_ReturnsError()
    {
        var ex = new SimpleException("x");
        Func<string, Task<OperationResult<int>>> next = _ => throw ex;

        var result = await Operation.Success("x").Then(next);

        Assert.False(result.IsSuccess);
        Assert.Same(ex, result.Exception);
    }

    [Fact]
    public async Task Then_OperationResult_Func_T_Task_OperationResult_TNext_FaultedTask_ReturnsError()
    {
        var ex = new SimpleException("x");
        Func<string, Task<OperationResult<int>>> next = _ => Task.FromException<OperationResult<int>>(ex);

        var result = await Operation.Success("x").Then(next);

        Assert.False(result.IsSuccess);
        Assert.Same(ex, result.Exception);
    }

    [Fact]
    public async Task Then_TaskOperationResult_Func_T_OperationResult_TNext_ThrownException_ReturnsError()
    {
        var ex = new SimpleException("x");
        Func<string, OperationResult<int>> next = _ => throw ex;

        var result = await Task.FromResult(Operation.Success("x")).Then(next);

        Assert.False(result.IsSuccess);
        Assert.Same(ex, result.Exception);
    }

    [Fact]
    public async Task ThenResult_TaskOperationResult_Func_Result_Task_OperationResult_TNext_ThrownException_ReturnsError()
    {
        var ex = new SimpleException("x");
        Func<OperationResult<string>, Task<OperationResult<int>>> next = _ => throw ex;

        var result = await Task.FromResult(Operation.Success("x")).ThenResult(next);

        Assert.False(result.IsSuccess);
        Assert.Same(ex, result.Exception);
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
            .MapValue(m => m + "y");

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

    [Fact]
    public async Task WhenResult_InvokesCallbackOnlyWhenConditionMatches()
    {
        var called = 0;

        var result = await Task.FromResult(Operation.Success(1))
            .WhenResult(r => r.Value == 1, _ =>
            {
                called++;
                return Task.CompletedTask;
            })
            .WhenResult(r => r.Value == 2, _ =>
            {
                called++;
                return Task.CompletedTask;
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, called);
    }

    [Fact]
    public async Task OnFaulted_DoesNotRunForCanceledResult()
    {
        var called = false;

        var result = await Task.FromResult(Operation.Cancel<int>())
            .OnFaulted(_ => called = true);

        Assert.True(result.IsCanceled());
        Assert.False(called);
    }

    [Fact]
    public async Task OnFaulted_RunsForNonCanceledError()
    {
        var called = false;

        var result = await Task.FromResult(Operation.Error<int>("x"))
            .OnFaulted(_ => called = true);

        Assert.True(result.IsFaulted());
        Assert.True(called);
    }
}
