namespace FclEx.Extensions.TaskExtensions;

public class RepeatTests
{
    [Fact]
    public async Task Repeat_Action_InvokesActionForEachRepetition()
    {
        var invocationCount = 0;

        await Task.Repeat(() => Interlocked.Increment(ref invocationCount), 5);

        Assert.Equal(5, invocationCount);
    }

    [Fact]
    public async Task Repeat_Func_InvokesFunctionForEachRepetition()
    {
        var invocationCount = 0;

        var results = await Task.Repeat(() => Interlocked.Increment(ref invocationCount), 5);

        Assert.Equal(5, invocationCount);
        Assert.Equal([1, 2, 3, 4, 5], results.OrderBy(m => m).ToArray());
    }

}
