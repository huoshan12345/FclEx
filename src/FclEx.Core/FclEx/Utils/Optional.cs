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
    /// <returns>An <see cref="Optional{T}"/> that has no value (equivalent to <c>Optional{T}.None</c>).</returns>
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
/// A lightweight structure representing an optional value of type <typeparamref name="T"/>.
/// This can either contain a value or be considered "empty" if the value is null.
/// </summary>
/// <typeparam name="T">The type of the value that may be optionally present.</typeparam>
public readonly record struct Optional<T>(T? Value)
{
    /// <summary>
    /// Indicates whether the optional value is present (i.e., not null).
    /// </summary>
    /// <remarks>
    /// If <c>HasValue</c> is <see langword="true"/>, then the <c>Value</c> property is guaranteed to be non-null.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

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
    /// <returns>The nullable value contained in the optional, or null if not present.</returns>
    public static implicit operator T?(Optional<T> o) => o.Value;
}
