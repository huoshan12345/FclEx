using static FclEx.Helpers.ReflectionHelper;

namespace FclEx.Extensions;

public static class PropertyInfoExtensions
{
    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the property getter.
    /// Throws <see cref="MissingMethodException"/> if the getter does not exist.
    /// </summary>
    /// <param name="property">The property metadata.</param>
    /// <returns>The getter <see cref="MethodInfo"/>.</returns>
    public static MethodInfo GetRequiredGetMethod(this PropertyInfo property)
    {
        return property.GetGetMethod(true) ?? throw new MissingMethodException($"No getter in property '{property.Name}'");
    }

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> for the property setter.
    /// Throws <see cref="MissingMethodException"/> if the setter does not exist.
    /// </summary>
    /// <param name="property">The property metadata.</param>
    /// <returns>The setter <see cref="MethodInfo"/>.</returns>
    public static MethodInfo GetRequiredSetMethod(this PropertyInfo property)
    {
        return property.GetSetMethod(true) ?? throw new MissingMethodException($"No setter in property '{property.Name}'");
    }

    /// <summary>
    /// Gets the value of the property and attempts to cast it to <typeparamref name="T"/>.
    /// Returns <see langword="null"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The expected type of the property value.</typeparam>
    /// <param name="property">The property metadata.</param>
    /// <param name="obj">The object instance from which to retrieve the value, or null for static properties.</param>
    /// <returns>The value of the property cast to <typeparamref name="T"/>.</returns>
    public static T? GetValue<T>(this PropertyInfo property, object? obj)
    {
        return property.GetValue(obj).CastTo<T>();
    }

    /// <summary>
    /// Gets the value of the property, throwing <see cref="InvalidOperationException"/> if the value is null.
    /// </summary>
    /// <param name="property">The property metadata.</param>
    /// <param name="obj">The object instance from which to retrieve the value, or null for static properties.</param>
    /// <returns>The non-null value of the property.</returns>
    public static object GetRequiredValue(this PropertyInfo property, object? obj)
    {
        return property.GetValue(obj) ?? throw new InvalidOperationException($"The value of property '{property.Name}' is null");
    }

    /// <summary>
    /// Gets the value of the property, throws <see cref="InvalidOperationException"/> if the value is null,
    /// and casts the result to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the property value.</typeparam>
    /// <param name="property">The property metadata.</param>
    /// <param name="obj">The object instance from which to retrieve the value, or null for static properties.</param>
    /// <returns>The non-null value of the property cast to <typeparamref name="T"/>.</returns>
    public static T GetRequiredValue<T>(this PropertyInfo property, object? obj)
    {
        return property.GetRequiredValue(obj).CastTo<T>();
    }

    /// <summary>
    /// Determines whether the property has a static accessor (getter or setter).
    /// </summary>
    /// <param name="property">The property metadata.</param>
    /// <param name="nonPublic">Whether to include non-public accessors in the check.</param>
    /// <returns><c>true</c> if the property is static; otherwise, <c>false</c>.</returns>
    public static bool IsStatic(this PropertyInfo property, bool nonPublic = false)
    {
        return property.GetAccessors(nonPublic).Any(x => x.IsStatic);
    }

    public static Expression ToExpression(this PropertyInfo property, Expression parameter)
    {
        return Expression.Property(parameter, property);
    }

    private static readonly Type _isExternalInit = typeof(IsExternalInit);

    [MethodImpl(AggressiveInlining)]
    public static bool IsInitOnly(this PropertyInfo property)
    {
        var setter = property.SetMethod;
        if (setter?.ReturnParameter is not { } returnParam)
            return false;

        var mods = returnParam.GetRequiredCustomModifiers();
        return mods.Any(t => t.FullName == _isExternalInit.FullName);
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotVisibleToDerived(this PropertyInfo property)
    {
        var m = property.GetMethod ?? property.SetMethod;
        if (m is null)
            return true;

        // visible: public, protected(Family), protected internal(FamORAssem)
        // not visible: private, internal(Assembly), private protected(FamANDAssem)
        return (m.Attributes & MethodAttributes.MemberAccessMask) <= MethodAttributes.Assembly;
    }

    /// <summary>
    /// Determines whether the specified property is an auto-implemented property.
    /// </summary>
    public static bool IsAutoProperty(this PropertyInfo property)
    {
        return property.TryGetAutoBackingField(out _);
    }

    /// <summary>
    /// Gets the backing field associated with the specified auto-implemented property.
    /// </summary>
    /// <param name="property">The property to inspect.</param>
    /// <param name="field">
    /// When this method returns, contains the backing field if the property is an
    /// auto-implemented property; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the property is auto-implemented and has a backing field;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetAutoBackingField(this PropertyInfo property, [NotNullWhen(true)] out FieldInfo? field)
    {
        field = null;

        var getter = property.GetMethod;
        var setter = property.SetMethod;

        if (getter is null && setter is null)
            return false;

        if (getter?.IsCompilerGenerated() == false
            || setter?.IsCompilerGenerated() == false)
            return false;

        if (property.DeclaringType is not { } type)
            return false;

        var fieldName = GetAutoBackingFieldName(property.Name);
        var f = type.GetField(fieldName, BindingAttributes.Declared);
        if (f is null)
            return false;

        // do not use ReflectionHelper.AccessorAccessesField to check whether the property accesses the field, 
        // because it is not reliable for generic types

        field = f;
        return true;
    }
}