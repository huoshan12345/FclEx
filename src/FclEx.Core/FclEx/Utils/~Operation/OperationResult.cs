namespace FclEx.Utils;

/// <summary>
/// Represents the result of an operation, including a success/failure indicator,
/// an optional value, an optional exception, and the elapsed time.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public readonly struct OperationResult<T> : IOperationResult<T>
{
    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public TimeSpan Elapsed { get; }

    /// <inheritdoc />
    public T? Value { get; }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => Exception is null;

    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsError => Exception is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{T}"/> struct for a failed operation.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="elapsed">The time elapsed during the operation.</param>
    public OperationResult(Exception ex, TimeSpan elapsed)
    {
        Exception = ex ?? throw new ArgumentNullException(nameof(ex));
        Elapsed = elapsed;
        Value = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{T}"/> struct for a successful operation.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <param name="elapsed">The time elapsed during the operation.</param>
    public OperationResult(T result, TimeSpan elapsed)
    {
        Exception = null;
        Elapsed = elapsed;
        Value = result;
    }

    public static OperationResult<T> FromSuccess(T item, TimeSpan elapsed = default) => new(item!, elapsed);

    public static OperationResult<T> FromError(string? error, TimeSpan elapsed = default) => new(new SimpleException(error), elapsed);

    public static OperationResult<T> FromError(Exception ex, TimeSpan elapsed = default)
    {
        return new OperationResult<T>(ex, elapsed);
    }

    public static implicit operator OperationResult<T>(Exception ex)
    {
        return FromError(ex, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>(string? error)
    {
        return FromError(error, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>((string?, TimeSpan) paras)
    {
        return FromError(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult<T>((TimeSpan, string?) paras)
    {
        return FromError(paras.Item2, paras.Item1);
    }

    public static implicit operator OperationResult<T>((Exception, TimeSpan) paras)
    {
        return FromError(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult<T>((TimeSpan, Exception) paras)
    {
        return FromError(paras.Item2, paras.Item1);
    }

    public static implicit operator OperationResult<T>(T item)
    {
        return FromSuccess(item, TimeSpan.Zero);
    }

    public static implicit operator OperationResult<T>((T, TimeSpan) paras)
    {
        return FromSuccess(paras.Item1, paras.Item2);
    }

    public static implicit operator OperationResult(OperationResult<T> result)
    {
        return result.IsSuccess
            ? OperationResult.FromSuccess(Unit.Default, result.Elapsed)
            : OperationResult.FromError(result.Exception, result.Elapsed);
    }

    public static implicit operator Task<OperationResult<T>>(OperationResult<T> result)
    {
        return Task.FromResult(result);
    }

    /// <summary>
    /// Casts the value of the current <see cref="OperationResult{T}"/> to a new type, producing an <see cref="OperationResult{TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The target type to cast the value to.</typeparam>
    /// <returns>
    /// A new <see cref="OperationResult{TTarget}"/>:
    /// <list type="bullet">
    /// <item>If the current operation was successful, the value is cast to <typeparamref name="TTarget"/> and retained.</item>
    /// <item>If the current operation was an error, the exception is propagated.</item>
    /// </list>
    /// The elapsed time is always preserved.
    /// </returns>
    public OperationResult<TTarget> Cast<TTarget>()
    {
        return IsSuccess
            ? Value is TTarget castValue
                ? Operation.Success(castValue, Elapsed)
                : Operation.Error<TTarget>(new InvalidCastException($"Cannot cast value of type {typeof(T)} to {typeof(TTarget)}.").SetStackTrace(), Elapsed)
            : (Exception, Elapsed);
    }

    /// <summary>
    /// Deconstructs the <see cref="OperationResult{T}"/> into its components.
    /// </summary>
    /// <param name="success">A boolean indicating success or failure.</param>
    /// <param name="value">The value, if successful.</param>
    /// <param name="ex">The exception, if any.</param>
    /// <param name="elapsed">The elapsed time.</param>
    public void Deconstruct(out bool success, out T? value, out Exception? ex, out TimeSpan elapsed)
    {
        success = IsSuccess;
        ex = Exception;
        elapsed = Elapsed;
        value = Value;
    }
}