namespace FclEx.Utils;

public class ScopedSetterTests
{
    private class Options
    {
        public bool Enabled { get; set; }
        public int StartCount { get; set; }
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
        setter.Dispose(); // should not throw

        Assert.False(options.Enabled); // restored
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
}
