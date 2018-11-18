using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class ValueTaskExcuteResultExtension
    {
        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Action action)
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static ValueTask<ExcuteResult> Error(this ValueTask<ExcuteResult> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult> Ok(this ValueTask<ExcuteResult> @this, Func<ValueTask> action)
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static ValueTask<ExcuteResult> Error(this ValueTask<ExcuteResult> @this, Func<Exception, ValueTask> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Action<T> action)
        {
            return @this.On(r => r.Success, t => action(t.Result));
        }

        public static ValueTask<ExcuteResult<T>> Error<T>(this ValueTask<ExcuteResult<T>> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Success, t => action(t.Exception));
        }

        public static ValueTask<ExcuteResult<T>> Ok<T>(this ValueTask<ExcuteResult<T>> @this, Func<T, ValueTask> action)
        {
            return @this.On(r => r.Success, t => action(t.Result));
        }

        public static ValueTask<ExcuteResult<T>> Error<T>(this ValueTask<ExcuteResult<T>> @this, Func<Exception, ValueTask> action)
        {
            return @this.On(r => !r.Success, t => action(t.Exception));
        }
    }
}
