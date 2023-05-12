using System;

namespace FclEx.Numbers;

public struct Number<T> where T : struct, IComparable<T>, IEquatable<T>
{
    private readonly T _value;

    public Number(T value)
    {
        _value = value;
    }

    public static implicit operator Number<T>(T value)
    {
        return new Number<T>(value);
    }

    public static explicit operator T(Number<T> value)
    {
        return value._value;
    }
}