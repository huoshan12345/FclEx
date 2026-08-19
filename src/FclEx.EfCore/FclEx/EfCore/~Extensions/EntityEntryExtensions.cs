namespace FclEx.EfCore;

public static class EntityEntryExtensions
{
    /// <summary>
    /// Sets the key values of the entity represented by the <see cref="EntityEntry{T}"/> to match those of another entity, applying a transformation function to each key value.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entry">The EntityEntry representing the entity whose key values are to be set.</param>
    /// <param name="entity">The entity from which to copy the key values.</param>
    /// <param name="transform">A function to transform each key value before setting it on the target entity.</param>
    /// <exception cref="InvalidOperationException">Thrown if the entity type does not have a primary key defined.</exception>
    public static EntityEntry<T> ApplyKeyTo<T>(this EntityEntry<T> entry, T entity, Func<IProperty, object?, object?>? transform = null) where T : class
    {
        var key = entry.Metadata.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{typeof(T).Name} does not have a primary key defined");

        transform ??= (property, value) => value;
        foreach (var property in key.Properties)
        {
            var value = entry.Property(property.Name).CurrentValue;
            var newValue = transform(property, value);
            property.SetValue(entity, newValue);
        }

        return entry;
    }

    /// <summary>
    /// Sets the key values of the entity represented by the <see cref="EntityEntry{T}"/> to match those of another entity, applying the default value for each key property type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entry"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static EntityEntry<T> ApplyKeyToDefault<T>(this EntityEntry<T> entry, T entity) where T : class
    {
        return entry.ApplyKeyTo(entity, (property, value) => property.ClrType.DefaultValue());
    }
}
