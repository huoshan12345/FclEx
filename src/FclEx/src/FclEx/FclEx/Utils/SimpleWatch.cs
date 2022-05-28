using System;
using System.Threading.Tasks;
using FclEx.Extensions;

namespace FclEx.Utils;

public static class SimpleWatch
{
    public static TimeSpan Do(Action action)
    {
        var watch = ValueStopwatch.StartNew();
        action();
        return watch.GetElapsedTime();
    }

    public static async Task<TimeSpan> DoAsync(Func<Task> action)
    {
        var watch = ValueStopwatch.StartNew();
        await action().DonotCapture();
        return watch.GetElapsedTime();
    }

    public static (T Ret, TimeSpan TimeSpan) Do<T>(Func<T> action)
    {
        var watch = ValueStopwatch.StartNew();
        var ret = action();
        return (ret, watch.GetElapsedTime());
    }

    public static async Task<(T Ret, TimeSpan TimeSpan)> DoAsync<T>(Func<Task<T>> action)
    {
        var watch = ValueStopwatch.StartNew();
        var ret = await action().DonotCapture();
        return (ret, watch.GetElapsedTime());
    }
}