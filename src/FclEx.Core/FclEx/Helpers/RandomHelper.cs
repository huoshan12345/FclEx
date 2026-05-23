namespace FclEx.Helpers;

public static class RandomHelper
{
    private static readonly ThreadLocal<Random> _random = new(() => new Random());
    public static Random Shared => _random.Value!;
}
