using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace FclEx
{
    public static class EventExtensions
    {
        public static T On<T>(this T t, Func<T, bool> condition, Action<T> action)
        {
            if (condition(t)) action(t);
            return t;
        }

        public static async Task<T> On<T>(this Task<T> t, Func<T, bool> condition, Action<T> action)
        {
            var r = await t.DonotCapture();
            if (condition(r)) action(r);
            return r;
        }

        public static async Task<T> On<T>(this Task<T> t, Func<T, bool> condition, Func<T, Task> action)
        {
            var r = await t.DonotCapture();
            if (condition(r)) await action(r).DonotCapture();
            return r;
        }
    }
}
