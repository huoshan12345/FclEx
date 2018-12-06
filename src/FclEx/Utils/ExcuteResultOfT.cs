using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public class ExcuteResult<T> : IExcuteResult<T>
    {
        public bool Successful => Code == 0;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;
        public T Result { get; }

        internal ExcuteResult(int code, Exception ex)
        {
            Code = Check.NotEqual(code, 0, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = default;
            Result = default;
        }

        internal ExcuteResult(T result, TimeSpan elapsed)
        {
            Code = 0;
            Exception = null;
            Elapsed = elapsed;
            Result = result;
        }

        public static implicit operator ExcuteResult(ExcuteResult<T> result)
        {
            return result.Successful
                ? new ExcuteResult(result.Elapsed)
                : new ExcuteResult(result.Code, result.Exception);
        }

        public static implicit operator ExcuteResult<T>(ExcuteResult result)
        {
            return result.ToExplicit<T>();
        }

        public static implicit operator ExcuteResult<T>(Exception ex)
        {
            return new ExcuteResult<T>(-1, ex);
        }

        public static implicit operator ExcuteResult<T>(string error)
        {
            return new ExcuteResult<T>(-1, new SimpleException(error));
        }

        public static implicit operator ExcuteResult<T>(T item)
        {
            return item == null
                ? ExcuteResult.CreateError<T>(-1, "结果为空")
                : ExcuteResult.CreateSuccess(item, TimeSpan.Zero);
        }
    }
}
