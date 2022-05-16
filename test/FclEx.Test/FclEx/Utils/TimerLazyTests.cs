using System;
using FclEx.Helpers;
using Xunit;

namespace FclEx.Utils
{
    public class TimerLazyTests
    {
        [Fact]
        public void Recreate_Test()
        {
            var span = TimeSpan.FromMilliseconds(900);

            var lazy = new TimerLazy<ReLazyTests.DisposableTester>(() => new ReLazyTests.DisposableTester(), span);
            Assert.False(lazy.IsValueCreated);
            var value = lazy.Value;
            Assert.NotNull(value);
            Assert.True(lazy.IsValueCreated);

            ThreadHelper.Sleep((int)Math.Ceiling(span.TotalSeconds));
            Assert.False(lazy.IsValueCreated);
            Assert.False(value.IsDisposed);

            var newValue = lazy.Value;
            Assert.True(lazy.IsValueCreated);
            Assert.NotNull(newValue);
            Assert.NotEqual(value, newValue);

            lazy.Dispose();
        }
    }
}
