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

        Assert.False(source.TryCancel());
    }

    [Fact]
    public void TryCancel_ReturnsTrueOnlyForTheFirstObservedCancellation()
    {
        using var source = new CancellationTokenSource();

        Assert.True(source.TryCancel());
        Assert.False(source.TryCancel());
    }
}
