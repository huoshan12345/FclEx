namespace FclEx.Utils;

public interface IOperateResult
{
    /// <summary>
    /// <see cref="OperateResultCodes"/>>
    /// </summary>
    public int Code { get; }
    public Exception? Exception { get; }
    public TimeSpan Elapsed { get; }

    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success { get; }

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool Error { get; }
}