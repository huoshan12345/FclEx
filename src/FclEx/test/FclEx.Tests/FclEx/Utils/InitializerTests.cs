namespace FclEx.Utils;

public class InitializerTests
{
    [Fact]
    public async Task Init_ThreadSafe_Test()
    {
        var number = 0;
        var initializer = new Initializer(true);

        var tasks = Enumerable.Range(1, 100000).Select(m =>
            (Func<Task>)(() => Task.Run(() => initializer.Init(() => number++))));
        await tasks.Select(m => m()).WhenAll();
        Assert.Equal(1, number);
    }

    [Fact]
    public void Init_NonThreadSafe_Test()
    {
        var number = 0;
        var initializer = new Initializer(false);
        for (var i = 0; i < 100000; i++)
        {
            initializer.Init(() => number++);
        }
        Assert.Equal(1, number);
    }

    [RetryFact]
    public async Task InitAsync_ThreadSafe_Test()
    {
        var number = 0;
        var initializer = new Initializer(true);

        var tasks = Enumerable.Range(1, 100000).Select(m =>
            (Func<Task>)(() => initializer.InitAsync(async () =>
            {
                await TaskHelper.Delay(1);
                number++;
            })));

        var (successful, ex, elapsed) = await Operate.ExecuteAsync(() => tasks.Select(m => m()).WhenAll(),
            TimeSpan.FromSeconds(3));

        Assert.True(successful);
        Assert.True(elapsed < TimeSpan.FromSeconds(2));
        Assert.Equal(1, number);
    }

    [Fact]
    public async Task InitAsync_NonThreadSafe_Test()
    {
        var number = 0;
        var initializer = new Initializer(false);
        var (successful, _, elapsed) = await Operate.ExecuteAsync(async () =>
        {
            for (var i = 0; i < 100000; i++)
            {
                await initializer.InitAsync(async () =>
                {
                    await TaskHelper.Delay(1);
                    number++;
                });
            }
        }, TimeSpan.FromSeconds(3));

        Assert.True(successful);
        Assert.True(elapsed < TimeSpan.FromSeconds(2), elapsed.ToString());
        Assert.Equal(1, number);
    }
}