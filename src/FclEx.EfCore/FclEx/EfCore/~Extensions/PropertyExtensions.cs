namespace FclEx.EfCore;

public static class PropertyExtensions
{
    /// <summary>
    /// Gets the value of a property from an entity, using either the property's PropertyInfo or FieldInfo.
    /// </summary>
    /// <param name="property">The property to get the value from.</param>
    /// <param name="entity">The entity instance from which to get the property value.</param>
    /// <returns>The value of the property.</returns>
    public static object? GetValue(this IProperty property, object entity)
    {
        return property.PropertyInfo is { } propertyInfo
            ? propertyInfo.GetValue(entity)
            : property.FieldInfo?.GetValue(entity);
    }

    /// <summary>
    /// Sets the value of a property on an entity, using either the property's PropertyInfo or FieldInfo.
    /// </summary>
    /// <param name="property">The property to set the value for.</param>
    /// <param name="entity">The entity instance on which to set the property value.</param>
    /// <param name="value">The value to set.</param>
    public static void SetValue(this IProperty property, object entity, object? value)
    {
        if (property.PropertyInfo is { } propertyInfo)
        {
            propertyInfo.SetValue(entity, value);
        }
        else
        {
            property.FieldInfo?.SetValue(entity, value);
        }
    }
}
