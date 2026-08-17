namespace FclEx.Utils;

/// <summary>
/// Factory class for creating <see cref="ScopedSetter{T}"/> instances.
/// </summary>
public static class ScopedSetter
{
    /// <summary>
    /// Creates a new <see cref="ScopedSetter{T}"/> for the specified object.
    /// Use within a <c>using</c> block to ensure proper disposal and restoration.
    /// </summary>
    /// <typeparam name="T">The type of the object to modify temporarily.</typeparam>
    /// <param name="obj">The object whose members will be temporarily modified.</param>
    /// <returns>A new <see cref="ScopedSetter{T}"/> instance.</returns>
    public static ScopedSetter<T> For<T>(T obj) where T : class
    {
        return new(obj);
    }
}

/// <summary>
/// Provides a scope-based mechanism for temporarily overriding properties
/// of an object and restoring their original values when the scope ends.
/// </summary>
/// <remarks>Instances are not thread-safe and must not be used concurrently.</remarks>
public class ScopedSetter<T>(T obj) : IDisposable where T : class
{
    private readonly T _obj = Check.NotNull(obj);
    private Dictionary<DataMemberInfo, object?>? _members = [];

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        var members = _members;
        _members = null;
        if (members is null)
            return;

        List<Exception>? exceptions = null;
        foreach (var (member, value) in members)
        {
            try
            {
                member.SetValue(_obj, value);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                (exceptions ??= []).Add(exception.InnerException);
            }
            catch (Exception exception)
            {
                (exceptions ??= []).Add(exception);
            }
        }

        if (exceptions is { Count: 1 })
        {
            exceptions[0].ReThrow();
            return;
        }

        if (exceptions is { Count: > 1 })
            throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Temporarily overrides a property or field of the target object.
    /// The original value is saved and will be restored when <see cref="Dispose"/> is called.
    /// </summary>
    /// <typeparam name="TMember">The type of the member being set.</typeparam>
    /// <param name="selector">
    /// An expression selecting the member to modify, e.g. <c>x => x.Property</c>.
    /// </param>
    /// <param name="tempValue">
    /// The temporary value to assign to the member for the lifetime of this scope.
    /// </param>
    /// <returns>The current <see cref="ScopedSetter{T}"/> instance, to allow fluent chaining.</returns>
    public ScopedSetter<T> Set<TMember>(Expression<Func<T, TMember>> selector, TMember tempValue)
    {
        var members = _members;
        if (members is null)
            throw new ObjectDisposedException(GetType().Name);

        var member = ExpressionHelper.GetMember(selector).ToDataMemberInfo();
        var value = member.GetValue<TMember>(_obj);
        member.SetValue(_obj, tempValue);
        members.TryAdd(member, value); // Only save the original value once

        return this;
    }
}
