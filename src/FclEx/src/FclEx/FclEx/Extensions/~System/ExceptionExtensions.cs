using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using FclEx.Utils;
using MoreLinq;

namespace FclEx.Extensions;

public static class ExceptionExtensions
{
    public static void ReThrow(this Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();

    public static Exception GetInnermost(this Exception ex)
    {
        var p = ex;
        while (p.InnerException != null)
        {
            p = p.InnerException;
        }

        return p;
    }

    public static void HandleAll(this Exception? ex, Action<Exception>? action)
    {
        if (ex is null || action is null)
            return;

        var q = new Queue<Exception>();
        q.Enqueue(ex);
        var handled = new HashSet<Exception>();
        while (q.Any())
        {
            var e = q.Dequeue();
            if (e is AggregateException aEx)
            {
                aEx.InnerExceptions.ForEach(EnqueueIfUnHandled);
            }
            else if (e.InnerException is not null)
            {
                EnqueueIfUnHandled(e.InnerException);
            }
            else
            {
                try
                {
                    action(e);
                }
                finally
                {
                    handled.Add(e);
                }
            }
        }
        handled.Clear();

        void EnqueueIfUnHandled(Exception exception)
        {
            if (handled.Contains(exception))
                return;
            q.Enqueue(exception);
        }
    }

    public static Exception Unwrap(this Exception ex)
    {
        if (ex is AggregateException agg)
        {
            agg = agg.Flatten(); // agg does not contain AggregateException now.
            if (agg.InnerExceptions.Count == 1)
                return agg.InnerExceptions[0];
        }
        return ex;
    }

    public static bool IsObjEx<T>([NotNullWhen(true)] this Exception? ex, [NotNullWhen(true)] out T? value) where T : notnull
    {
        if (ex is ObjectException<T> objEx)
        {
            value = objEx.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static bool IsObjEx<T>([NotNullWhen(true)] this Exception? ex, Func<T, bool> condition) where T : notnull
    {
        return ex is ObjectException<T> objEx && condition(objEx.Value);
    }
}