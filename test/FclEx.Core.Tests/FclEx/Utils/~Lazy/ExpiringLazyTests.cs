namespace FclEx.Utils;

public class ExpiringLazyTests
{
    [Fact]
    public void FailedRefreshKeepsPreviousValueAlive()
    {
        var oldValue = new DisposableTestModel();
        var newValue = new DisposableTestModel();
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<DisposableTestModel>(() => ++invocationCount switch
        {
            1 => oldValue,
            2 => throw new InvalidOperationException("Refresh failed."),
            _ => newValue,
        }, TimeSpan.FromMilliseconds(10));

        Assert.Same(oldValue, lazy.Value);
        Thread.Sleep(30);

        Assert.Throws<InvalidOperationException>(() => lazy.Value);
        Assert.False(oldValue.IsDisposed);

        Assert.Same(newValue, lazy.Value);
        Assert.True(oldValue.IsDisposed);
    }

    [Fact]
    public void DisposeIsIdempotentAndPreventsFurtherAccess()
    {
        var value = new DisposableTestModel();
        var lazy = new ExpiringLazy<DisposableTestModel>(() => value, TimeSpan.FromMinutes(1));
        _ = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();

        Assert.True(value.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
    }

    [Fact]
    public void MaximumLifetime_DoesNotOverflowWhenCreatingTheFirstValue()
    {
        var expected = new object();
        using var lazy = new ExpiringLazy<object>(() => expected, TimeSpan.MaxValue);

        Assert.Same(expected, lazy.Value);
    }
}
