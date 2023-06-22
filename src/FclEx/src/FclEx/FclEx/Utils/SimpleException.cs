namespace FclEx.Utils;

public class SimpleException : Exception
{
    public SimpleException(string? msg, bool noStackTrace = false) : this(msg, null, noStackTrace)
    {
    }

    public SimpleException(string? msg, Exception? inner, bool noStackTrace = false) : base(msg, inner)
    {
        StackTrace = noStackTrace
            ? ""
            : new StackTrace(1, true).ToString();
    }

    public override string StackTrace { get; }
}