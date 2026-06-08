namespace FclEx.Utils;

public partial class OperationTests
{
    [Fact]
    public void Execute_Test()
    {
        var r = Operation.Execute(() => new object());

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.NotEqual(default, r.Elapsed);
    }

    [Fact]
    public void Execute_Error_Test()
    {
        var r = Operation.Execute((Func<object>)(() => throw new SimpleException("")));

        Assert.False(r.IsSuccess);
        Assert.Null(r.Value);
        Assert.NotEqual(default, r.Elapsed);
        Assert.NotNull(r.Exception);
    }

    [Fact]
    public void Execute_OperationResult_UsesOuterElapsed()
    {
        var r = Operation.Execute(() => Operation.Success(TimeSpan.FromHours(1)));

        Assert.True(r.IsSuccess);
        Assert.NotEqual(TimeSpan.FromHours(1), r.Elapsed);
        Assert.True(r.Elapsed < TimeSpan.FromMinutes(1), r.Elapsed.ToString());
    }

    [Fact]
    public void Execute_OperationResult_T_UsesOuterElapsed()
    {
        var r = Operation.Execute(() => Operation.Success(1, TimeSpan.FromHours(1)));

        Assert.True(r.IsSuccess);
        Assert.Equal(1, r.Value);
        Assert.NotEqual(TimeSpan.FromHours(1), r.Elapsed);
        Assert.True(r.Elapsed < TimeSpan.FromMinutes(1), r.Elapsed.ToString());
    }

    [Fact]
    public void Execute_RejectsNullAction()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Execute((Action)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Execute((Func<int>)null!); });
    }

    [Fact]
    public void Action_RejectsNullExecute()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Action<int>((Func<CancellationToken, int>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Action((Action<CancellationToken>)null!); });
    }

    [Fact]
    public void CreateFactories_RejectNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Cancel<int>((Exception)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Error<int>((Exception)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.Error<int>((string)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.ObjectError<string>(null!, "error"); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.ObjectError("value", (string)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = Operation.ObjectError("value", (Exception)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = OperationResult<int>.FromError((string)null!); });
    }

    [Fact]
    public void Cancel_FromNonCancellationException_ReturnsCanceledResult()
    {
        var exception = new SimpleException("stop");

        var result = Operation.Cancel<int>(exception, TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.True(result.IsCanceled());
        Assert.IsType<OperationCanceledException>(result.Exception);
        Assert.Same(exception, result.Exception.InnerException);
        Assert.Equal(TimeSpan.FromSeconds(1), result.Elapsed);
    }

    [Fact]
    public void Cancel_FromOperationCanceledException_PreservesException()
    {
        var exception = new OperationCanceledException("stop");

        var result = Operation.Cancel<int>(exception);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsCanceled());
        Assert.Same(exception, result.Exception);
    }

    [Fact]
    public void ObjectError_RoundTripsAssociatedValue()
    {
        var result = Operation.ObjectError<string, int>("input", new SimpleException("x"), TimeSpan.FromSeconds(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("input", result.FromObjectError<string>());
        Assert.True(result.IsObjectError<string>((value, exception) => value == "input" && exception.Message == "x"));
        Assert.Equal(TimeSpan.FromSeconds(1), result.Elapsed);
    }

    [Fact]
    public void NotImplemented_ReturnsNotImplementedError()
    {
        var result = Operation.NotImplemented<int>();

        Assert.False(result.IsSuccess);
        Assert.IsType<NotImplementedException>(result.Exception);
    }
}
