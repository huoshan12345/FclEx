namespace FclEx.Utils;

/// <summary>
/// A generic exception that wraps an object of type T, allowing the exception to carry additional data.
/// </summary>
/// <typeparam name="T">The type of the object being wrapped by this exception.</typeparam>
public class ObjectException<T> : SimpleException
{
    /// <summary>
    /// Gets the object value contained within this exception.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectException{T}"/> class.
    /// </summary>
    /// <param name="obj">The object to be wrapped by this exception.</param>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ObjectException(T obj, string? msg = null, Exception? inner = null)
        : base(msg, inner)
    {
        Value = obj;
    }
}

public static class ObjectException
{
    /// <summary>
    /// Creates a new instance of <see cref="ObjectException{T}"/> with the specified object, message, and inner exception.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="obj">The object to be wrapped by the exception.</param>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    /// <returns>A new ObjectException instance containing the specified object.</returns>
    public static ObjectException<T> Create<T>(T obj, string? msg = null, Exception? inner = null)
        => new(obj, msg, inner);
}