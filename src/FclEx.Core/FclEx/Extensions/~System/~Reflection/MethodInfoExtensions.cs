namespace FclEx.Extensions;

public static class MethodInfoExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsAsync(this MethodInfo method)
    {
        // Obtain the custom attribute for the method.
        // The value returned contains the StateMachineType property.
        // Null is returned if the attribute isn't present for the method.
        return method.IsDefined<AsyncStateMachineAttribute>();
    }

    public static string GetSignature(this MethodInfo method)
    {
        var paras = method.GetParameters();
        var name = method.GetFullName();
        var paraNames = paras.Select(m => m.ParameterType.LongName()).JoinWith(",");
        return name + $"({paraNames})";
    }

    [MethodImpl(AggressiveInlining)]
    public static string GetFullName(this MethodInfo method)
    {
        return method.DeclaringType == null
            ? method.Name
            : $"{method.DeclaringType.Namespace}.{method.DeclaringType.ShortName()}.{method.Name}";
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