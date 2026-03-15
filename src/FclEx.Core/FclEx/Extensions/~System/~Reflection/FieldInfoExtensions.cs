using static FclEx.Helpers.ReflectionHelper;

namespace FclEx.Extensions;

public static class FieldInfoExtensions
{
    public static T? GetValue<T>(this FieldInfo field, object? obj)
    {
        return field.GetValue(obj).CastTo<T>();
    }

    public static object GetRequiredValue(this FieldInfo field, object? obj)
    {
        return field.GetValue(obj) ?? throw new InvalidOperationException($"The value of field {field.Name} is null");
    }

    public static T GetRequiredValue<T>(this FieldInfo field, object? obj)
    {
        return field.GetRequiredValue(obj).CastTo<T>();
    }

    public static Expression ToExpression(this FieldInfo field, Expression parameter)
    {
        return Expression.Field(parameter, field);
    }
    /// <summary>
    /// Determines whether the specified <see cref="FieldInfo"/> represents
    /// the compiler-generated storage field of a C# auto-property.
    /// </summary>
    public static bool IsAutoPropertyBackingField(this FieldInfo field)
    {
        return field.TryGetAutoProperty(out _);
    }

    public static bool TryGetAutoProperty(this FieldInfo field, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = null;

        if (field.DeclaringType is not { } type)
            return false;

        if (field.IsCompilerGenerated() == false)
            return false;

        if (Regexes.AutoPropertyBackingField.TryMatch(field.Name, 1, out var name) == false)
            return false;

        var p = type.GetProperty(name, BindingAttributes.Declared);
        if (p is null)
            return false;

        var getter = p.GetMethod;
        var setter = p.SetMethod;

        if (getter?.IsCompilerGenerated() == false
            || setter?.IsCompilerGenerated() == false)
            return false;

        if (getter is not null && AccessorUsesField(getter, field) == false)
            return false;

        if (setter is not null && AccessorUsesField(setter, field) == false)
            return false;

        property = p;
        return true;
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotVisibleToDerived(this FieldInfo field)
    {
        // visible: public, protected(Family), protected internal(FamORAssem)
        // not visible: private, internal(Assembly), private protected(FamANDAssem)
        return (field.Attributes & FieldAttributes.FieldAccessMask) <= FieldAttributes.Assembly;
    }
}