using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public class ExcuteResult<T> : IExcuteResult<T>
    {
        public bool Successful => Code == ExcuteResultCodes.Success;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;
        public T Result { get; }

        internal ExcuteResult(int code, TimeSpan elapsed, Exception ex)
        {
            Code = Check.NotEqual(code, ExcuteResultCodes.Success, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = elapsed;
            Result = default;
        }

        internal ExcuteResult(T result, TimeSpan elapsed)
        {
            Code = ExcuteResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
            Result = result;
        }

        public static implicit operator ExcuteResult(ExcuteResult<T> result)
        {
            return result.Successful
                ? new ExcuteResult(result.Elapsed)
                : new ExcuteResult(result.Code, result.Elapsed, result.Exception);
        }

        public static implicit operator ExcuteResult<T>(Exception ex)
        {
            return new ExcuteResult<T>(ExcuteResultCodes.FromException, TimeSpan.Zero, ex);
        }

        public static implicit operator ExcuteResult<T>(string error)
        {
            return new ExcuteResult<T>(ExcuteResultCodes.FromString, TimeSpan.Zero, new SimpleException(error));
        }

        public static implicit operator ExcuteResult<T>(T item)
        {
            return item == null
                ? ExcuteResult.CreateError<T>(ExcuteResultCodes.NullData, "结果为空")
                : ExcuteResult.CreateSuccess(item, TimeSpan.Zero);
        }

        public ExcuteResult<TTagert> ToExplicit<TTagert>()
        {
            return Successful 
                ? new ExcuteResult<TTagert>(Result.CastTo<TTagert>(), Elapsed)
                : new ExcuteResult<TTagert>(Code, Elapsed, Exception);
        }
    }
}
