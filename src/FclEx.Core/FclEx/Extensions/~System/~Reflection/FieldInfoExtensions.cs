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

    /// <summary>
    /// Determines whether the specified <see cref="FieldInfo"/> represents
    /// the compiler-generated storage field of a C# auto-property.
    /// 
    /// <para>
    /// Detection is performed using accessor IL analysis rather than
    /// field name matching (e.g. "&lt;X&gt;k__BackingField").
    /// </para>
    /// 
    /// <para>
    /// A field is considered an auto-property backing field iff:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The associated property getter and setter exist</description></item>
    /// <item><description>Both accessors are compiler-generated</description></item>
    /// <item><description>Getter performs exactly one <c>ldfld</c></description></item>
    /// <item><description>Setter performs exactly one <c>stfld</c></description></item>
    /// <item><description>Both operate on the same field</description></item>
    /// </list>
    /// 
    /// <para>
    /// This avoids relying on compiler-specific naming conventions
    /// and remains stable under obfuscation and AOT scenarios.
    /// </para>
    /// </summary>
    public static bool IsAutoPropertyBackingField(this FieldInfo field)
    {
        if (field.DeclaringType is not { } type)
            return false;

        // NOTE:
        // We intentionally DO NOT detect auto-property backing fields by name
        // (e.g. "<PropertyName>k__BackingField").
        //
        // Although this is the naming convention currently emitted by Roslyn,
        // field names are NOT part of the CLI specification and therefore:
        // 
        // 1. Not guaranteed by ECMA-335
        // 2. Compiler-dependent (other C# compilers may differ)
        // 3. Not stable under obfuscation / AOT / trimming
        // 4. Potentially localized or rewritten by post-processors
        //
        // In contrast, the IL pattern for auto-property accessors IS stable:
        //
        // Getter:
        //     ldarg.0
        //     ldfld <field>
        //     ret
        //
        // Setter / init:
        //     ldarg.0
        //     ldarg.1
        //     stfld <field>
        //     ret
        //
        // Where both accessors operate on the SAME compiler-generated field.
        //
        // This access pattern is guaranteed by the language lowering rules,
        // making it a reliable metadata-level indicator that the field is
        // the storage of an auto-property.
        //
        // Therefore, detection is performed by analyzing accessor IL rather
        // than relying on implementation-specific naming conventions.
        var backingFields = ReflectionHelper.GetAutoPropertyBackingFields(type);
        return backingFields.Contains(field);
    }
}