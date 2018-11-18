using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class ExcuteResultExtensions
    {
        public static ExcuteResult Ok(this ExcuteResult @this, Action action)
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static ExcuteResult Error(this ExcuteResult @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ExcuteResult<T> Ok<T>(this ExcuteResult<T> @this, Action<T> action)
        {
            return @this.On(r => r.Success, t => action(t.Result));
        }

        public static ExcuteResult<T> Error<T>(this ExcuteResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Success, t => action(t.Exception));
        }
    }
}
