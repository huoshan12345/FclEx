using System;

namespace FclEx.Utils;

partial class OperateResultTests
{
    [Fact]
    public void TestExecute()
    {
        var r = Operate.Execute(() => new object());

        Assert.True(r.Success);
        Assert.NotNull(r.Value);
        Assert.NotEqual(default, r.Elapsed);
    }

    [Fact]
    public void TestExecuteError()
    {
        var r = Operate.Execute((Func<object>)(() => throw new SimpleException("")));
        Assert.True(!r.Success);
        Assert.Null(r.Value);
        Assert.NotEqual(default, r.Elapsed);
        Assert.NotNull(r.Exception);
    }
}