namespace FclEx.Utils;

/// <summary>
/// When you want to extend a StringBuilder for use in a specific scenario but don't want to pollute the global namespace,
/// you can use this class or inherit from this class and add expansion methods to it.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public class PooledStringBuilder<TSelf> : IDisposable where TSelf : PooledStringBuilder<TSelf>, new()
{
    protected readonly DisposableValue<StringBuilder> _builder = StringBuilderHelper.GetCached();

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    public virtual StringBuilder Builder => _builder.Value;

    public override string ToString() => _builder.Value.ToString();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _builder.Dispose();
    }

    public static string Build(Action<TSelf> action)
    {
        using var builder = new TSelf();
        action(builder);
        return builder.ToString();
    }
}