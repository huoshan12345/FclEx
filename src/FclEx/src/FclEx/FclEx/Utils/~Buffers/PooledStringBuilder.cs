namespace FclEx.Utils;

public class PooledStringBuilder<TSelf> : IDisposable where TSelf : PooledStringBuilder<TSelf>, new()
{
    protected PooledObject<StringBuilder> _builder = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
    
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    public virtual StringBuilder StringBuilder => _builder.Value;

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