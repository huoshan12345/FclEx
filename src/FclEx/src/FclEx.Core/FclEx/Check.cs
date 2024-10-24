namespace FclEx;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
[DebuggerStepThrough]
public static partial class Check
{
    public static T NotNull<T>([NotNull, NoEnumeration] T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName ?? nameof(value));
        }

        return value;
    }

    public static string NotEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        var name = parameterName ?? nameof(value);
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }

        if (value.Trim().Length == 0)
        {
            throw new ArgumentException($"The string argument {name} cannot be empty.");
        }

        return value;
    }

    public static void HasNoEmptyElements([NotNull] IEnumerable<string?>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);

        if (value.Any(string.IsNullOrEmpty))
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException($"The collection argument {name} must not contain any empty elements.");
        }
    }

    public static T LessThan<T>(T value, T max, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, max) >= 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value must be less than " + max);
        }

        return value;
    }

    public static T NotLessThan<T>(T value, T min, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, min) < 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be less than " + min);
        }

        return value;
    }

    public static T GreaterThan<T>(T value, T min, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, min) <= 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be less than " + min);
        }

        return value;
    }

    public static T NotGreaterThan<T>(T value, T max, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, max) > 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be greater than " + max);
        }

        return value;
    }

    public static T EqualTo<T>(T value, T expected, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (Equals(value, expected) == false)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException("The value should be " + expected, name);
        }

        return value;
    }

    public static T NotEqualTo<T>(T value, T expected, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (Equals(value, expected))
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException("The value should be " + expected, name);
        }

        return value;
    }

    public static T NotNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        return NotLessThan(value, default!, parameterName);
    }

    public static T Positive<T>(T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        return GreaterThan(value, default!, parameterName);
    }

    public static void NotEmpty<T>([NotNull] IReadOnlyCollection<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);
        if (value.Count == 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException($"The list argument {name} cannot be empty.");
        }
    }

    public static void NotEmpty<T>([NotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);
        if (value.AnyExt() == false)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException($"The list argument {name} cannot be empty.");
        }
    }

    public static void HasNoNulls<T>([NotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);

        if (value.Any(e => e == null))
        {
            throw new ArgumentException(parameterName ?? nameof(value));
        }
    }
}
