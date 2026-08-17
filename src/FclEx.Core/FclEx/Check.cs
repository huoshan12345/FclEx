namespace FclEx;

[DebuggerStepThrough]
[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
public static class Check
{
    [MethodImpl(AggressiveInlining)]
    public static T NotNull<T>([NotNull, NoEnumeration] T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName ?? nameof(value));
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static string NotEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        var name = parameterName ?? nameof(value);
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"The string argument '{name}' cannot be empty.");
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static void HasNoEmptyElements([NotNull] IEnumerable<string?>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);

        if (value.Any(string.IsNullOrEmpty))
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException($"The collection argument '{name}' must not contain any empty elements.");
        }
    }

    [MethodImpl(AggressiveInlining)]
    public static T LessThan<T>(T value, T max, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, max) >= 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value must be less than " + max);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T Between<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, min) < 0 || Comparer<T>.Default.Compare(value, max) > 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, $"The value must be between {min} and {max}.");
        }
        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T NotLessThan<T>(T value, T min, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, min) < 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be less than " + min);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T GreaterThan<T>(T value, T min, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, min) <= 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be less than " + min);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T NotGreaterThan<T>(T value, T max, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(value, max) > 0)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentOutOfRangeException(name, value, "The value cannot be greater than " + max);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T EqualTo<T>(T value, T expected, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (Equals(value, expected) == false)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException("The value should be " + expected, name);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T NotEqualTo<T>(T value, T expected, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (Equals(value, expected))
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException("The value should be " + expected, name);
        }

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T NotNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        return NotLessThan(value, default!, parameterName);
    }

    [MethodImpl(AggressiveInlining)]
    public static T Positive<T>(T value, [CallerArgumentExpression(nameof(value))] string? parameterName = null) where T : IComparable<T>
    {
        return GreaterThan(value, default!, parameterName);
    }

    [MethodImpl(AggressiveInlining)]
    public static void NotEmpty<T>([NotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);

        var empty = false;

        if (value.TryGetNonEnumeratedCount(out var count))
        {
            empty = count == 0;
        }
        else if (value.AnyEx() == false)
        {
            empty = true;
        }

        if (empty)
        {
            var name = parameterName ?? nameof(value);
            throw new ArgumentException($"The list argument '{name}' cannot be empty.");
        }
    }

    [MethodImpl(AggressiveInlining)]
    public static void HasNoNulls<T>([NotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        NotNull(value, parameterName);

        if (value.Any(e => e is null))
        {
            throw new ArgumentException(parameterName ?? nameof(value));
        }
    }

    /// <summary>
    /// Attempts to retrieve the single non-null value from the two given arguments.
    /// </summary>
    /// <typeparam name="T">The type of the input values.</typeparam>
    /// <param name="left">The first value to check.</param>
    /// <param name="right">The second value to check.</param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the single non-null value
    /// from <paramref name="left"/> or <paramref name="right"/>. Otherwise, contains <see langword="default"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if exactly one of <paramref name="left"/> or <paramref name="right"/> is non-null; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException" />
    /// <remarks>
    /// This method enforces that at least one value must be provided.<br/>
    /// If both values are non-null, the method fails without throwing and sets <paramref name="result"/> to <see langword="default"/>.<br/>
    /// If both values are <see langword="null"/>, an <see cref="ArgumentNullException" /> will be thrown.
    /// </remarks>
    public static bool TryGetSingleNonNull<T>(
        [NotNullWhen(false)] T? left,
        [NotNullWhen(false)] T? right,
        [NotNullWhen(true)] out T? result)
    {
        switch (left, right)
        {
            case (null, null): throw new ArgumentNullException(nameof(left), $"{nameof(left)} and {nameof(right)} cannot both be null.");
            case (null, not null):
            {
                result = right;
                return true;
            }
            case (not null, null):
            {
                result = left;
                return true;
            }
            default:
            {
                result = default;
                return false;
            }
        }
    }

    [MethodImpl(AggressiveInlining)]
    internal static void CanCopyTo<T>([NotNull] T[]? array, int arrayIndex, int count)
    {
        NotNull(array);

        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < count)
            throw new ArgumentException(nameof(array));
    }

    [MethodImpl(AggressiveInlining)]
    internal static void VersionEqual(int version, int currentVersion)
    {
        if (version != currentVersion)
            throw new InvalidOperationException();
    }
}
