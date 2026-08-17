namespace FclEx.Utils;

public class ScopedSetterTests
{
    private class Options
    {
        public bool Enabled { get; set; }
        public int StartCount { get; set; }
    }

    private class RestoreFailureOptions
    {
        private int _failing = 2;

        public bool ThrowWhenRestoring { get; set; }

        public int Failing
        {
            get => _failing;
            set
            {
                if (ThrowWhenRestoring && value == 2)
                    throw new InvalidOperationException("restore failed");

                _failing = value;
            }
        }

        public int Other { get; set; } = 1;
    }

    [Fact]
    public void Set_SingleProperty_ShouldRestoreAfterDispose()
    {
        var options = new Options { Enabled = false };

        using (ScopedSetter.For(options).Set(o => o.Enabled, true))
        {
            Assert.True(options.Enabled); // temporarily overridden
        }

        Assert.False(options.Enabled); // restored
    }

    [Fact]
    public void Set_MultipleProperties_ShouldRestoreAllAfterDispose()
    {
        var options = new Options { Enabled = false, StartCount = 1 };

        using (ScopedSetter.For(options)
            .Set(o => o.Enabled, true)
            .Set(o => o.StartCount, 10))
        {
            Assert.True(options.Enabled);
            Assert.Equal(10, options.StartCount);
        }

        Assert.False(options.Enabled);
        Assert.Equal(1, options.StartCount);
    }

    [Fact]
    public void NestedScopes_ShouldRestoreCorrectly()
    {
        var options = new Options { Enabled = false, StartCount = 1 };

        using (ScopedSetter.For(options).Set(o => o.Enabled, true))
        {
            Assert.True(options.Enabled);

            using (ScopedSetter.For(options).Set(o => o.Enabled, false))
            {
                Assert.False(options.Enabled); // overridden by inner scope
            }

            Assert.True(options.Enabled); // restored to outer scope’s override
        }

        Assert.False(options.Enabled); // fully restored
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldNotThrow()
    {
        var options = new Options { Enabled = false };

        var setter = ScopedSetter.For(options).Set(o => o.Enabled, true);

        setter.Dispose();
        options.Enabled = true;
        setter.Dispose(); // should not throw

        Assert.True(options.Enabled); // second Dispose must not restore again
    }

    [Fact]
    public void Set_SamePropertyTwice_ShouldRestoreOriginalValue()
    {
        var options = new Options { Enabled = false };

        using (ScopedSetter.For(options)
            .Set(o => o.Enabled, true)
            .Set(o => o.Enabled, false))
        {
            Assert.False(options.Enabled); // last override applied
        }

        Assert.False(options.Enabled); // restored to original (false)
    }

    [Fact]
    public void Dispose_Should_Restore_Remaining_Members_When_One_Restore_Fails()
    {
        var options = new RestoreFailureOptions();
        var setter = ScopedSetter.For(options)
            .Set(o => o.Failing, 20)
            .Set(o => o.Other, 10);
        options.ThrowWhenRestoring = true;

        var exception = Assert.Throws<InvalidOperationException>(setter.Dispose);

        Assert.Equal("restore failed", exception.Message);
        Assert.Equal(1, options.Other);
    }

    [Fact]
    public void Set_After_Dispose_Should_Throw()
    {
        var options = new Options();
        var setter = ScopedSetter.For(options);
        setter.Dispose();

        Assert.Throws<ObjectDisposedException>(() => setter.Set(o => o.Enabled, true));
    }
}
