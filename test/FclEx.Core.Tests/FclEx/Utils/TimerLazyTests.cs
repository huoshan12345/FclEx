namespace FclEx.Utils;

public class TimerLazyTests
{
    [RetryFact]
    public async Task Recreate_Test()
    {
        var span = TimeSpan.FromMilliseconds(100);

        var lazy = new TimerLazy<ReLazyTests.DisposableTester>(() => new ReLazyTests.DisposableTester(), span);
        Assert.False(lazy.IsValueCreated);
        var value = lazy.Value;
        Assert.NotNull(value);
        Assert.True(lazy.IsValueCreated);

        await Task.Delay(span + TimeSpan.FromMilliseconds(100));
        Assert.False(lazy.IsValueCreated);
        Assert.False(value.IsDisposed);

        var newValue = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(newValue);
        Assert.NotEqual(value, newValue);

        lazy.Dispose();
    }
}