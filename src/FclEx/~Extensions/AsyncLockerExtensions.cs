using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class AsyncLockerExtensions
    {
        public static T Do<T>(this AsyncLocker locker, Func<T> func)
        {
            using (locker.Lock())
            {
                return func();
            }
        }

        public static async Task<T> DoAsync<T>(this AsyncLocker locker, Func<Task<T>> func)
        {
            using (await locker.LockAsync())
            {
                return await func();
            }
        }

        public static void Do(this AsyncLocker locker, Action action)
        {
            using (locker.Lock())
            {
                action();
            }
        }

        public static async Task DoAsync(this AsyncLocker locker, Func<Task> action)
        {
            using (await locker.LockAsync())
            {
                await action();
            }
        }

        public static void DoubleCheckAndDo(this AsyncLocker locker, Func<bool> condition, Action action)
        {
            if (condition() && action != null)
            {
                using (locker.Lock())
                {
                    if (condition())
                    {
                        action();
                    }
                }
            }
        }

        public static async Task DoubleCheckAndDoAsync(this AsyncLocker locker, Func<bool> condition, Func<Task> action)
        {
            if (condition() && action != null)
            {
                using (await locker.LockAsync())
                {
                    if (condition())
                    {
                        await action().DonotCapture();
                    }
                }
            }
        }
    }
}
