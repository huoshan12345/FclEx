using FclEx.TypeCasters;

namespace FclEx.Extensions;

public static class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CastTo<T>(this object? obj)
    {
        return DynamicTypeCaster.Instance.CastTo<object?, T>(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static TTarget? CastTo<T, TTarget>(this T? obj)
    {
        return ExpressionTypeCaster.Instance.CastTo<T, TTarget>(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringOrEmpty<T>(this T? obj)
    {
        return obj?.ToString() ?? string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeSafely<T>(this T? obj)
    {
        return obj is null ? 0 : obj.GetHashCode();
    }

    public static (string Name, TMember value) GetNamedValue<T, TMember>(this T obj, Expression<Func<T, TMember>> selector)
    {
        var member = ExpressionHelper.GetDataMemberInfo(selector);
        return (member.Name, member.GetValue(obj).CastTo<TMember>())!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CloneByJson<T>(this T? obj, JsonSerializerOptions? options = null)
    {
        return obj is null ? obj : obj.ToJson(options).ToJsonNode(options).Deserialize<T>(options);
    }
}