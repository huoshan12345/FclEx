namespace FclEx.Utils;

[Serializable]
public class SimpleException : Exception
{
    private readonly bool _noStackTrace;
    private readonly string? _stackTrace;

    [StackTraceHidden]
    public SimpleException(string? msg, bool noStackTrace = true) : this(msg, null, noStackTrace)
    {
    }

    [StackTraceHidden]
    public SimpleException(string? msg, Exception? inner, bool noStackTrace = true) : base(msg, inner)
    {
        _noStackTrace = noStackTrace;

        if (noStackTrace == false)
            _stackTrace = new StackTrace(true).ToString();
    }

    public override string? StackTrace => _noStackTrace ? null : base.StackTrace ?? _stackTrace;
}