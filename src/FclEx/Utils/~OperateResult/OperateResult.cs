using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public readonly partial struct OperateResult : IOperateResult<IUnit>
    {
        public bool Successful => Code == OperateResultCodes.Success;
        public int Code { get; }
        [JsonIgnore]
        public Exception Exception { get; }
        public TimeSpan Elapsed { get; }
        public string Msg => Exception?.Message;
        public string StackTrace => Exception?.StackTrace;
        public IUnit Result { get; }

        public void Deconstruct(out bool successful, out TimeSpan elapsed, out Exception exception)
        {
            successful = Successful;
            exception = Exception;
            elapsed = Elapsed;
        }

        public OperateResult<TTarget> ToExplicit<TTarget>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult<TTarget>(Code, Exception, Elapsed);
        }

        public IOperateResult WithElapsed(TimeSpan span)
        {
            return Successful 
                ? new OperateResult(span) 
                : new OperateResult(Code, Exception, span);
        }

        internal OperateResult(int code, Exception ex, TimeSpan elapsed)
        {
            Code = Guard.Argument(code, nameof(code)).NotEqual(OperateResultCodes.Success);
            Exception = Guard.Argument(ex, nameof(ex)).NotNull();
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
            return CreateError(ex, TimeSpan.Zero);
        }

        public static implicit operator OperateResult(string error)
        {
            return CreateError(error, TimeSpan.Zero);
        }
    }
}
