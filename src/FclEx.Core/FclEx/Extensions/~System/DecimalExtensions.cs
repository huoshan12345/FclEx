namespace FclEx.Extensions;

public static class DecimalExtensions
{
    public static int[] GetBits(this decimal value)
    {
        return decimal.GetBits(value);
    }
}