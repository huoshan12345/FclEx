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
}
