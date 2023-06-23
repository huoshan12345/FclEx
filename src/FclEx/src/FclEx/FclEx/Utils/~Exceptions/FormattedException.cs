namespace FclEx.Utils;

public class FormattedException : Exception
{
    private readonly Exception _exception;

    public FormattedException(Exception exception)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public override string? StackTrace => _exception.StackTrace;
    public override string Message => _exception.Message;
    public override IDictionary Data => _exception.Data;
    public override string? Source
    {
        get => _exception.Source;
        set => _exception.Source = value;
    }
    public override string? HelpLink 
    { 
        get => _exception.HelpLink; 
        set => _exception.HelpLink = value;
    }
    public override int GetHashCode() => _exception.GetHashCode();
    public override Exception GetBaseException() => _exception.GetBaseException();
    public override void GetObjectData(SerializationInfo info, StreamingContext context) => _exception.GetObjectData(info, context);
    public override bool Equals(object? obj) => _exception.Equals(obj);
    public override string ToString() => _exception.ToFormattedString();
}