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
    /// Returns <c>null</c> if the value is null.
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
    /// <param name="source">The property metadata.</param>
    /// <param name="nonPublic">Whether to include non-public accessors in the check.</param>
    /// <returns><c>true</c> if the property is static; otherwise, <c>false</c>.</returns>
    public static bool IsStatic(this PropertyInfo source, bool nonPublic = false)
    {
        return source.GetAccessors(nonPublic).Any(x => x.IsStatic);
    }
}