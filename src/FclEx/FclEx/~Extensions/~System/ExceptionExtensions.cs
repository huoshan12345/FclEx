using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using MoreLinq;

namespace FclEx
{
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
    }
}
