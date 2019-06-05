using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public class ReLazyTests
    {
        public class Tester
        {
        }

        public class DisposableTester : IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void Recreate_Test()
        {
            var lazy = new ReLazy<Tester>(() => new Tester());
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Recreate_Dispose_Test(bool dispose)
        {
            var lazy = new ReLazy<DisposableTester>(() => new DisposableTester(), dispose);
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
            Assert.True(newValue.IsDisposed == dispose);
        }
    }
}
