namespace FclEx.Extensions;

public static class ConstructorInfoExtensions
{
    public static T Invoke<T>(this ConstructorInfo method, params object?[]? parameters)
    {
        return method.Invoke(parameters).CastTo<T>();
    }
}