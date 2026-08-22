namespace FclEx.Utils;

public class ExpiringLazyTests
{
    [Fact]
    public void Constructor_NullFactory_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ExpiringLazy<object>(null!, TimeSpan.FromMinutes(1)));

        Assert.Equal("valueFactory", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveTimeToLive_Throws(int milliseconds)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExpiringLazy<object>(() => new object(), TimeSpan.FromMilliseconds(milliseconds)));

        Assert.Equal("timeToLive", exception.ParamName);
    }

    [Fact]
    public void Value_BeforeExpiration_CreatesOnceAndCachesValue()
    {
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<object>(() =>
        {
            invocationCount++;
            return new object();
        }, TimeSpan.FromMinutes(1));

        var first = lazy.Value;
        var second = lazy.Value;

        Assert.Same(first, second);
        Assert.Equal(1, invocationCount);
        Assert.True(lazy.IsValueCreated);
    }

    [Fact]
    public void Value_Null_IsStillCreatedAndCached()
    {
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<string?>(() =>
        {
            invocationCount++;
            return null;
        }, TimeSpan.FromMinutes(1));

        Assert.Null(lazy.Value);
        Assert.Null(lazy.Value);
        Assert.True(lazy.IsValueCreated);
        Assert.Equal(1, invocationCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Value_ValueType_IsCreatedAndCached(int expected)
    {
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<int>(() =>
        {
            invocationCount++;
            return expected;
        }, TimeSpan.FromMinutes(1));

        Assert.Equal(expected, lazy.Value);
        Assert.Equal(expected, lazy.Value);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Value_ConcurrentInitialCallers_RunFactoryOnce()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<object>(() =>
        {
            Interlocked.Increment(ref invocationCount);
            factoryStarted.Set();
            continueFactory.Wait();
            return new object();
        }, TimeSpan.FromMinutes(1));

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() => lazy.Value)).ToArray();
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        continueFactory.Set();
        var values = await Task.WhenAll(tasks);

        Assert.Equal(1, invocationCount);
        Assert.All(values, value => Assert.Same(values[0], value));
    }

    [RetryFact(3, 100)]
    public async Task Value_ConcurrentRefreshCallers_RunFactoryOnce()
    {
        using var refreshStarted = new ManualResetEventSlim();
        using var continueRefresh = new ManualResetEventSlim();
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<TrackedDisposable>(() =>
        {
            if (Interlocked.Increment(ref invocationCount) == 1)
                return first;

            refreshStarted.Set();
            continueRefresh.Wait();
            return second;
        }, TimeSpan.FromMilliseconds(20), value => value.Dispose());
        Assert.Same(first, lazy.Value);
        Thread.Sleep(75);

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() => lazy.Value)).ToArray();
        Assert.True(refreshStarted.Wait(TimeSpan.FromSeconds(5)));
        continueRefresh.Set();
        var values = await Task.WhenAll(tasks);

        Assert.Equal(2, invocationCount);
        Assert.All(values, value => Assert.Same(second, value));
        Assert.True(first.IsDisposed);
    }

    [RetryFact(3, 100)]
    public void Value_AfterExpiration_RefreshesAndReleasesOldValue()
    {
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<TrackedDisposable>(
            () => ++invocationCount == 1 ? first : second,
            TimeSpan.FromMilliseconds(20),
            value => value.Dispose());
        Assert.Same(first, lazy.Value);
        Thread.Sleep(75);

        Assert.Same(second, lazy.Value);

        Assert.True(first.IsDisposed);
        Assert.False(second.IsDisposed);
        Assert.Equal(2, invocationCount);
    }

    [RetryFact(3, 100)]
    public void FailedRefresh_KeepsPreviousValueAndNextAccessRetries()
    {
        var oldValue = new TrackedDisposable();
        var newValue = new TrackedDisposable();
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<TrackedDisposable>(() => ++invocationCount switch
        {
            1 => oldValue,
            2 => throw new InvalidOperationException("Refresh failed."),
            _ => newValue,
        }, TimeSpan.FromMilliseconds(20), value => value.Dispose());
        Assert.Same(oldValue, lazy.Value);
        Thread.Sleep(75);

        Assert.Throws<InvalidOperationException>(() => lazy.Value);
        Assert.False(oldValue.IsDisposed);
        Assert.True(lazy.IsValueCreated);

        Assert.Same(newValue, lazy.Value);
        Assert.True(oldValue.IsDisposed);
    }

    [Fact]
    public void InitialFactoryFailure_IsNotCached()
    {
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<int>(
            () => ++invocationCount == 1 ? throw new InvalidOperationException() : 42,
            TimeSpan.FromMinutes(1));

        Assert.Throws<InvalidOperationException>(() => lazy.Value);

        Assert.Equal(42, lazy.Value);
        Assert.Equal(2, invocationCount);
    }

    [RetryFact(3, 100)]
    public void Refresh_ReturningSameReference_DoesNotReleaseCurrentValue()
    {
        var value = new TrackedDisposable();
        using (var lazy = new ExpiringLazy<TrackedDisposable>(
                   () => value,
                   TimeSpan.FromMilliseconds(20),
                   item => item.Dispose()))
        {
            Assert.Same(value, lazy.Value);
            Thread.Sleep(75);
            Assert.Same(value, lazy.Value);
            Assert.Equal(0, value.DisposeCount);
        }

        Assert.Equal(1, value.DisposeCount);
    }

    [RetryFact(3, 100)]
    public void WithoutReleaseCallback_DoesNotAssumeOwnershipOfDisposableValues()
    {
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        var invocationCount = 0;
        using (var lazy = new ExpiringLazy<TrackedDisposable>(
                   () => ++invocationCount == 1 ? first : second,
                   TimeSpan.FromMilliseconds(20)))
        {
            _ = lazy.Value;
            Thread.Sleep(75);
            _ = lazy.Value;
        }

        Assert.False(first.IsDisposed);
        Assert.False(second.IsDisposed);
    }

    [Fact]
    public void Value_RecursiveFactoryAccess_ThrowsInsteadOfDeadlocking()
    {
        ExpiringLazy<int>? lazy = null;
        lazy = new ExpiringLazy<int>(() => lazy!.Value, TimeSpan.FromMinutes(1));
        using (lazy)
        {
            var exception = Assert.Throws<InvalidOperationException>(() => lazy.Value);

            Assert.Contains("recursively", exception.Message);
        }
    }

    [Fact]
    public async Task Dispose_DuringCreation_ReleasesProducedValueAndFailsAccess()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        var value = new TrackedDisposable();
        var lazy = new ExpiringLazy<TrackedDisposable>(() =>
        {
            factoryStarted.Set();
            continueFactory.Wait();
            return value;
        }, TimeSpan.FromMinutes(1), item => item.Dispose());

        var valueTask = Task.Run(() => lazy.Value);
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        lazy.Dispose();
        continueFactory.Set();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => valueTask);
        Assert.True(value.IsDisposed);
    }

    [Fact]
    public void Dispose_BeforeCreation_DoesNotRunFactoryOrReleaseValue()
    {
        var invocationCount = 0;
        var releaseCount = 0;
        var lazy = new ExpiringLazy<int>(
            () => ++invocationCount,
            TimeSpan.FromMinutes(1),
            _ => releaseCount++);

        lazy.Dispose();

        Assert.Equal(0, invocationCount);
        Assert.Equal(0, releaseCount);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
    }

    [Fact]
    public void Dispose_IsIdempotentReleasesOnceAndPreventsFurtherAccess()
    {
        var value = new TrackedDisposable();
        var lazy = new ExpiringLazy<TrackedDisposable>(
            () => value,
            TimeSpan.FromMinutes(1),
            item => item.Dispose());
        _ = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();

        Assert.Equal(1, value.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
        Assert.Throws<ObjectDisposedException>(() => lazy.IsValueCreated);
    }

    [Fact]
    public void MaximumTimeToLive_DoesNotOverflowWhenCreatingFirstValue()
    {
        var expected = new object();
        using var lazy = new ExpiringLazy<object>(() => expected, TimeSpan.MaxValue);

        Assert.Same(expected, lazy.Value);
    }

    [RetryFact(3, 100)]
    public void ReleaseFailure_AfterRefreshLeavesNewValuePublished()
    {
        var invocationCount = 0;
        using var lazy = new ExpiringLazy<int>(
            () => ++invocationCount,
            TimeSpan.FromMilliseconds(20),
            value =>
            {
                if (value == 1)
                    throw new InvalidOperationException("Release failed.");
            });
        Assert.Equal(1, lazy.Value);
        Thread.Sleep(75);

        Assert.Throws<InvalidOperationException>(() => lazy.Value);

        Assert.Equal(2, lazy.Value);
        Assert.Equal(2, invocationCount);
    }
}
