namespace FclEx.Utils;

/// <summary>
/// A lightweight, immutable reference-type wrapper around a value type <typeparamref name="T"/>.
/// Useful for storing value types in APIs that require a reference type, such as
/// <see cref="ConditionalWeakTable{TKey, TValue}"/>, which constrains TValue to <c>class</c>.
/// </summary>
/// <remarks>
/// Instances are immutable by design: once a <see cref="ValueBox{T}"/> is constructed, its
/// <see cref="Value"/> can never change. This makes instances safe to share and read from
/// multiple threads without any locking, and keeps the semantics consistent with the atomic
/// get-or-add behavior of <see cref="ConditionalWeakTable{TKey, TValue}.GetValue"/>.
/// To "update" a value, create a new <see cref="ValueBox{T}"/> instance rather than mutating
/// an existing one.
/// </remarks>
/// <typeparam name="T">The value type being boxed.</typeparam>
public sealed class ValueBox<T> : IEquatable<ValueBox<T>>
    where T : struct
{
    public readonly T Value;

    public ValueBox(T value) => Value = value;

    public static implicit operator ValueBox<T>(T value) => new(value);
    public static implicit operator T(ValueBox<T> box) => box.Value;

    public override string? ToString() => Value.ToString();

    private static bool Equals(T? left, T? right) => left?.Equals(right) ?? right is null;

    public bool Equals(ValueBox<T>? other)
    {
        return Value.Equals(other?.Value);
    }

    /// <summary>
    /// Determines equality by comparing wrapped values.
    /// Only compares against other <see cref="ValueBox{T}"/> instances to preserve
    /// the symmetry contract of <see cref="object.Equals(object)"/>.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
               || obj is ValueBox<T> other && Equals(other);
    }

    public bool Equals(T other) => Value.Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ValueBox<T>? left, ValueBox<T>? right) => Equals(left?.Value, right?.Value);
    public static bool operator !=(ValueBox<T>? left, ValueBox<T>? right) => !(left == right);
    public static bool operator ==(ValueBox<T> left, T right) => left.Value.Equals(right);
    public static bool operator !=(ValueBox<T> left, T right) => !(left == right);
    public static bool operator ==(T left, ValueBox<T> right) => right.Value.Equals(left);
    public static bool operator !=(T left, ValueBox<T> right) => !(left == right);
}
