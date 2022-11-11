using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FclEx.Extensions;

public static class ValueTaskExtensions
{
    public static Task WhenAll(this IEnumerable<ValueTask> tasks)
    {
        return tasks.Select(t => t.AsTask()).WhenAll();
    }

    public static Task<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
    {
        return tasks.Select(t => t.AsTask()).WhenAll();
    }

    public static ValueTask<T> ToValueTask<T>(this T obj) => new(obj);

    public static ConfiguredValueTaskAwaitable<T> DonotCapture<T>(this ValueTask<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredValueTaskAwaitable DonotCapture(this ValueTask task)
    {
        return task.ConfigureAwait(false);
    }
}