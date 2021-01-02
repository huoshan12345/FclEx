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
        [MemberNotNullWhen(true, nameof(Result))]
        [MemberNotNullWhen(false, nameof(Exception))]
#pragma warning disable CS8775 // Member must have a non-null value when exiting in some condition.
        public bool Successful => Exception is null;
#pragma warning restore CS8775 // Member must have a non-null value when exiting in some condition.
        public int Code { get; }
        public Exception? Exception { get; }
        public TimeSpan Elapsed { get; }
        [AllowNull] public T Result { get; }

        public void Deconstruct(out bool successful, out TimeSpan elapsed, [MaybeNull] out T obj, out Exception? ex)
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
        public OperateResult(int code, Exception ex, TimeSpan elapsed)
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
        public OperateResult([DisallowNull] T result, TimeSpan elapsed)
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

        public static implicit operator OperateResult<T>(string? error)
        {
            return OperateResult.CreateError<T>(error, TimeSpan.Zero);
        }

        public static implicit operator OperateResult<T>((string?, TimeSpan) paras)
        {
            return OperateResult.CreateError<T>(paras.Item1, paras.Item2);
        }

        public static implicit operator OperateResult<T>((TimeSpan, string?) paras)
        {
            return OperateResult.CreateError<T>(paras.Item2, paras.Item1);
        }

        public static implicit operator OperateResult<T>((Exception, TimeSpan) paras)
        {
            return OperateResult.CreateError<T>(paras.Item1, paras.Item2);
        }

        public static implicit operator OperateResult<T>((TimeSpan, Exception) paras)
        {
            return OperateResult.CreateError<T>(paras.Item2, paras.Item1);
        }

        public static implicit operator OperateResult<T>(T item)
        {
            return OperateResult.CreateSuccess(item, TimeSpan.Zero);
        }

        public static implicit operator OperateResult<T>((T, TimeSpan) paras)
        {
            return OperateResult.CreateSuccess(paras.Item1, paras.Item2);
        }

        public static implicit operator OperateResult(OperateResult<T> r)
        {
            return r.ToUntyped();
        }

        public static implicit operator OperateResult<T>(OperateResult r)
        {
            return r.ToExplicit<T>();
        }

        public static implicit operator Task<IOperateResult>(OperateResult<T> result)
        {
            return ((IOperateResult)result).ToTask();
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

        public OperateResult<TDest> ToExplicit<TDest>(Func<T, TDest> func)
        {
            return Successful
                ? new OperateResult<TDest>(func(Result)!, Elapsed)
                : new OperateResult<TDest>(Code, Exception!, Elapsed);
        }

        public OperateResult<TDest> ToExplicit<TDest>()
        {
            return ToExplicit(m => m.CastTo<TDest>())!;
        }

        void IOperateResult.Deconstruct(out bool successful, out TimeSpan elapsed, out Exception? ex)
        {
            successful = Successful;
            ex = Exception;
            elapsed = Elapsed;
        }
    }
}
