namespace FclEx.Actions;

public partial class ExtensionsTests
{
    [Fact]
    public async Task Union_Success_Test()
    {
        var (successful, result, _, _) = await Operation.Action(t => Task.FromResult(1))
            .Union(r => Operation.Action(t => Task.FromResult(1 + r)))
            .Union((a, b) => Operation.Action(t => Task.FromResult(1 + a + b)))
            .ExecuteAsync();

        Assert.True(successful);
        Assert.Equal((1, 2, 4), result);
    }

    [Fact]
    public async Task Union_Error_Begin_Test()
    {
        var flag = false;
        var (successful, _, ex, _) = await Operation.Action(t => Operation.Error<int>("error"))
            .Union(r => Operation.Action(t =>
            {
                flag = true;
                return Task.FromResult(1 + r);
            }))
            .ExecuteAsync();

        Assert.False(flag);
        Assert.False(successful);
        Assert.Equal("error", ex?.Message);
    }


    [Fact]
    public async Task Union_Error_Middle_Test()
    {
        var flag = false;
        var (successful, _, ex, _) = await Operation.Action(t => Task.FromResult(1))
            .Union(r =>
            {
                Assert.Equal(1, r);
                return Operation.Action(t => Operation.Error<int>("error"));
            })
            .Union((a, b) =>
            {
                flag = true;
                return Operation.Action(t => Task.FromResult(1 + a + b));
            })
            .ExecuteAsync();

        Assert.False(flag);
        Assert.False(successful);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task Union_Error_End_Test()
    {
        var (successful, _, ex, _) = await Operation.Action(t => Task.FromResult(1))
            .Union(r => Operation.Action(t => Task.FromResult(1 + r)))
            .Union((a, b) =>
            {
                Assert.Equal(1, a);
                Assert.Equal(2, b);
                return Operation.Action(t => Operation.Error<int>("error"));
            })
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error", ex?.Message);
    }

    [Fact]
    public async Task Union_Errors_Test()
    {
        var (successful, _, ex, _) = await Operation.Action(t => Operation.Error<int>("error1"))
            .Union(r => Operation.Action(t => Operation.Error<int>("error2")))
            .ExecuteAsync();

        Assert.False(successful);
        Assert.Equal("error1", ex?.Message);
    }
}