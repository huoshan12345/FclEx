using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    [Obsolete]
    public class AsyncLockerTests
    {
        [Fact]
        public async Task NoLock_Test()
        {
            var executeTimes = 0;
            const int count = 100000;
            await Enumerable.Range(1, count).Select(m => Task.Run(() => executeTimes++)).WhenAll();
            Assert.True(count > executeTimes);
        }

        [Fact]
        public async Task LockAsync_Test()
        {
            var executeTimes = 0;
            using var locker = new AsyncLocker();
            const int count = 100000;
            await Enumerable.Range(1, count).Select(m => Task.Run(async () =>
            {
                using (await locker.LockAsync())
                {
                    executeTimes++;
                }
            })).WhenAll();

            Assert.Equal(count, executeTimes);
        }

        [Fact]
        public async Task LockAsync_Token_Test()
        {
            var executeTimes = 0;
            using var locker = new AsyncLocker();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
            const int count = 10;
            var tasks = Enumerable.Range(1, count).Select(m => Task.Run(async () =>
            {
                using (await locker.LockAsync(cts.Token))
                {
                    ++executeTimes;
                    await TaskHelper.DelayMilli(100);
                }
            }));
            await Assert.ThrowsAsync<OperationCanceledException>(() => tasks.WhenAll());
            Assert.True(count > executeTimes);
        }

        [Fact]
        public async Task LockAsync_TimeSpan_Test()
        {
            var executeTimes = 0;
            using var locker = new AsyncLocker();
            const int count = 10;
            var tasks = Enumerable.Range(1, count).Select(m => Task.Run(async () =>
            {
                using (await locker.LockAsync(TimeSpan.FromSeconds(1)))
                {
                    ++executeTimes;
                }
            }));
            await tasks.WhenAll(); // no exception here
            Assert.Equal(count, executeTimes);
        }

        [Fact]
        public async Task LockAsync_Disposabled_Test()
        {
            var locker = new AsyncLocker();
            locker.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                using (await locker.LockAsync()) { }
            });
        }

        [Fact]
        public void Lock_Disposabled_Test()
        {
            var locker = new AsyncLocker();
            locker.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                using (locker.Lock()) { }
            });
        }
    }
}
