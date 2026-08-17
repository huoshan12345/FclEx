namespace FclEx.Utils;

/// <summary>
/// A lightweight, immutable reference-type wrapper around a nullable reference type <typeparamref name="T"/>.
/// Useful for storing nullable reference types in APIs that require a reference type, such as
/// <see cref="ConditionalWeakTable{TKey, TValue}"/>, which constrains TValue to <c>class</c>.
/// </summary>
/// <remarks>
/// Instances are immutable by design: once a <see cref="Box{T}"/> is constructed, its
/// <see cref="Value"/> can never change. This makes instances safe to share and read from
/// multiple threads without any locking, and keeps the semantics consistent with the atomic
/// get-or-add behavior of <see cref="ConditionalWeakTable{TKey, TValue}.GetValue"/>.
/// To "update" a value, create a new <see cref="Box{T}"/> instance rather than mutating
/// an existing one.
/// </remarks>
/// <typeparam name="T">The nullable reference type being boxed.</typeparam>
public class Box<T> : IEquatable<Box<T>>
    where T : class
{
    public readonly T? Value;

    public Box(T? value) => Value = value;

    public static implicit operator Box<T>(T? value) => new(value);
    public static implicit operator T?(Box<T> box) => box.Value;

    public override string? ToString() => Value?.ToString();

    private static bool Equals(T? left, T? right) => left?.Equals(right) ?? right is null;

    public bool Equals(Box<T>? other)
    {
        return Equals(Value, other?.Value);
    }

    /// <summary>
    /// Determines equality by comparing wrapped values.
    /// Only compares against other <see cref="Box{T}"/> instances to preserve
    /// the symmetry contract of <see cref="object.Equals(object)"/>.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
               || obj is Box<T> other && Equals(other);
    }

    public bool Equals(T? other) => Equals(Value, other);
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public static bool operator ==(Box<T>? left, Box<T>? right) => Equals(left?.Value, right?.Value);
    public static bool operator !=(Box<T>? left, Box<T>? right) => !(left == right);
    public static bool operator ==(Box<T> left, T? right) => Equals(left.Value, right);
    public static bool operator !=(Box<T> left, T? right) => !(left == right);
    public static bool operator ==(T? left, Box<T> right) => Equals(right.Value, left);
    public static bool operator !=(T? left, Box<T> right) => !(left == right);
}
