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

    public static (FieldInfo?, PropertyInfo?) GetFieldPropertyPair(this DataMemberInfo member)
    {
        switch (member.MemberInfo)
        {
            case FieldInfo field:
            {
                var autoProperty = field.TryGetAutoProperty(out var auto)
                    ? auto
                    : null;
                return (field, autoProperty);
            }
            case PropertyInfo property:
            {
                var autoField = property.TryGetAutoBackingField(out var auto)
                    ? auto
                    : null;
                return (autoField, property);
            }
            default:
                throw new InvalidOperationException($"Unsupported member type: {member.MemberInfo.MemberType}");
        }
    }
}