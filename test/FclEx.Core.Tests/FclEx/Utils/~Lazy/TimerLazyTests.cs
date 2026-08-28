namespace FclEx.Utils;

[TestClass(DisableParallelization = true)]
public class TimerLazyTests
{
    [Fact]
    public void Constructor_NullFactory_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TimerLazy<object>(null!, TimeSpan.FromMinutes(1)));

        Assert.Equal("valueFactory", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PeriodConstructor_NonPositivePeriod_Throws(int milliseconds)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TimerLazy<object>(() => new object(), TimeSpan.FromMilliseconds(milliseconds)));

        Assert.Equal("period", exception.ParamName);
    }

    [RetryFact]
    public async Task PeriodConstructor_DoesNotResetBeforeFirstPeriod()
    {
        var period = TimeSpan.FromMilliseconds(300);
        using var lazy = new TimerLazy<object>(() => new object(), period);
        _ = lazy.Value;

        await Task.Delay(75);

        Assert.True(lazy.IsValueCreated);
        await LazyTestHelper.WaitUntilAsync(() => lazy.IsValueCreated == false, TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Timer_DoesNotCreateValue()
    {
        var invocationCount = 0;
        using var lazy = new TimerLazy<int>(
            () => Interlocked.Increment(ref invocationCount),
            TimeSpan.FromMilliseconds(20));

        await Task.Delay(100);

        Assert.False(lazy.IsValueCreated);
        Assert.Equal(0, invocationCount);
    }

    [RetryFact]
    public async Task ExplicitDueTime_ResetsCreatedValue()
    {
        var first = new TrackedDisposable();
        using var lazy = new TimerLazy<TrackedDisposable>(
            () => first,
            TimeSpan.FromMilliseconds(50),
            Timeout.InfiniteTimeSpan,
            value => value.Dispose());
        _ = lazy.Value;

        await LazyTestHelper.WaitUntilAsync(() => lazy.IsValueCreated == false, TimeSpan.FromSeconds(3));

        Assert.True(first.IsDisposed);
        Assert.Null(lazy.LastResetException);
    }

    [RetryFact]
    public async Task FaultedGeneration_IsResetByTimerAndCanRetry()
    {
        var invocationCount = 0;
        using var lazy = new TimerLazy<int>(
            () => Interlocked.Increment(ref invocationCount) == 1
                ? throw new InvalidOperationException("Factory failed.")
                : 42,
            TimeSpan.FromMilliseconds(50));
        Assert.Throws<InvalidOperationException>(() => lazy.Value);

        await Task.Delay(150);

        Assert.Equal(42, lazy.Value);
        Assert.Equal(2, invocationCount);
    }

    [RetryFact]
    public async Task ReleaseFailure_IsCapturedWithoutStoppingFutureResets()
    {
        var releaseCount = 0;
        using var lazy = new TimerLazy<object>(
            () => new object(),
            TimeSpan.FromMilliseconds(50),
            _ =>
            {
                if (Interlocked.Increment(ref releaseCount) == 1)
                    throw new InvalidOperationException("Release failed.");
            });
        _ = lazy.Value;

        await LazyTestHelper.WaitUntilAsync(() => lazy.LastResetException is not null, TimeSpan.FromSeconds(3));

        Assert.IsType<InvalidOperationException>(lazy.LastResetException);
        _ = lazy.Value;
        await LazyTestHelper.WaitUntilAsync(
            () => Volatile.Read(ref releaseCount) >= 2 && lazy.LastResetException is null,
            TimeSpan.FromSeconds(3));
    }

    [RetryFact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task DifferentInstances_DoNotShareTimerCallbackLock()
    {
        using var firstReleaseStarted = new ManualResetEventSlim();
        using var continueFirstRelease = new ManualResetEventSlim();
        using var secondReleased = new ManualResetEventSlim();
        using var first = new TimerLazy<object>(
            () => new object(),
            TimeSpan.FromMilliseconds(50),
            Timeout.InfiniteTimeSpan,
            _ =>
            {
                firstReleaseStarted.Set();
                continueFirstRelease.Wait();
            });
        using var second = new TimerLazy<object>(
            () => new object(),
            TimeSpan.FromMilliseconds(50),
            Timeout.InfiniteTimeSpan,
            _ => secondReleased.Set());
        _ = first.Value;
        _ = second.Value;

        try
        {
            Assert.True(firstReleaseStarted.Wait(TimeSpan.FromSeconds(3)));
            Assert.True(secondReleased.Wait(TimeSpan.FromSeconds(3)));
        }
        finally
        {
            continueFirstRelease.Set();
        }

        await LazyTestHelper.WaitUntilAsync(() => first.IsValueCreated == false, TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ResetAndReplaceValueFactory_ForwardToUnderlyingLazy()
    {
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        using var lazy = new TimerLazy<TrackedDisposable>(
            () => first,
            TimeSpan.FromDays(1),
            value => value.Dispose());
        Assert.Same(first, lazy.Value);

        lazy.Reset();
        Assert.True(first.IsDisposed);
        lazy.ReplaceValueFactory(() => second);

        Assert.Same(second, lazy.Value);
    }

    [Fact]
    public async Task Dispose_IsIdempotentStopsTimerAndPreventsFurtherUse()
    {
        var value = new TrackedDisposable();
        var lazy = new TimerLazy<TrackedDisposable>(
            () => value,
            TimeSpan.FromMilliseconds(50),
            item => item.Dispose());
        _ = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();
        await Task.Delay(100);

        Assert.Equal(1, value.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
        Assert.Throws<ObjectDisposedException>(() => lazy.IsValueCreated);
        Assert.Throws<ObjectDisposedException>(() => lazy.Reset());
        Assert.Throws<ObjectDisposedException>(() => lazy.ReplaceValueFactory(() => new TrackedDisposable()));
    }
}
