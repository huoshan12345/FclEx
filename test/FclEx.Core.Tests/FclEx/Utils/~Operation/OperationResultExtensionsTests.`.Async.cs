// ReSharper disable ConvertToLocalFunction
#pragma warning disable IDE0039
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
    public async Task Then_OperationResult_Func_T_Task_OperationResult_TNext_ThrownException_FaultsTask()
    {
        var ex = new SimpleException("x");
        Func<string, Task<OperationResult<int>>> next = _ => throw ex;

        var actual = await Assert.ThrowsAsync<SimpleException>(() => Operation.Success("x").Then(next));

        Assert.Same(ex, actual);
    }

    [Fact]
    public async Task Then_OperationResult_Func_T_Task_OperationResult_TNext_FaultedTask_FaultsTask()
    {
        var ex = new SimpleException("x");
        Func<string, Task<OperationResult<int>>> next = _ => Task.FromException<OperationResult<int>>(ex);

        var actual = await Assert.ThrowsAsync<SimpleException>(() => Operation.Success("x").Then(next));

        Assert.Same(ex, actual);
    }

    [Fact]
    public async Task Then_TaskOperationResult_Func_T_OperationResult_TNext_ThrownException_FaultsTask()
    {
        var ex = new SimpleException("x");
        Func<string, OperationResult<int>> next = _ => throw ex;

        var actual = await Assert.ThrowsAsync<SimpleException>(() => Task.FromResult(Operation.Success("x")).Then(next));

        Assert.Same(ex, actual);
    }

    [Fact]
    public async Task ThenResult_TaskOperationResult_Func_Result_Task_OperationResult_TNext_ThrownException_FaultsTask()
    {
        var ex = new SimpleException("x");
        Func<OperationResult<string>, Task<OperationResult<int>>> next = _ => throw ex;

        var actual = await Assert.ThrowsAsync<SimpleException>(() => Task.FromResult(Operation.Success("x")).ThenResult(next));

        Assert.Same(ex, actual);
    }

    [Fact]
    public async Task Then_TaskOperationResult_SourceFault_ReturnsError()
    {
        var ex = new SimpleException("x");

        var result = await Task.FromException<OperationResult<string>>(ex)
            .Then(m => Task.FromResult(Operation.Success(m.Length)));

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
    public async Task Then_TaskOperationResult_AddsElapsedFromBothResults()
    {
        var result = await Task.FromResult(Operation.Success(1, TimeSpan.FromSeconds(2)))
            .Then(value => Task.FromResult(Operation.Success(value + 1, TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public async Task Then_TaskOperationResult_FillsDefaultSourceElapsed()
    {
        var result = await Task.Delay(20)
            .Then(() => Operation.Success(1))
            .Then(value => Task.FromResult(Operation.Success(value + 1, TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.True(result.Elapsed > TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task ThenResult_TaskOperationResult_AddsElapsedFromBothResults()
    {
        var result = await Task.FromResult(Operation.Success(1, TimeSpan.FromSeconds(2)))
            .ThenResult(_ => Task.FromResult(Operation.Success("x", TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal("x", result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public async Task Fallback_TaskOperationResult_Success_DoesNotDoubleElapsed()
    {
        var result = await Task.FromResult(Operation.Success(1, TimeSpan.FromSeconds(2)))
            .Fallback(_ => Task.FromResult(Operation.Success(2, TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(2), result.Elapsed);
    }

    [Fact]
    public async Task Fallback_TaskOperationResult_Error_AddsElapsedFromFallback()
    {
        var result = await Task.FromResult(Operation.Error<int>("x", TimeSpan.FromSeconds(2)))
            .Fallback(_ => Task.FromResult(Operation.Success(2, TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public async Task ThenWith_TaskOperationResult_TaskOperationResult_AddsElapsedFromBothResults()
    {
        var result = await Task.FromResult(Operation.Success("a", TimeSpan.FromSeconds(2)))
            .ThenWith(value => Task.FromResult(Operation.Success(value + "b", TimeSpan.FromSeconds(3))));

        Assert.True(result.IsSuccess);
        Assert.Equal(("a", "ab"), result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
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
