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

    private static readonly Regex _backingFieldName = new(@"^<(\w+)>k__BackingField$", RegexOptions.Compiled);
    public static bool TryGetAutoProperty(this FieldInfo field, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = null;

        var type = field.DeclaringType;
        if (type is null)
            return false;

        if (_backingFieldName.TryMatch(field.Name, 1, out var propertyName) == false)
            return false;

        property = type.GetProperty(propertyName, false);
        return property is not null;
    }

    public static Expression ToExpression(this FieldInfo field, Expression parameter)
    {
        return Expression.Field(parameter, field);
    }

    private static readonly Regex AutoFieldRegex = new("^<(.+)>k__BackingField$", RegexOptions.Compiled);

    /// <summary>
    /// Determines whether the specified <see cref="FieldInfo"/> represents
    /// the compiler-generated storage field of a C# auto-property.
    /// </summary>
    public static bool IsAutoPropertyBackingField(this FieldInfo field)
    {
        if (field.DeclaringType is not { } type)
            return false;

        if (field.IsCompilerGenerated() == false)
            return false;

        if (AutoFieldRegex.TryMatch(field.Name, 1, out var name) == false)
            return false;

        var property = type.GetProperty(name, BindingAttributes.AllDeclared);
        if (property is null)
            return false;

        return AccessorUsesField(property.GetMethod, field)
               || AccessorUsesField(property.SetMethod, field);
    }

    private static bool AccessorUsesField(MethodInfo? method, FieldInfo field)
    {
        if(method is null)
            return false;

        if(method.IsCompilerGenerated() == false)
            return false;

        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il == null)
            return false;

        var fieldToken = field.MetadataToken;

        for (var i = 0; i < il.Length - 4; i++)
        {
            var op = il[i];

            if (op != 0x7B /* ldfld */ && op != 0x7D /* stfld */) 
                continue;

            var token = BitConverter.ToInt32(il, i + 1);
            if (token == fieldToken)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotVisibleToDerived(this FieldInfo field)
    {
        // visible: public, protected(Family), protected internal(FamORAssem)
        // not visible: private, internal(Assembly), private protected(FamANDAssem)
        return (field.Attributes & FieldAttributes.FieldAccessMask) <= FieldAttributes.Assembly;
    }
}