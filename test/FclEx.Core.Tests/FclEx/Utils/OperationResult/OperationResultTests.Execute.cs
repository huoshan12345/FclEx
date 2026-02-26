namespace FclEx.Utils.OperationResult;

partial class OperationResultTests
{
    [Fact]
    public void TestExecute()
    {
        var r = Operation.Execute(() => new object());

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.NotEqual(default, r.Elapsed);
    }

    [Fact]
    public void TestExecuteError()
    {
        var r = Operation.Execute((Func<object>)(() => throw new SimpleException("")));
        Assert.True(!r.IsSuccess);
        Assert.Null(r.Value);
        Assert.NotEqual(default, r.Elapsed);
        Assert.NotNull(r.Exception);
    }
}