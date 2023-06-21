namespace FclEx.Extensions;

public static class MethodInfoExtensions
{

    public static bool IsAsync(this MethodInfo method)
    {
        // Obtain the custom attribute for the method.
        // The value returned contains the StateMachineType property.
        // Null is returned if the attribute isn't present for the method.
        var attrib = method.GetCustomAttribute<AsyncStateMachineAttribute>();
        return (attrib != null);
    }

    public static string GetSignature(this MethodInfo method)
    {
        var paras = method.GetParameters();
        var name = method.GetFullName();
        var paraNames = paras.Select(m => m.ParameterType.ShortName()).JoinWith(",");
        return name + $"({paraNames})";
    }

    public static string GetFullName(this MethodInfo method)
    {
        return method.DeclaringType == null 
            ? method.Name 
            : $"{method.DeclaringType.Namespace}.{method.DeclaringType.ShortName()}.{method.Name}";
    }

    public static T? Invoke<T>(this MethodInfo method, object? obj, object?[] parameters)
    {
        return method.Invoke(obj, parameters).CastTo<T>();
    }

    public static T? InvokeInstance<T>(this MethodInfo method, object obj, params object?[] parameters)
    {
        return method.Invoke<T>(obj, parameters);
    }

    public static T? InvokeStatic<T>(this MethodInfo method, params object?[] parameters)
    {
        return method.Invoke<T>(null, parameters);
    }
}