using System.Reflection;

namespace FclEx.Extensions;

public static class FieldInfoExtensions
{
    public static T? GetValue<T>(this FieldInfo info, object? obj)
    {
        var value = info.GetValue(obj);
        return value is null ? default : (T)value;
    }
}