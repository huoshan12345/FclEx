namespace FclEx.Extensions;

public static partial class NumberExtensions
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Rounds up the specified <paramref name="number"/> to the nearest multiple of <paramref name="factor"/>.
    /// </summary>
    /// <typeparam name="T">The numeric type of the parameters, which implements <see cref="INumber{T}"/>.</typeparam>
    /// <param name="number">The number to be rounded up.</param>
    /// <param name="factor">The factor to which <paramref name="number"/> is rounded up.</param>
    /// <returns>The smallest multiple of <paramref name="factor"/> that is greater than or equal to <paramref name="number"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="number"/> or <paramref name="factor"/> is less than zero.</exception>
    public static T RoundUpTo<T>(this T number, T factor) where T : INumber<T>
    {
        Check.NotLessThan(number, T.Zero);
        Check.GreaterThan(factor, T.Zero);

        var remaining = number % factor;
        return remaining == T.Zero
            ? number
            : number + (factor - remaining);
    }

    /// <summary>
    /// Calculates the absolute difference between two numbers of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements <see cref="INumber{T}"/>.</typeparam>
    /// <param name="value">The first number.</param>
    /// <param name="other">The second number to compare with.</param>
    /// <returns>The absolute difference between <paramref name="value"/> and <paramref name="other"/>.</returns>
    public static T AbsDiff<T>(this T value, T other) where T : INumber<T>
    {
        return value > other
            ? value - other
            : other - value;
    }
#endif
}