namespace FclEx.Utils;

public class DisposableTestModel : IDisposable
{
    public bool IsDisposed { get; private set; }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        IsDisposed = true;
    }
}

public class ReLazyTests
{
    public class TestModel;

    [Fact]
    public void Recreate_Test()
    {
        var lazy = new ReLazy<TestModel>(() => new TestModel());
        Assert.False(lazy.IsValueCreated);

        var value = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(value);

        var valueAgain = lazy.Value;
        Assert.Equal(value, valueAgain);

        lazy.Recreate();

        var newValue = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(newValue);
        Assert.NotEqual(value, newValue);

        lazy.Dispose();
    }

    [Fact]
    public void Recreate_Dispose_Test()
    {
        var lazy = new ReLazy<DisposableTestModel>(() => new DisposableTestModel());
        Assert.False(lazy.IsValueCreated);

        var value = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(value);

        var valueAgain = lazy.Value;
        Assert.Equal(value, valueAgain);

        lazy.Recreate();
        Assert.False(value.IsDisposed);

        var newValue = lazy.Value;
        Assert.True(lazy.IsValueCreated);
        Assert.NotNull(newValue);
        Assert.NotEqual(value, newValue);

        lazy.Dispose();
    }

    [Fact]
    public void Dispose_IsIdempotent_And_PreventsFurtherUse()
    {
        var discardedCount = 0;
        var lazy = new ReLazy<DisposableTestModel>(
            () => new DisposableTestModel(),
            discardValueHandler: (_, value) =>
            {
                discardedCount++;
                value.Dispose();
            });
        var value = lazy.Value;

        lazy.Dispose();
        lazy.Dispose();

        Assert.Equal(1, discardedCount);
        Assert.True(value.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => lazy.Value);
        Assert.Throws<ObjectDisposedException>(() => lazy.IsValueCreated);
        Assert.Throws<ObjectDisposedException>(() => lazy.Recreate());
        Assert.Throws<ObjectDisposedException>(() => lazy.SetValueFactory(() => new DisposableTestModel()));
    }
}
