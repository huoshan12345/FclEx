namespace FclEx.Extensions.EnumerableExtensions;

public class WhenAnySuccessTests
{
    [Fact]
    public async Task WhenAnySuccess_Test()
    {
        var numbers = new[] { 0, 0 };
        var tasks = numbers.Select((m, i) => Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            return Interlocked.Increment(ref numbers[i]);
        }));
        var result = await tasks.WhenAnySuccess(m => m > 0, () => 0);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task WhenAnySuccess_DefaultValue_Test()
    {
        var tasks = Enumerable.Range(1, 3).Select((m, i) => Task.Run<int>(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new Exception();
        }));

        var result = await tasks.WhenAnySuccess(m => m > 0, () => 0);
        Assert.Equal(0, result);
    }
}