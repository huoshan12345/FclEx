using System.Threading.Tasks;
using Nito.AsyncEx;

namespace FclEx.Extensions;

public static class AsyncLockExtensions
{
    public static T Do<T>(this AsyncLock locker, Func<T> func)
    {
        using (locker.Lock())
        {
            return func();
        }
    }

    public static async Task<T> DoAsync<T>(this AsyncLock locker, Func<Task<T>> func)
    {
        using (await locker.LockAsync())
        {
            return await func();
        }
    }

    public static void Do(this AsyncLock locker, Action action)
    {
        using (locker.Lock())
        {
            action();
        }
    }

    public static async Task DoAsync(this AsyncLock locker, Func<Task> action)
    {
        using (await locker.LockAsync())
        {
            await action();
        }
    }

    public static void DoubleCheckAndDo(this AsyncLock locker, Func<bool> condition, Action action)
    {
        if (condition())
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

    public static async Task DoubleCheckAndDoAsync(this AsyncLock locker, Func<bool> condition, Func<Task> action)
    {
        if (condition())
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