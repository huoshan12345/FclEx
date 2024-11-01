namespace FclEx.Extensions;

public static partial class NumberExtensions
{
#if NET7_0_OR_GREATER
    public static T RoundUp<T>(this T number, T @base) where T : INumber<T>
    {
        Check.NotLessThan(number, T.Zero);
        Check.GreaterThan(@base, T.Zero);

        var remaining = number % @base;
        return remaining == T.Zero
            ? number
            : number + (@base - remaining);
    }
#endif
}