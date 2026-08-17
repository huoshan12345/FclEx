namespace FclEx.Utils;

public class CompositeDisposableTests
{
    public class Tester : IDisposable
    {
        public int Count { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Count = -1;
        }
    }

    [Fact]
    public void Create_Test()
    {
        var test = new Tester();

        new[] { test }.Select(m =>
        {
            m.Count = 1;
            return m;
        }).Merge();

        Assert.Equal(1, test.Count);
    }

    [Fact]
    public void Dispose_Test()
    {
        var test = new Tester();
        using (new[] { test }.Merge()) { }
        Assert.Equal(-1, test.Count);
    }

    [Fact]
    public void Dispose_CalledMoreThanOnce_DisposesEachItemOnce()
    {
        var disposable = new CountingDisposable();
        var composite = new CompositeDisposable<CountingDisposable>([disposable]);

        composite.Dispose();
        composite.Dispose();

        Assert.Equal(1, disposable.DisposeCount);
    }

    [Fact]
    public void Add_AfterDispose_ThrowsObjectDisposedException()
    {
        var composite = new CompositeDisposable<CountingDisposable>(null);
        composite.Dispose();

        Assert.Throws<ObjectDisposedException>(() => composite.Add(new CountingDisposable()));
    }

    [Fact]
    public void Dispose_WhenItemsThrow_DisposesEveryItemAndAggregatesExceptions()
    {
        var firstException = new InvalidOperationException("first");
        var secondException = new InvalidOperationException("second");
        var first = new CountingDisposable(firstException);
        var middle = new CountingDisposable();
        var last = new CountingDisposable(secondException);
        var composite = new CompositeDisposable<CountingDisposable>([first, middle, last]);

        var exception = Assert.Throws<AggregateException>(() => composite.Dispose());

        Assert.Equal([firstException, secondException], exception.InnerExceptions);
        Assert.Equal(1, first.DisposeCount);
        Assert.Equal(1, middle.DisposeCount);
        Assert.Equal(1, last.DisposeCount);
    }

    private sealed class CountingDisposable(Exception? exception = null) : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            if (exception is not null)
                throw exception;
        }
    }
}
