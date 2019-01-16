using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class ExcuteResultOfValueTaskExtension
    {
        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Action action)
        {
            return @this.Ok(t => action());
        }

        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static ValueTask<ExcuteResult> Error(this ValueTask<ExcuteResult> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult> ThrowIfError(this ValueTask<ExcuteResult> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Func<ValueTask> action)
        {
            return @this.Ok(t => action());
        }

        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Func<TimeSpan, ValueTask> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static ValueTask<ExcuteResult> Error(this ValueTask<ExcuteResult> @this, Func<Exception, ValueTask> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Action<T> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static ValueTask<ExcuteResult<T>> Error<T>(this ValueTask<ExcuteResult<T>> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult<T>> ThrowIfError<T>(this ValueTask<ExcuteResult<T>> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Func<T, ValueTask> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Func<T, TimeSpan, ValueTask> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static ValueTask<ExcuteResult<T>> Error<T>(this ValueTask<ExcuteResult<T>> @this, Func<Exception, ValueTask> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }
    }
}
