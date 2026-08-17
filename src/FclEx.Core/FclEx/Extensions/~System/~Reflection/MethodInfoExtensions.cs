namespace FclEx.Extensions;

public static class MethodInfoExtensions
{
    private static readonly ConditionalWeakTable<MethodInfo, ValueBox<long>> _runtimeIdentityTags = new();
    private static long _lastRuntimeIdentityTag;

    [MethodImpl(AggressiveInlining)]
    public static bool IsAsync(this MethodInfo method)
    {
        // Obtain the custom attribute for the method.
        // The value returned contains the StateMachineType property.
        // Null is returned if the attribute isn't present for the method.
        return method.IsDefined<AsyncStateMachineAttribute>();
    }

    /// <summary>Gets a concise, human-readable representation of a method name and parameter types.</summary>
    /// <remarks>
    /// The returned text is intended for display only. It is not a unique method identity because it omits generic
    /// arguments, return type, and parameter modifiers. Use <see cref="GetRuntimeIdentityTag"/> for a process-local
    /// identity tag.
    /// </remarks>
    public static string GetSignature(this MethodInfo method)
    {
        var paras = method.GetParameters();
        var name = method.GetFullName();
        var paraNames = paras.Select(m => m.ParameterType.LongName());

        return StringBuilderHelper.Build(m =>
        {
            m.Append(name);

            if (method.IsGenericMethod)
            {
                var genericArgs = method
                    .GetGenericArguments()
                    .Select(x => x.LongName());

                m.AppendAngleBracketed(x => x.AppendJoin(", ", genericArgs));
            }

            m.AppendCurlyBraced(x => x.AppendJoin(", ", paraNames));
        });
    }

    /// <summary>Gets a process-local tag that uniquely identifies this <see cref="MethodInfo"/> object.</summary>
    /// <param name="method">The method metadata object to identify.</param>
    /// <returns>A non-zero tag that remains stable for the lifetime of <paramref name="method"/>.</returns>
    /// <remarks>
    /// Tags are unique only among live <see cref="MethodInfo"/> objects in the current process. They are not stable
    /// across processes and are not metadata identifiers: a separately obtained reflection wrapper for the same
    /// underlying method can receive a different tag.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
    public static long GetRuntimeIdentityTag(this MethodInfo method)
    {
        Check.NotNull(method);
        return _runtimeIdentityTags.GetValue(method, static _ => new(Interlocked.Increment(ref _lastRuntimeIdentityTag))).Value;
    }

    [MethodImpl(AggressiveInlining)]
    public static string GetFullName(this MethodInfo method)
    {
        return method.DeclaringType == null
            ? method.Name
            : $"{method.DeclaringType.LongName()}.{method.Name}";
    }

    [MethodImpl(AggressiveInlining)]
    public static T? Invoke<T>(this MethodInfo method, object? obj, object?[]? parameters)
    {
        return method.Invoke(obj, parameters).CastTo<T>();
    }

    [MethodImpl(AggressiveInlining)]
    public static T? InvokeInstance<T>(this MethodInfo method, object obj, params object?[]? parameters)
    {
        return method.Invoke<T>(obj, parameters);
    }

    [MethodImpl(AggressiveInlining)]
    public static T? InvokeStatic<T>(this MethodInfo method, params object?[]? parameters)
    {
        return method.Invoke<T>(null, parameters);
    }

#if !NET5_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T));
    }

    [MethodImpl(AggressiveInlining)]
    public static T CreateDelegate<T>(this MethodInfo method, object? target) where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T), target);
    }
#endif
}
