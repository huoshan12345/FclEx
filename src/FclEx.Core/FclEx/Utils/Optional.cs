namespace FclEx.Utils;

/// <summary>
/// A utility class for working with the <see cref="Optional{T}"/> type.
/// Provides methods to create an "empty" or "some" optional value.
/// </summary>
public static class Optional
{
    /// <summary>
    /// Creates an "empty" <see cref="Optional{T}"/> with no value.
    /// </summary>
    /// <typeparam name="T">The type of the value that would be optionally present.</typeparam>
    /// <returns>An <see cref="Optional{T}"/> that has no value (equivalent to its default value).</returns>
    public static Optional<T> None<T>() => default;

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> containing a value.
    /// </summary>
    /// <typeparam name="T">The type of the value that will be wrapped in the optional.</typeparam>
    /// <param name="value">The value to be wrapped in the optional.</param>
    /// <returns>An <see cref="Optional{T}"/> containing the specified value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Optional<T> Some<T>(T value) => Check.NotNull(value);
}

/// <summary>
/// A lightweight structure representing a non-null value of type <typeparamref name="T"/> that may be absent.
/// </summary>
/// <typeparam name="T">The type of the value that may be optionally present.</typeparam>
[JsonConverter(typeof(OptionalJsonConverter))]
public readonly record struct Optional<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    /// <summary>
    /// Initializes an <see cref="Optional{T}"/> from a value.
    /// </summary>
    /// <param name="value">
    /// The value to store. A <see langword="null"/> value creates an optional with no value.
    /// </param>
    public Optional(T? value)
    {
        _value = value;
        _hasValue = value is not null;
    }

    /// <summary>
    /// Gets or initializes the value. When no value is present, the getter returns the default value of
    /// <typeparamref name="T"/>. Assigning <see langword="null"/> creates an optional with no value.
    /// </summary>
    public T? Value
    {
        get => _value;
        init
        {
            _value = value;
            _hasValue = value is not null;
        }
    }

    /// <summary>
    /// Indicates whether a non-null value is present.
    /// </summary>
    /// <remarks>
    /// If <c>HasValue</c> is <see langword="true"/>, then the <c>Value</c> property is guaranteed to be non-null.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => _hasValue;

    /// <summary>
    /// Deconstructs the optional into its contained value.
    /// </summary>
    /// <param name="value">The contained value, or the default value of <typeparamref name="T"/> when not present.</param>
    public void Deconstruct(out T? value) => value = Value;

    /// <summary>
    /// Implicit conversion from a nullable value <typeparamref name="T?"/> to an <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Optional{T}"/> containing the value.</returns>
    public static implicit operator Optional<T>(T? value) => new(value);

    /// <summary>
    /// Implicit conversion from an <see cref="Optional{T}"/> to a nullable value of type <typeparamref name="T?"/>.
    /// </summary>
    /// <param name="o">The optional value to convert.</param>
    /// <returns>The contained value, or the default value of <typeparamref name="T"/> if not present.</returns>
    public static implicit operator T?(Optional<T> o) => o.Value;
}
