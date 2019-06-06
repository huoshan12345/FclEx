using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public struct OperateResult : IOperateResult<IUnit>
    {
        public bool Successful => Code == OperateResultCodes.Success;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;
        public IUnit Result { get; }

        public OperateResult<TTarget> ToExplicit<TTarget>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult<TTarget>(Code, Exception, Elapsed);
        }

        internal OperateResult(int code, Exception ex, TimeSpan elapsed)
        {
            Code = Check.NotEqual(code, OperateResultCodes.Success, nameof(code));
            Exception = Check.NotNull(ex, nameof(ex));
            Elapsed = elapsed;
            Result = default;
        }

        internal OperateResult(TimeSpan elapsed)
        {
            Code = OperateResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
            Result = default;
        }

        public static implicit operator OperateResult(Exception ex)
        {
            return OperateUtil.CreateError(ex, TimeSpan.Zero);
        }

        public static implicit operator OperateResult(string error)
        {
            return OperateUtil.CreateError(error, TimeSpan.Zero);
        }
    }
}
