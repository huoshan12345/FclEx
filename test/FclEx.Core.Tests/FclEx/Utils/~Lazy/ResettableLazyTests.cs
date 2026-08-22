namespace FclEx.Utils;

public class ResettableLazyTests
{
    [Fact]
    public void Constructor_NullFactory_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ResettableLazy<object>(null!));

        Assert.Equal("valueFactory", exception.ParamName);
    }

    [Fact]
    public void Value_CreatesOnceAndCachesReference()
    {
        var invocationCount = 0;
        using var lazy = new ResettableLazy<object>(() =>
        {
            invocationCount++;
            return new object();
        });

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
        using var lazy = new ResettableLazy<string?>(() =>
        {
            invocationCount++;
            return null;
        });

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
        using var lazy = new ResettableLazy<int>(() =>
        {
            invocationCount++;
            return expected;
        });

        Assert.Equal(expected, lazy.Value);
        Assert.Equal(expected, lazy.Value);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Value_ConcurrentCallers_RunFactoryOnce()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        var invocationCount = 0;
        using var lazy = new ResettableLazy<object>(() =>
        {
            Interlocked.Increment(ref invocationCount);
            factoryStarted.Set();
            continueFactory.Wait();
            return new object();
        });

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() => lazy.Value)).ToArray();
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        continueFactory.Set();
        var values = await Task.WhenAll(tasks);

        Assert.Equal(1, invocationCount);
        Assert.All(values, value => Assert.Same(values[0], value));
    }

    [Fact]
    public void Value_FactoryException_IsCachedUntilReset()
    {
        var expected = new InvalidOperationException("Factory failed.");
        var invocationCount = 0;
        using var lazy = new ResettableLazy<int>(() => ++invocationCount == 1 ? throw expected : 42);

        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => lazy.Value));
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => lazy.Value));
        Assert.Equal(1, invocationCount);
        Assert.False(lazy.IsValueCreated);

        lazy.Reset();

        Assert.Equal(42, lazy.Value);
        Assert.Equal(2, invocationCount);
    }

    [Fact]
    public void Reset_BeforeValueCreation_DoesNotRunFactoryOrReleaseValue()
    {
        var invocationCount = 0;
        var releaseCount = 0;
        using var lazy = new ResettableLazy<int>(() => ++invocationCount, _ => releaseCount++);

        lazy.Reset();

        Assert.False(lazy.IsValueCreated);
        Assert.Equal(0, invocationCount);
        Assert.Equal(0, releaseCount);
        Assert.Equal(1, lazy.Value);
    }

    [Fact]
    public void Reset_ReleasesCreatedValueAndCreatesNewGeneration()
    {
        using var lazy = new ResettableLazy<TrackedDisposable>(
            () => new TrackedDisposable(),
            value => value.Dispose());
        var first = lazy.Value;

        lazy.Reset();

        Assert.True(first.IsDisposed);
        Assert.False(lazy.IsValueCreated);
        var second = lazy.Value;
        Assert.NotSame(first, second);
        Assert.False(second.IsDisposed);
    }

    [Fact]
    public void Reset_NullValue_InvokesReleaseCallback()
    {
        var releaseCount = 0;
        using var lazy = new ResettableLazy<string?>(() => null, value =>
        {
            Assert.Null(value);
            releaseCount++;
        });
        _ = lazy.Value;

        lazy.Reset();

        Assert.Equal(1, releaseCount);
    }

    [Fact]
    public void ReplaceValueFactory_ReleasesValueAndUsesReplacement()
    {
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        using var lazy = new ResettableLazy<TrackedDisposable>(() => first, value => value.Dispose());
        Assert.Same(first, lazy.Value);

        lazy.ReplaceValueFactory(() => second);

        Assert.True(first.IsDisposed);
        Assert.False(lazy.IsValueCreated);
        Assert.Same(second, lazy.Value);
    }

    [Fact]
    public void ReplaceValueFactory_NullFactory_DoesNotInvalidateCurrentValue()
    {
        var expected = new object();
        using var lazy = new ResettableLazy<object>(() => expected);
        _ = lazy.Value;

        var exception = Assert.Throws<ArgumentNullException>(() => lazy.ReplaceValueFactory(null!));

        Assert.Equal("valueFactory", exception.ParamName);
        Assert.Same(expected, lazy.Value);
    }

    [Fact]
    public void ReplaceValueFactory_SameDelegateStillResetsCurrentGeneration()
    {
        var invocationCount = 0;
        Func<int> valueFactory = () => ++invocationCount;
        using var lazy = new ResettableLazy<int>(valueFactory);
        Assert.Equal(1, lazy.Value);

        lazy.ReplaceValueFactory(valueFactory);

        Assert.False(lazy.IsValueCreated);
        Assert.Equal(2, lazy.Value);
    }

    [Fact]
    public async Task Reset_DuringCreation_ReleasesObsoleteValueAndRetries()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        var invocationCount = 0;
        using var lazy = new ResettableLazy<TrackedDisposable>(() =>
        {
            if (Interlocked.Increment(ref invocationCount) == 1)
            {
                factoryStarted.Set();
                continueFactory.Wait();
                return first;
            }

            return second;
        }, value => value.Dispose());

        var valueTask = Task.Run(() => lazy.Value);
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        lazy.Reset();
        continueFactory.Set();

        Assert.Same(second, await valueTask);
        Assert.True(first.IsDisposed);
        Assert.False(second.IsDisposed);
        Assert.Equal(2, invocationCount);
    }

    [Fact]
    public async Task ReplaceValueFactory_DuringCreation_UsesReplacementAndReleasesObsoleteValue()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        var first = new TrackedDisposable();
        var second = new TrackedDisposable();
        using var lazy = new ResettableLazy<TrackedDisposable>(() =>
        {
            factoryStarted.Set();
            continueFactory.Wait();
            return first;
        }, value => value.Dispose());

        var valueTask = Task.Run(() => lazy.Value);
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        lazy.ReplaceValueFactory(() => second);
        continueFactory.Set();

        Assert.Same(second, await valueTask);
        Assert.True(first.IsDisposed);
        Assert.False(second.IsDisposed);
    }

    [Fact]
    public async Task ReplaceValueFactory_DuringFailingCreation_RetriesWithReplacement()
    {
        using var factoryStarted = new ManualResetEventSlim();
        using var continueFactory = new ManualResetEventSlim();
        using var lazy = new ResettableLazy<int>(() =>
        {
            factoryStarted.Set();
            continueFactory.Wait();
            throw new InvalidOperationException("Obsolete factory failed.");
        });

        var valueTask = Task.Run(() => lazy.Value);
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        lazy.ReplaceValueFactory(() => 42);
        continueFactory.Set();

        Assert.Equal(42, await valueTask);
    }

    [Fact]
    public void Value_RecursiveFactoryAccess_ThrowsInsteadOfDeadlocking()
    {
        ResettableLazy<int>? lazy = null;
        lazy = new ResettableLazy<int>(() => lazy!.Value);
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
        var lazy = new ResettableLazy<TrackedDisposable>(() =>
        {
            factoryStarted.Set();
            continueFactory.Wait();
            return value;
        }, item => item.Dispose());

        var valueTask = Task.Run(() => lazy.Value);
        Assert.True(factoryStarted.Wait(TimeSpan.FromSeconds(5)));
        lazy.Dispose();
        continueFactory.Set();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => valueTask);
        Assert.True(value.IsDisposed);
    }

    [Fact]
    public void Dispose_FromFactory_ReleasesProducedValueAndFailsAccess()
    {
        var value = new TrackedDisposable();
        ResettableLazy<TrackedDisposable>? lazy = null;
        lazy = new ResettableLazy<TrackedDisposable>(() =>
        {
            lazy!.Dispose();
            return value;
        }, item => item.Dispose());

        Assert.Throws<ObjectDisposedException>(() => lazy.Value);

        Assert.True(value.IsDisposed);
    }

    [Fact]
    public void Dispose_IsIdempotentReleasesOnceAndPreventsFurtherUse()
    {
        var value = new TrackedDisposable();
        var lazy = new ResettableLazy<TrackedDisposable>(() => value, item => item.Dispose());
        _ = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();

        Assert.Equal(1, value.DisposeCount);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
        Assert.Throws<ObjectDisposedException>(() => lazy.IsValueCreated);
        Assert.Throws<ObjectDisposedException>(() => lazy.Reset());
        Assert.Throws<ObjectDisposedException>(() => lazy.ReplaceValueFactory(() => new TrackedDisposable()));
    }

    [Fact]
    public void Reset_ReleaseFailureLeavesGenerationReset()
    {
        var invocationCount = 0;
        var releaseCount = 0;
        using var lazy = new ResettableLazy<int>(() => ++invocationCount, _ =>
        {
            if (++releaseCount == 1)
                throw new InvalidOperationException();
        });
        Assert.Equal(1, lazy.Value);

        Assert.Throws<InvalidOperationException>(() => lazy.Reset());

        Assert.False(lazy.IsValueCreated);
        Assert.Equal(2, lazy.Value);
    }
}
