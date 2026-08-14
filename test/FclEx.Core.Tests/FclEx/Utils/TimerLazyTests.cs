namespace FclEx.Utils;

public class TimerLazyTests
{
    [RetryFact(3, 100)]
    public async Task Recreate_Test()
    {
        var span = TimeSpan.FromMilliseconds(100);
        using var lazy = new TimerLazy<DisposableTestModel>(() => new DisposableTestModel(), span);

        Assert.False(lazy.IsValueCreated);
        var value = lazy.Value;
        Assert.NotNull(value);
        Assert.True(lazy.IsValueCreated);

        await Task.Delay(span + TimeSpan.FromMilliseconds(200));
        Assert.False(lazy.IsValueCreated);
        Assert.False(value.IsDisposed);

        var newValue = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(newValue);
        Assert.NotEqual(value, newValue);
    }

    [Fact]
    public async Task Dispose_StopsTimer_AndPreventsFurtherUse()
    {
        var discardedCount = 0;
        var lazy = new TimerLazy<DisposableTestModel>(
            () => new DisposableTestModel(),
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(1),
            discardValueHandler: (_, value) =>
            {
                discardedCount++;
                value.Dispose();
            });
        var value = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();
        await Task.Delay(50);

        Assert.Equal(1, discardedCount);
        Assert.True(value.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
    }
}
