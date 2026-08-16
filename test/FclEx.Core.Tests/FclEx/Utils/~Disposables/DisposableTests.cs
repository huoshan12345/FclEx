namespace FclEx.Utils;

public class DisposableTests
{
    [Fact]
    public void Dispose_Invokes_Callback_Once_Under_Concurrency()
    {
        var invocationCount = 0;
        var disposable = new Disposable(() => Interlocked.Increment(ref invocationCount));

        Parallel.For(0, 100, _ => disposable.Dispose());

        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public void Dispose_Does_Not_Retry_A_Failed_Callback()
    {
        var invocationCount = 0;
        var expected = new InvalidOperationException("failed");
        var disposable = new Disposable(() =>
        {
            invocationCount++;
            throw expected;
        });

        var actual = Assert.Throws<InvalidOperationException>(() => disposable.Dispose());
        disposable.Dispose();

        Assert.Same(expected, actual);
        Assert.Equal(1, invocationCount);
    }
}
