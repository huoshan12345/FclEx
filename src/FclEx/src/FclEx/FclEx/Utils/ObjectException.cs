namespace FclEx.Utils;

public class ObjectException<T> : SimpleException where T : notnull
{
    public T Value { get; }

    public ObjectException(T obj, string? msg = null, Exception? inner = null)
        : base(msg, inner)
    {
        Value = obj;
    }
}

public static class ObjectException
{
    public static ObjectException<T> Create<T>(T obj, string? msg = null, Exception? inner = null) where T : notnull
        => new(obj, msg, inner);
}