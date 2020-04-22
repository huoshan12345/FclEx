using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public readonly struct OperateResult<T> : IOperateResult<T>
    {
        public bool Successful => Exception is null;
        public int Code { get; }
        public Exception? Exception { get; }
        public TimeSpan Elapsed { get; }
        [AllowNull] public T Result { get; }

        public void Deconstruct(out bool successful, out TimeSpan elapsed, out T obj, out Exception? ex)
        {
            successful = Successful;
            ex = Exception;
            elapsed = Elapsed;
            obj = Result;
        }

        /// <summary>
        /// Create an erroneous result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ex"></param>
        /// <param name="elapsed"></param>
        public OperateResult(int code, [DisallowNull]Exception? ex, TimeSpan elapsed)
        {
            Code = Guard.Argument(code, nameof(code)).NotEqual(OperateResultCodes.Success);
            Exception = ex ?? throw new ArgumentNullException(nameof(ex));
            Elapsed = elapsed;
            Result = default;
        }

        /// <summary>
        /// Create an successful result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="elapsed"></param>
        public OperateResult([AllowNull]T result, TimeSpan elapsed)
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
            return r.Successful
                ? new OperateResult(r.Elapsed)
                : new OperateResult(r.Code, r.Exception, r.Elapsed);
        }

        public static implicit operator Task<OperateResult>(OperateResult<T> result)
        {
            return ((OperateResult)result).ToTask();
        }

        public static implicit operator Task<IOperateResult<T>>(OperateResult<T> result)
        {
            return ((IOperateResult<T>)result).ToTask();
        }

        public static implicit operator Task<OperateResult<T>>(OperateResult<T> result)
        {
            return result.ToTask();
        }

        public OperateResult<TTarget> ToExplicit<TTarget>()
        {
            return Successful
                ? new OperateResult<TTarget>(Result!.CastTo<TTarget>(), Elapsed)
                : new OperateResult<TTarget>(Code, Exception!, Elapsed);
        }

        public OperateResult<T> WithElapsed(TimeSpan span)
        {
            return Successful
                ? new OperateResult<T>(Result, span)
                : new OperateResult<T>(Code, Exception!, span);
        }

        void IOperateResult.Deconstruct(out bool successful, out TimeSpan elapsed, out Exception? ex)
        {
            successful = Successful;
            ex = Exception;
            elapsed = Elapsed;
        }

        IOperateResult IOperateResult.WithElapsed(TimeSpan span)
        {
            return WithElapsed(span);
        }
        IOperateResult<T> IOperateResult<T>.WithElapsed(TimeSpan span)
        {
            return WithElapsed(span);
        }
    }
}
