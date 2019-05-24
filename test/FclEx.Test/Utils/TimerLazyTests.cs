using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Helpers;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public class TimerLazyTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Recreate_Test(bool dispose)
        {
            var span = TimeSpan.FromMilliseconds(900);

            var lazy = new TimerLazy<ReLazyTests.DisposableTester>(() => new ReLazyTests.DisposableTester(), span, dispose);
            Assert.False(lazy.IsValueCreated);
            var value = lazy.Value;
            Assert.NotNull(value);
            Assert.True(lazy.IsValueCreated);

            ThreadHelper.Sleep((int)Math.Ceiling(span.TotalSeconds));
            Assert.False(lazy.IsValueCreated);
            Assert.True(value.IsDisposed == dispose);

            var newValue = lazy.Value;
            Assert.True(lazy.IsValueCreated);
            Assert.NotNull(newValue);
            Assert.NotEqual(value, newValue);

            lazy.Dispose();
            Assert.True(value.IsDisposed == dispose);
        }
    }
}
