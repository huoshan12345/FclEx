using static System.Text.StringBuilderCache;

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

    public static DisposableValue<StringBuilder> GetPooled(int capacity = 16) // == StringBuilder.DefaultCapacity
    {
        return Acquire(capacity).ToDisposable(Release);
    }
}