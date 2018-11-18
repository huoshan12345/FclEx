using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class TaskExcuteResultExtension
    {
        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Action action)
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static Task<ExcuteResult> Error(this Task<ExcuteResult> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Func<Task> action)
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static Task<ExcuteResult> Error(this Task<ExcuteResult> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Action<T> action)
        {
            return @this.On(r => r.Success, t => action(t.Result));
        }

        public static Task<ExcuteResult<T>> Error<T>(this Task<ExcuteResult<T>> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Success, t => action(t.Exception));
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Func<T, Task> action)
        {
            return @this.On(r => r.Success, t => action(t.Result));
        }

        public static Task<ExcuteResult<T>> Error<T>(this Task<ExcuteResult<T>> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Success, t => action(t.Exception));
        }
    }
}
