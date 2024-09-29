namespace FclEx.Helpers;

public static class StringBuilderHelper
{
    public static string Build(Action<StringBuilder> action)
    {
        using var disposable = GetPooled();
        var builder = disposable.Value;
        action(builder);
        return builder.ToString();
    }

    public static PooledObject<StringBuilder> GetPooled()
    {
        return ObjectPoolHelper.StringBuilderPool.GetPooled();
    }
}