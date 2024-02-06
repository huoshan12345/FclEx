namespace FclEx.Extensions.EnumerableExtensions;

public class WhenAnySuccessTests
{
    private readonly ITestOutputHelper _output;

    public WhenAnySuccessTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task WhenAnySuccess_Test()
    {
        var numbers = new[] { 0, 0 };
        var tasks = numbers.Select((m, i) => Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            _output.WriteLine("task " + i);
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
            await Task.Delay(TimeSpan.FromSeconds(1));
            _output.WriteLine("task " + i);
            throw new Exception();
        }));

        var result = await tasks.WhenAnySuccess(m => m > 0, () => 0);
        Assert.Equal(0, result);
    }
}