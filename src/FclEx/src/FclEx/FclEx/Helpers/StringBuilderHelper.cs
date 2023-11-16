namespace FclEx.Helpers;

public static class StringBuilderHelper
{
    public static string Build(Action<StringBuilder> action)
    {
        using var disposable = CreatePooled();
        var builder = disposable.Value;
        action(builder);
        return builder.ToString();
    }

    public static PooledObject<StringBuilder> CreatePooled()
    {
        return ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
    }
}