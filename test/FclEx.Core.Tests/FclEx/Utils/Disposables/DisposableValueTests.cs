namespace FclEx.Utils.Disposables;

public class DisposableValueTests
{
    [Fact]
    public void Dispose_Invokes_Callback_Once_Under_Concurrency()
    {
        var invocationCount = 0;
        var disposable = new DisposableValue<int>(42, _ => Interlocked.Increment(ref invocationCount));

        Parallel.For(0, 100, _ => disposable.Dispose());

        Assert.Equal(1, invocationCount);
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
    }

    [Fact]
    public void Failed_Disposal_Still_Makes_Value_Disposed()
    {
        var expected = new InvalidOperationException("failed");
        var invocationCount = 0;
        var disposable = new DisposableValue<int>(42, _ =>
        {
            invocationCount++;
            throw expected;
        });

        var actual = Assert.Throws<InvalidOperationException>(() => disposable.Dispose());
        disposable.Dispose();

        Assert.Same(expected, actual);
        Assert.Equal(1, invocationCount);
        Assert.Throws<ObjectDisposedException>(() => _ = disposable.Value);
    }
}
