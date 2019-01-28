using System;
using System.Threading.Tasks;
using FclEx.Http.Event;

namespace FclEx.Http
{
    public static class ActionEventTaskExtensions
    {
        public static async Task<ActionEvent> Ok(this Task<ActionEvent> @this, Action<object> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsOk) action(result.Target);
            return result;
        }

        public static async Task<ActionEvent> Ok(this Task<ActionEvent> @this, Func<object, Task> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsOk) await action(result.Target);
            return result;
        }

        public static async Task<ActionEvent<T>> Ok<T>(this Task<ActionEvent<T>> @this, Action<T> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsOk) action(result.Result);
            return result;
        }

        public static async Task<ActionEvent<T>> Ok<T>(this Task<ActionEvent<T>> @this, Func<T, Task> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsOk) await action(result.Result);
            return result;
        }

        public static async Task<ActionEvent> Error(this Task<ActionEvent> @this, Action<Exception> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsError) action(result.Exception);
            return result;
        }

        public static async Task<ActionEvent> Error(this Task<ActionEvent> @this, Func<Exception, Task> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsError) await action(result.Exception);
            return result;
        }

        public static async Task<ActionEvent<T>> Error<T>(this Task<ActionEvent<T>> @this, Action<Exception> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsError) action(result.Exception);
            return result;
        }

        public static async Task<ActionEvent<T>> Error<T>(this Task<ActionEvent<T>> @this, Func<Exception, Task> action)
        {
            var result = await @this.DonotCapture();
            if (result.IsError) await action(result.Exception);
            return result;
        }

        public static async Task<ActionEvent<T>> ToExplicit<T>(this Task<ActionEvent> @this)
        {
            var result = await @this.DonotCapture();
            return result.ToExplicit<T>();
        }
    }
}
