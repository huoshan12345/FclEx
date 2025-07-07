using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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
    public static ScopedSetter<T> For<T>(T obj) => new(obj);
}

/// <summary>
/// Provides a scope-based mechanism for temporarily overriding properties
/// of an object and restoring their original values when the scope ends.
/// </summary>
public class ScopedSetter<T>(T obj) : IDisposable
{
    private readonly T _obj = Check.NotNull(obj);
    private readonly Dictionary<DataMemberInfo, object?> _members = [];

    public void Dispose()
    {
        foreach (var (member, value) in _members)
        {
            member.SetValue(_obj, value);
        }
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
        var member = ExpressionHelper.GetMember(selector).ToDataMemberInfo();
        var value = member.GetValue<TMember>(_obj);
        member.SetValue(_obj, tempValue);
        _members.TryAdd(member, value); // Only save the original value once
        return this;
    }
}
