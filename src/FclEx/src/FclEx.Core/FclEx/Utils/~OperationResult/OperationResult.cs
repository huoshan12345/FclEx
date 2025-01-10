using static FclEx.Utils.Operation;

namespace FclEx.Utils;

public readonly struct OperationResult<T> : IOperationResult
{
    public int Code { get; }
    public Exception? Exception { get; }
    public TimeSpan Elapsed { get; }
    public T? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success => Exception is null;

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool Error => Exception is not null;

    /// <summary>
    /// Create an erroneous result
    /// </summary>
    /// <param name="code"></param>
    /// <param name="ex"></param>
    /// <param name="elapsed"></param>
    public OperationResult(int code, Exception ex, TimeSpan elapsed)
    {
        Code = Check.NotEqualTo(code, OperationResultCodes.Success);
        Exception = ex ?? throw new ArgumentNullException(nameof(ex));
        Elapsed = elapsed;
        Value = default;
    }

    /// <summary>
    /// Create an success result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="elapsed"></param>
    public OperationResult(T result, TimeSpan elapsed)
    {
        Code = OperationResultCodes.Success;
        Exception = null;
        Elapsed = elapsed;
        Value = result;
    }

    public static implicit operator OperationResult<T>(Exception ex)
    {
        return CreateError<T>(ex, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>(string? error)
    {
        return CreateError<T>(error, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>((string?, TimeSpan) paras)
    {
        return CreateError<T>(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult<T>((TimeSpan, string?) paras)
    {
        return CreateError<T>(paras.Item2, paras.Item1);
    }

    public static implicit operator OperationResult<T>((Exception, TimeSpan) paras)
    {
        return CreateError<T>(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult<T>((TimeSpan, Exception) paras)
    {
        return CreateError<T>(paras.Item2, paras.Item1);
    }

    public static implicit operator OperationResult<T>(T item)
    {
        return CreateSuccess(item, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>((T, TimeSpan) paras)
    {
        return CreateSuccess(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult(OperationResult<T> result)
    {
        return result.Success
            ? CreateSuccess(result.Elapsed)
            : CreateError(result.Code, result.Exception!, result.Elapsed);
    }
    
    public static implicit operator Task<OperationResult<T>>(OperationResult<T> result)
    {
        return result.ToTask();
    }

    public OperationResult<TDest> ToExplicit<TDest>(Func<T, TDest> func)
    {
        return Success
            ? new OperationResult<TDest>(func(Value)!, Elapsed)
            : new OperationResult<TDest>(Code, Exception!, Elapsed);
    }

    public OperationResult<TDest> ToExplicit<TDest>()
    {
        return ToExplicit(m => m.CastTo<TDest>())!;
    }

    public void Deconstruct(out bool success, out T? value, out Exception? ex, out TimeSpan elapsed)
    {
        success = Success;
        ex = Exception;
        elapsed = Elapsed;
        value = Value;
    }
}