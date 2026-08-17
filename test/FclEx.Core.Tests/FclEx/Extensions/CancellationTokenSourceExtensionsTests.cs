namespace FclEx.Extensions;

public class CancellationTokenSourceExtensionsTests
{
    [Fact]
    public void TryCancel_PropagatesCancellationCallbackFailures()
    {
        using var source = new CancellationTokenSource();
        source.Token.Register(static () => throw new InvalidOperationException("callback failure"));

        Assert.Throws<AggregateException>(() => source.TryCancel());
    }

    [Fact]
    public void TryCancel_IgnoresAnAlreadyDisposedSource()
    {
        var source = new CancellationTokenSource();
        source.Dispose();

        source.TryCancel();
    }
}
