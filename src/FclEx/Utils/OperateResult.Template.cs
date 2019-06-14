using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public struct OperateResult<T> : IOperateResult<T>
    {
        public bool Successful => Code == OperateResultCodes.Success;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;
        public T Result { get; }

        internal OperateResult(int code, Exception ex, TimeSpan elapsed)
        {
            Code = Check.NotEqual(code, OperateResultCodes.Success, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = elapsed;
            Result = default;
        }

        internal OperateResult(T result, TimeSpan elapsed)
        {
            Code = OperateResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
            Result = result;
        }

        public static implicit operator OperateResult<T>(Exception ex)
        {
            return OperateResult.CreateError<T>(ex, TimeSpan.Zero);
        }

        public static implicit operator OperateResult<T>(string error)
        {
            return OperateResult.CreateError<T>(error, TimeSpan.Zero);
        }

        public static implicit operator OperateResult<T>(T item)
        {
            return OperateResult.CreateSuccess(item, TimeSpan.Zero);
        }

        public static implicit operator OperateResult<T>(OperateResult r)
        {
            return r.ToExplicit<T>();
        }

        public static implicit operator OperateResult(OperateResult<T> r)
        {
            if (r.Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult(r.Code, r.Exception, r.Elapsed);
        }

        public OperateResult<TTarget> ToExplicit<TTarget>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult<TTarget>(Code, Exception, Elapsed);
        }
    }
}
