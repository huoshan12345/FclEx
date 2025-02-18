namespace FclEx.Utils;

public class FormattedException : Exception
{
    public Exception Exception { get; }

    public FormattedException(Exception exception) : base(exception.Message, exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public override string ToString() => Exception.ToFormattedString();
}