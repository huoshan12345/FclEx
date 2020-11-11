using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using MoreLinq;

namespace FclEx
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessages(this Exception ex)
        {
            return ex.InnerException != null ? $"{ex.Message}[{GetAllMessages(ex.InnerException)}]" : ex.Message;
        }

        public static void ReThrow(this Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();

        public static void ThrowInWrapper(this Exception e, string msg) => throw new Exception(msg, e);

        public static Exception GetInnermost(this Exception ex)
        {
            var p = ex;
            while (p.InnerException != null)
            {
                p = p.InnerException;
            }

            return p;
        }

        public static void HandleAll(this Exception ex, Action<Exception> action)
        {
            if (action == null) return;

            var q = new Queue<Exception>();
            q.Enqueue(ex);
            var handled = new HashSet<Exception>();
            while (q.Any())
            {
                var e = q.Dequeue();
                if (e == null) continue;
                else if (e is AggregateException aEx)
                    aEx.InnerExceptions.ForEach(EnqueueIfUnHandled);
                else if (e.InnerException != null)
                    EnqueueIfUnHandled(e.InnerException);
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
                if (exception != null && !handled.Contains(exception))
                    q.Enqueue(exception);
            }
        }
    }
}
