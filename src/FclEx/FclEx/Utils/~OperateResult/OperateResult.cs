using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dawn;
using static FclEx.Utils.Operate;

namespace FclEx.Utils;

public readonly struct OperateResult<T>
{
    public int Code { get; }
    public Exception? Exception { get; }
    public TimeSpan Elapsed { get; }
    public T? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Successful => Exception is null;

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HasError => Exception is not null;

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
        Value = default;
    }

    /// <summary>
    /// Create an successful result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="elapsed"></param>
    public OperateResult(T result, TimeSpan elapsed)
    {
        Code = OperateResultCodes.Success;
        Exception = null;
        Elapsed = elapsed;
        Value = result;
    }

    public static implicit operator OperateResult<T>(Exception ex)
    {
        return CreateError<T>(ex, TimeSpan.Zero);
    }

    public static implicit operator OperateResult<T>(string? error)
    {
        return CreateError<T>(error, TimeSpan.Zero);
    }

    public static implicit operator OperateResult<T>((string?, TimeSpan) paras)
    {
        return CreateError<T>(paras.Item1, paras.Item2);
    }

    public static implicit operator OperateResult<T>((TimeSpan, string?) paras)
    {
        return CreateError<T>(paras.Item2, paras.Item1);
    }

    public static implicit operator OperateResult<T>((Exception, TimeSpan) paras)
    {
        return CreateError<T>(paras.Item1, paras.Item2);
    }

    public static implicit operator OperateResult<T>((TimeSpan, Exception) paras)
    {
        return CreateError<T>(paras.Item2, paras.Item1);
    }

    public static implicit operator OperateResult<T>(T item)
    {
        return CreateSuccess(item, TimeSpan.Zero);
    }

    public static implicit operator OperateResult<T>((T, TimeSpan) paras)
    {
        return CreateSuccess(paras.Item1, paras.Item2);
    }

    public static implicit operator OperateResult(OperateResult<T> result)
    {
        return result.Successful
            ? CreateSuccess(result.Elapsed)
            : CreateError(result.Code, result.Exception!, result.Elapsed);
    }

    public static implicit operator Task<OperateResult>(OperateResult<T> result)
    {
        return ((OperateResult)result).ToTask();
    }

    public static implicit operator Task<OperateResult<T>>(OperateResult<T> result)
    {
        return result.ToTask();
    }

    public OperateResult<TDest> ToExplicit<TDest>(Func<T, TDest> func)
    {
        return Successful
            ? new OperateResult<TDest>(func(Value)!, Elapsed)
            : new OperateResult<TDest>(Code, Exception!, Elapsed);
    }

    public OperateResult<TDest> ToExplicit<TDest>()
    {
        return ToExplicit(m => m.CastTo<TDest>())!;
    }
}