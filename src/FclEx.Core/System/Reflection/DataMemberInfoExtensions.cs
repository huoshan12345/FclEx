namespace System.Reflection;

public static class DataMemberInfoExtensions
{
    public static T? GetValue<T>(this DataMemberInfo member, object? obj)
    {
        return member.GetValue(obj).CastTo<T>();
    }

    public static object GetRequiredValue(this DataMemberInfo member, object? obj)
    {
        return member.GetValue(obj) ?? throw new InvalidOperationException($"The value of member {member.Name} is null");
    }

    public static T GetRequiredValue<T>(this DataMemberInfo member, object? obj)
    {
        return member.GetRequiredValue(obj).CastTo<T>();
    }

    public static Expression ToExpression(this DataMemberInfo info, Expression parameter)
    {
        return info.IsProperty
            ? Expression.Property(parameter, info.MemberInfo.CastTo<PropertyInfo>())
            : Expression.Field(parameter, info.MemberInfo.CastTo<FieldInfo>());
    }
}