namespace Xunit.Testing;

[Flags]
public enum Options
{
    None,
    AllowRepeatedDisposals = 0x2,
    AllowRepeatedMoveNext = 0x4,
}

public static class TestingSequence
{
    public static TestingSequence<T> Of<T>(params T[] elements) =>
        new(elements, Options.None, maxEnumerations: 1);

    public static TestingSequence<T> Of<T>(Options options, params T[] elements) =>
        elements.AsTestingSequence(options, maxEnumerations: 1);

    public static TestingSequence<T> AsTestingSequence<T>(this IEnumerable<T> source,
                                                            Options options = Options.None,
                                                            int maxEnumerations = 1) =>
        source != null
        ? new TestingSequence<T>(source, options, maxEnumerations)
        : throw new ArgumentNullException(nameof(source));
}

/// <summary>
/// Sequence that asserts whether its iterator has been disposed
/// when it is disposed itself and also whether GetEnumerator() is
/// called exactly once or not.
/// </summary>
public sealed class TestingSequence<T> : IEnumerable<T>, IDisposable
{
    internal const string ExpectedDisposal = "Expected sequence to be disposed.";
    internal const string TooManyEnumerations = "Sequence should not be enumerated more than expected.";
    internal const string TooManyDisposals = "Sequence should not be disposed more than once per enumeration.";
    internal const string SimultaneousEnumerations = "Sequence should not have simultaneous enumeration.";
    internal const string MoveNextPostDisposal = "LINQ operators should not call MoveNext() on a disposed sequence.";
    internal const string MoveNextPostEnumeration = "LINQ operators should not continue iterating a sequence that has terminated.";
    internal const string CurrentPostDisposal = "LINQ operators should not attempt to get the Current value on a disposed sequence.";
    internal const string CurrentPostEnumeration = "LINQ operators should not attempt to get the Current value on a completed sequence.";

    private readonly IEnumerable<T> _sequence;
    private readonly Options _options;
    private readonly int _maxEnumerations;

    private int _disposedCount;
    private int _enumerationCount;

    internal TestingSequence(IEnumerable<T> sequence, Options options, int maxEnumerations)
    {
        _sequence = sequence;
        _maxEnumerations = maxEnumerations;
        _options = options;
    }

    public int MoveNextCallCount { get; private set; }
    public bool IsDisposed => _enumerationCount > 0 && _disposedCount == _enumerationCount;

    void IDisposable.Dispose()
    {
        if (_enumerationCount > 0)
            Assert.True(_enumerationCount == _disposedCount, ExpectedDisposal);
    }

    public IEnumerator<T> GetEnumerator()
    {
        Assert.True(_enumerationCount < (_maxEnumerations), TooManyEnumerations);
        Assert.True(_enumerationCount == (_disposedCount), SimultaneousEnumerations);
        _enumerationCount++;

        // ReSharper disable once GenericEnumeratorNotDisposed
        var enumerator = _sequence.GetEnumerator().AsWatchable();
        var disposed = false;
        enumerator.Disposed += delegate
        {
            if (!disposed)
            {
                _disposedCount++;
                disposed = true;
            }
            else if (!_options.HasFlag(Options.AllowRepeatedDisposals))
            {
                Assert.Fail(TooManyDisposals);
            }
        };

        var ended = false;
        enumerator.MoveNextCalled += (_, moved) =>
        {
            Assert.False(disposed, MoveNextPostDisposal);
            if (!_options.HasFlag(Options.AllowRepeatedMoveNext))
                Assert.False(ended, MoveNextPostEnumeration);

            ended = !moved;
            MoveNextCallCount++;
        };

        enumerator.GetCurrentCalled += delegate
        {
            Assert.False(disposed, CurrentPostDisposal);
            Assert.False(ended, CurrentPostEnumeration);
        };

        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
