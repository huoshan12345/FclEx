using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public readonly partial struct OperateResult : IOperateResult<Unit>
    {
        private readonly Unit _result;
        public bool Successful => Code == OperateResultCodes.Success;
        public int Code { get; }
        public Exception? Exception { get; }
        public TimeSpan Elapsed { get; }
        Unit IOperateResult<Unit>.Result => _result;

        public void Deconstruct(out bool successful, out TimeSpan elapsed, out Exception? ex)
        {
            successful = Successful;
            ex = Exception;
            elapsed = Elapsed;
        }

        public OperateResult<TTarget> ToExplicit<TTarget>()
        {
            if (Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult<TTarget>(Code, Exception!, Elapsed);
        }

        public OperateResult WithElapsed(TimeSpan span)
        {
            return Successful
                ? new OperateResult(span)
                : new OperateResult(Code, Exception, span);
        }

        void IOperateResult<Unit>.Deconstruct(out bool successful, out TimeSpan elapsed, out Unit obj, out Exception? ex)
        {
            successful = Successful;
            ex = Exception;
            elapsed = Elapsed;
            obj = _result;
        }

        IOperateResult IOperateResult.WithElapsed(TimeSpan span)
        {
            return WithElapsed(span);
        }
        IOperateResult<Unit> IOperateResult<Unit>.WithElapsed(TimeSpan span)
        {
            return WithElapsed(span);
        }

        /// <summary>
        /// Create an erroneous result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ex"></param>
        /// <param name="elapsed"></param>
        public OperateResult(int code, Exception? ex, TimeSpan elapsed)
        {
            Code = Guard.Argument(code, nameof(code)).NotEqual(OperateResultCodes.Success);
            Exception = ex ?? throw new ArgumentNullException(nameof(ex));
            Elapsed = elapsed;
            _result = new Unit();
        }

        /// <summary>
        /// Create an successful result
        /// </summary>
        /// <param name="elapsed"></param>
        public OperateResult(TimeSpan elapsed)
        {
            Code = OperateResultCodes.Success;
            Exception = null;
            Elapsed = elapsed;
            _result = new Unit();
        }

        public static implicit operator OperateResult(Exception ex)
        {
            return CreateError(ex, TimeSpan.Zero);
        }

        public static implicit operator OperateResult(string error)
        {
            return CreateError(error, TimeSpan.Zero);
        }

        public static implicit operator Task<IOperateResult<Unit>>(OperateResult result)
        {
            return ((IOperateResult<Unit>)result).ToTask();
        }

        public static implicit operator Task<OperateResult>(OperateResult result)
        {
            return result.ToTask();
        }
    }
}
