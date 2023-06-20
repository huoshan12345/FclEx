namespace FclEx.Extensions;

public static class MemberInfoExtensions
{
    public static bool IsDefined<T>(this MemberInfo memberInfo, bool inherit = true)
    {
        return memberInfo.IsDefined(typeof(T), inherit);
    }

    public static DataMemberInfo ToDataMemberInfo(this MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo propInfo => new DataMemberInfo(propInfo),
            FieldInfo fieldInfo => new DataMemberInfo(fieldInfo),
            _ => throw new ArgumentException(
                $"MemberInfo '{memberInfo.Name}' refers to neither a field nor a property.")
        };
    }

    public static bool IsCompilerGenerated(this MemberInfo memberInfo, bool inherit = true)
    {
        return memberInfo.IsDefined<CompilerGeneratedAttribute>(inherit);
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